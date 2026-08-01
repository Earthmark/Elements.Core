using System;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using System.IO;
using System.IO.Compression;
using LZ4;
using BrotliStream = BrotliSharpLib.BrotliStream;

namespace Elements.Core;

public static class DataTreeConverter
{
    public static readonly bool NativeBrotliSupported;

    static DataTreeConverter()
    {
        try
        {
            var version = Brotli.Brolib.BrotliDecoderVersion();
            version = Brotli.Brolib.BrotliEncoderVersion();

            NativeBrotliSupported = true;
        }
        catch (Exception ex)
        {
            UniLog.Warning($"Exception from calling native Brotli methods:\n{ex}");
        }
    }

    #region Headers

    public const string HEADER = "FrDT";
    public const int VERSION = 0;

    static void WriteHeader(Stream stream, Compression compression)
    {
        using var writer = new BinaryWriter(stream, Encoding.UTF8, true);
        for (int i = 0; i < HEADER.Length; i++)
            writer.Write((byte)HEADER[i]);

        writer.Write(VERSION);
        writer.WriteEnumBinary(compression);
    }

    static bool TryReadHeader(Stream stream, out Compression compression)
    {
        using var reader = new BinaryReader(stream, Encoding.UTF8, true);
        for (int i = 0; i < HEADER.Length; i++)
        {
            if (reader.ReadByte() != (byte)HEADER[i])
            {
                compression = default;

                stream.Seek(-(i + 1), SeekOrigin.Current);

                return false;
            }
        }

        var version = reader.ReadInt32();
        compression = reader.ReadEnumBinary<Compression>();

        if (version > VERSION)
            throw new NotSupportedException("Version is too new: " + version);

        return true;
    }

    #endregion

    #region Compression Enum Switching

    public enum Compression
    {
        None,
        LZ4,
        LZMA,
        Brotli,
    }

    public static bool IsSupportedFormat(string file)
    {
        return CompressionForExt(file) != null;
    }

    private static Compression? CompressionForExt(string file)
    {
        var ext = Path.GetExtension(file).ToLower().Replace(".", "");

        return ext switch
        {
            "7zbson" => Compression.LZMA,
            "lz4bson" => Compression.LZ4,
            "brson" => Compression.Brotli,
            "frdt" => Compression.None,
            _ => null
        };
    }

    public static Func<Stream, DataTreeDictionary> RawLoader(Compression compression)
    {
        return compression switch
        {
            Compression.None => FromRawBSON,
            Compression.LZ4 => FromRawLZ4BSON,
            Compression.LZMA => FromRaw7zBSON,
            Compression.Brotli => FromRawBRSON,
            _ => throw new ArgumentOutOfRangeException(nameof(compression), compression,
                "compression method not supported")
        };
    }

    public static Action<DataTreeDictionary, Stream> RawSaver(Compression compression)
    {
        return compression switch
        {
            Compression.None => ToRawBSON,
            Compression.LZ4 => ToRawLZ4BSON,
            Compression.LZMA => ToRaw7zBSON,
            Compression.Brotli => (d, s) => ToRawBRSON(d, s),
            _ => throw new ArgumentOutOfRangeException(nameof(compression), compression,
                "compression method not supported"),
        };
    }

    #endregion

    public static DataTreeDictionary Load(string file, Uri uri)
    // TODO: This does not forward the extension correctly.
        => Load(file, uri.LocalPath);

    public static DataTreeDictionary Load(string file, string ext = null)
    {
        // Check the extension
        var compression = ext != null ? CompressionForExt(ext) : CompressionForExt(file);

        using var fStream = File.OpenRead(file);

        // Check the header; if there was a header, compare it with the extension
        if (TryReadHeader(fStream, out var headerCompression))
        {
            // Overwrite with the header value if we didn't have an ext one.
            compression ??= headerCompression;
            if (compression != headerCompression)
            {
                throw new InvalidDataException(
                    $"File extension delcared {compression} compression, header declared {headerCompression} compression.");
            }
        }

        // Here lies the hopes and dreams of long buried files
        // In case of emergency, try your best to determine the compression type
        if (compression == null)
        {
            // try to determine the type for legacy files
            var mime = MimeDetective.MimeTypes.GetFileType(new FileInfo(file));

            if (mime?.Mime != null)
            {
                if (mime.Mime.Contains("lzma"))
                    compression = Compression.LZMA;

                if (mime.Mime.Contains("lz4"))
                    compression = Compression.LZ4;
            }
        }

        var loader = RawLoader(compression ??
                               throw new InvalidDataException(
                                   "Could not determine compression type from file extension or MIME type."));

        return loader(fStream);
    }

