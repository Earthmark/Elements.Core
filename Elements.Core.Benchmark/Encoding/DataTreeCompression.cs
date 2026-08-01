using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;

namespace Elements.Core.Benchmark.Encoding;

[MemoryDiagnoser]
[Config(typeof(PayloadSizeConfig))]
public class DataTreeCompression
{
    public class PayloadSizeConfig : ManualConfig
    {
        public PayloadSizeConfig()
        {
            AddColumn(new PayloadSizeColumn());
        }
    }

    public class PayloadSizeColumn : IColumn
    {
        readonly Dictionary<(string, DataTreeConverter.Compression), int> _compressionSizeCache = new();

        public string Id => nameof(PayloadSizeColumn);
        public string ColumnName => "Payload Size";
        public bool AlwaysShow => true;
        public ColumnCategory Category => ColumnCategory.Custom;
        public int PriorityInCategory => 0;
        public bool IsNumeric => true;
        public UnitType UnitType => UnitType.Size;
        public string Legend => "Size of the binary payload";

        public bool IsAvailable(Summary summary) => true;
        public bool IsDefault(Summary summary, BenchmarkCase benchmarkCase) => false;

        public string GetValue(Summary summary, BenchmarkCase benchmarkCase) =>
            GetValue(summary, benchmarkCase, SummaryStyle.Default);

        public string GetValue(Summary summary, BenchmarkCase benchmarkCase, SummaryStyle style)
        {
            if (benchmarkCase.Parameters["Tree"] is not TreeCase tree ||
                benchmarkCase.Parameters["Compression"] is not DataTreeConverter.Compression compression)
                return "ERR";

            if (!_compressionSizeCache.TryGetValue((tree.Name, compression), out var size))
            {
                using var stream = new MemoryStream();
                WriterFor(compression)(tree.Tree, stream);
                _compressionSizeCache[(tree.Name, compression)] = size = stream.ToArray().Length;
            }

            return size.ToString("N0");
        }
    }

    [ParamsSource(nameof(Trees))] public TreeCase Tree { get; set; }
    public IEnumerable<TreeCase> Trees =>
        [new("Deeply Nested", DeeplyNestedTree()), new("Kitchen Sink", BuildSampleTree())];

    public record TreeCase(string Name, DataTreeDictionary Tree)
    {
        public override string ToString() => Name;
    }

    [ParamsSource(nameof(SupportedCompressions))]
    public DataTreeConverter.Compression Compression { get; set; }

    public IEnumerable<DataTreeConverter.Compression> SupportedCompressions =>
    [
        DataTreeConverter.Compression.Brotli,
        DataTreeConverter.Compression.LZ4,
        DataTreeConverter.Compression.LZMA,
        DataTreeConverter.Compression.None,
    ];

    static DataTreeDictionary DeeplyNestedTree()
    {
        const int DEPTH = 125;
        var root = new DataTreeDictionary();
        var current = root;

        for (long i = 0; i < DEPTH; i++)
        {
            var child = new DataTreeDictionary { { "Depth", i } };

            current.Add("Child", child);
            current = child;
        }

        return root;
    }

    static DataTreeDictionary BuildSampleTree()
    {
        // Commented types do not come through as the same type, ints are promoted to longs, floats to bools.
        return new DataTreeDictionary
        {
            { "Bool", true },
            //{ "Int", 42 },
            { "Long", -9000000000L },
            //{ "Float", 1.5f },
            { "Double", -2.25 },
            { "String", "sample" },
            { "NullString", null as string },
            { "Url", new Uri("https://example.com/asset") },
            { "Enum", DataTreeConverter.Compression.LZMA },
            //{ "float3", new float3(1f, 2f, 3f) },
            { "double3", new double3(1.0, 2.0, 3.0) },
            {
                "List", new DataTreeList
                {
                    new DataTreeValue(1L),
                    DataTreeValue.RawString("two"),
                    new DataTreeValue(null as string),
                    new DataTreeDictionary { { "Nested", "value" } },
                    new DataTreeList { new DataTreeValue(3.5) }
                }
            },
            {
                "Child", new DataTreeDictionary
                {
                    { "ChildKey", "childValue" },
                    { "ChildList", new DataTreeList() }
                }
            }
        };
    }

    static Action<DataTreeDictionary, Stream> WriterFor(DataTreeConverter.Compression compression)
    {
        return compression switch
        {
            DataTreeConverter.Compression.LZ4 => DataTreeConverter.ToLZ4BSON,
            DataTreeConverter.Compression.LZMA => DataTreeConverter.To7zBSON,
            DataTreeConverter.Compression.Brotli => (root, stream) => DataTreeConverter.ToBRSON(root, stream),
            DataTreeConverter.Compression.None => DataTreeConverter.ToRawBSON,
            _ => throw new NotSupportedException("No writer for " + compression)
        };
    }

    static Func<Stream, DataTreeDictionary> ReaderFor(DataTreeConverter.Compression compression)
    {
        return compression switch
        {
            DataTreeConverter.Compression.LZ4 => DataTreeConverter.FromRawLZ4BSON,
            DataTreeConverter.Compression.LZMA => DataTreeConverter.FromRaw7zBSON,
            DataTreeConverter.Compression.Brotli => DataTreeConverter.FromRawBRSON,
            DataTreeConverter.Compression.None => DataTreeConverter.FromRawBSON,
            _ => throw new NotSupportedException("No reader for " + compression)
        };
    }

    byte[] _cachedPayload;

    [GlobalSetup]
    public void GlobalSetup()
    {
        var payload = Serialize();
        // Offset the payload to remove the header.
        // Bson saving doesn't record a header, skip the offsetting in that case.
        _cachedPayload = Compression != DataTreeConverter.Compression.None ? payload[9..] : payload;
    }

    [Benchmark]
    public byte[] Serialize()
    {
        using var stream = new MemoryStream();
        WriterFor(Compression)(Tree.Tree, stream);
        return stream.ToArray();
    }

    [Benchmark]
    public DataTreeDictionary Deserialize()
    {
        using var deserializeStream = new MemoryStream(_cachedPayload);
        return ReaderFor(Compression)(deserializeStream);
    }
}