    public static DataTreeDictionary LoadAuto(Stream stream)
    {
        return TryReadHeader(stream, out var compression) ? RawLoader(compression)(stream) : null;
    }

    #region Compression Implementations

    public static DataTreeDictionary FromRawBSON(Stream stream)
    {
        using var bson = new BsonDataReader(stream);
        bson.CloseInput = false;
        return (DataTreeDictionary)Read(bson);
    }

    public static DataTreeDictionary FromRawLZ4BSON(Stream stream)
    {
        using var lz = new LZ4Stream(stream, CompressionMode.Decompress);
        return FromRawBSON(lz);
    }

    public static DataTreeDictionary FromRawBRSON(Stream stream)
    {
        // The managed implementation is actually faster at the decompressing than the native one, so just use that instead
        if (NativeBrotliSupported)
        {
            using var memstream = new MemoryStream();
            Brotli.BrotliExtensions.DecompressFromBrotli(stream, memstream);

            memstream.Seek(0, SeekOrigin.Begin);

            return FromRawBSON(memstream);
        }
        else
        {
            // Use managed implementation as a backup
            using var memstream = new MemoryStream();
            using (var bs = new BrotliStream(stream, System.IO.Compression.CompressionMode.Decompress, true))
                bs.CopyTo(memstream);

            memstream.Seek(0, SeekOrigin.Begin);

            return FromRawBSON(memstream);
        }
    }

    public static DataTreeDictionary FromRaw7zBSON(Stream stream)
    {
        using var memstream = new MemoryStream();
        LZMAHelper.Decompress(stream, memstream);
        memstream.Seek(0, SeekOrigin.Begin);

        return FromRawBSON(memstream);
    }

    #endregion

    public static void Save(DataTreeDictionary root, string file, Compression compression)
    {
        // Throws if compression is not supported, this prevents creating empty files.
        // We're ignoring the return value, the switch statement is cheap.
        RawSaver(compression);
        using var fStream = File.Create(file);
        Save(root, fStream, compression);
    }

    public static void Save(DataTreeDictionary root, Stream stream, Compression compression)
    {
        var compressor = RawSaver(compression);
        WriteHeader(stream, Compression.LZ4);
        compressor(root, stream);
    }

    public static void ToLZ4BSON(DataTreeDictionary root, Stream stream)
        => Save(root, stream, Compression.LZ4);

    // TODO: Re-propagate quality somehow
    public static void ToBRSON(DataTreeDictionary root, Stream stream, int quality = 9)
        => Save(root, stream, Compression.Brotli);

    public static void To7zBSON(DataTreeDictionary root, Stream stream)
        => Save(root, stream, Compression.LZMA);

    public static void ToRawBSON(DataTreeDictionary root, Stream stream)
    {
        using var bson = new BsonDataWriter(stream);
        bson.CloseOutput = false;
        Write(root, bson);
    }

    public static void ToRawJSON(DataTreeDictionary root, Stream stream)
    {
        var builder = new StringBuilder();
        var writer = new StringWriter(builder);

        using (var json = new JsonTextWriter(writer))
        {
            json.Formatting = Formatting.Indented;
            Write(root, json);
        }

        var bw = new BinaryWriter(stream);
        bw.Write(builder.ToString());
        bw.Flush();
    }

    public static void ToRawLZ4BSON(DataTreeDictionary root, Stream stream)
    {
        using var lz = new LZ4Stream(stream, CompressionMode.Compress);
        ToRawBSON(root, lz);
    }

    public static void ToRawBRSON(DataTreeDictionary root, Stream stream, int quality = 9)
    {
        if (NativeBrotliSupported)
        {
            using var memstream = new MemoryStream();
            ToRawBSON(root, memstream);

            memstream.Seek(0, SeekOrigin.Begin);

            Brotli.BrotliExtensions.CompressToBrotli(memstream, stream, (uint)quality, 16);
        }
        else
        {
            // Use C# implementation as a fallback
            using var memstream = new MemoryStream();
            ToRawBSON(root, memstream);

            memstream.Seek(0, SeekOrigin.Begin);

            using var bs = new BrotliStream(stream, System.IO.Compression.CompressionMode.Compress, true);
            bs.SetQuality(quality);
            bs.SetWindow(16);

            memstream.CopyTo(bs);
        }
    }

    public static void ToRaw7zBSON(DataTreeDictionary root, Stream stream)
    {
        WriteHeader(stream, Compression.LZMA);

        using var memstream = new MemoryStream();
        ToRawBSON(root, memstream);
        memstream.Seek(0, SeekOrigin.Begin);
        LZMAHelper.Compress(memstream, stream);
    }

    #region CONVERT TO HELPER FUNCTIONS

    static void Write(DataTreeNode node, JsonWriter writer)
    {
        switch (node)
        {
            case DataTreeValue value:
                WriteValue(value, writer);
                break;

            case DataTreeList list:
                WriteList(list, writer);
                break;

            case DataTreeDictionary dict:
                WriteDictionary(dict, writer);
                break;
        }
    }

    static void WriteValue(DataTreeValue value, JsonWriter writer)
    {
        if (value.IsNull)
            writer.WriteNull();
        else if (value.Value is ulong u64)
            writer.WriteValue(unchecked((long)u64));
        else if (value.Value is uint u32)
            writer.WriteValue(unchecked((int)u32));
        else
            writer.WriteValue(value.Value);
    }

    static void WriteList(DataTreeList list, JsonWriter writer)
    {
        writer.WriteStartArray();

        foreach (var el in list.Children)
            Write(el, writer);

        writer.WriteEndArray();
    }

    static void WriteDictionary(DataTreeDictionary node, JsonWriter writer)
    {
        writer.WriteStartObject();

        foreach (var el in node.Children)
        {
            writer.WritePropertyName(el.Key.ToString());
            Write(el.Value, writer);
        }

        writer.WriteEndObject();
    }

    static DataTreeNode Read(JsonReader reader)
    {
        reader.MaxDepth = null;
        reader.Read();
        return ReadNode(reader);
    }

    static DataTreeNode ReadNode(JsonReader reader)
    {
        switch (reader.TokenType)
        {
            case JsonToken.Boolean:
                return new DataTreeValue((bool)reader.Value);

            case JsonToken.Float:
                return new DataTreeValue((double)reader.Value);

            case JsonToken.Integer:
                return new DataTreeValue((long)reader.Value);

            case JsonToken.String:
                return DataTreeValue.RawString(reader.Value as string);

            case JsonToken.Date:
                return new DataTreeValue((DateTime)reader.Value);

            case JsonToken.Null:
                return new DataTreeValue(null as string);

            case JsonToken.StartArray:
                return ReadList(reader);

            case JsonToken.StartObject:
                return ReadDictionary(reader);

            default:
                return null;
        }
    }

    static DataTreeList ReadList(JsonReader reader)
    {
        var list = new DataTreeList();

        while (reader.Read())
        {
            switch (reader.TokenType)
            {
                case JsonToken.EndArray:
                    return list;

                default:
                    var node = ReadNode(reader);

                    if (node != null)
                        list.Add(node);
                    break;
            }
        }

        throw new Exception("Didn't find end of array!");
    }

    static DataTreeDictionary ReadDictionary(JsonReader reader)
    {
        var dict = new DataTreeDictionary();

        string propertyName = null;

        while (reader.Read())
        {
            switch (reader.TokenType)
            {
                case JsonToken.EndObject:
                    return dict;

                case JsonToken.PropertyName:
                    propertyName = reader.Value as string;
                    break;

                default:
                    var node = ReadNode(reader);

                    if (node != null)
                    {
                        dict.Add(propertyName, node);
                        propertyName = null;
                    }

                    break;
            }
        }

        throw new Exception("Didn't find end of dictionary!");
    }

    #endregion
}
