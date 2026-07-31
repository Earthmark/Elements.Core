namespace Elements.Core.Tests
{
    public class SimpleClass
    {
        public class SimpleNestedClass
        {

        }
    }
    
    public class RootClass<T>
    {
        public enum NestedEnum { A, B, C, D };

        public class NestedClass
        {
            public class NestedGeneric<N>
            {
                public enum SuperNestedEnum { E, F, G, H };

                public class SuperNested
                {

                }
            }
        }

        public class NestedGeneric<E, F>
        {
            public class SuperNestedGeneric<G>
            {
                public class UltraNested
                {

                }
            }
        }
    }

    [TestClass]
    public class TypeParsingSerialization
    {
        [DataRow(typeof(SimpleClass), "SimpleClass")]
        [DataRow(typeof(SimpleClass.SimpleNestedClass), "SimpleClass+SimpleNestedClass")]

        [DataRow(typeof(SimpleClass[]), "SimpleClass[]")]
        [DataRow(typeof(SimpleClass[,]), "SimpleClass[,]")]
        [DataRow(typeof(SimpleClass[,,]), "SimpleClass[,,]")]
        [DataRow(typeof(SimpleClass[,,,]), "SimpleClass[,,,]")]
        [DataRow(typeof(SimpleClass[][]), "SimpleClass[][]")]
        [DataRow(typeof(SimpleClass[][][]), "SimpleClass[][][]")]
        [DataRow(typeof(SimpleClass[,][,,][,,,]), "SimpleClass[,][,,][,,,]")]

        [DataRow(typeof(DummyEnum), "DummyEnum", "Elements.Core.DummyEnum")]
        [DataRow(typeof(DummyEnum?), "DummyEnum?", "Elements.Core.DummyEnum?")]
        [DataRow(typeof(DummyEnum?[]), "DummyEnum?[]", "Elements.Core.DummyEnum?[]")]

        [DataRow(typeof(dummy<int?>), "dummy<int?>", "Elements.Core.dummy<int?>")]
        [DataRow(typeof(dummy<int>?), "dummy<int>?", "Elements.Core.dummy<int>?")]
        [DataRow(typeof(dummy<int?>?), "dummy<int?>?", "Elements.Core.dummy<int?>?")]
        [DataRow(typeof(List<dummy<int?>?>), "List<dummy<int?>?>", "System.Collections.Generic.List<Elements.Core.dummy<int?>?>")]

        [DataRow(typeof(SimpleClass.SimpleNestedClass[]), "SimpleClass+SimpleNestedClass[]")]
        [DataRow(typeof(SimpleClass.SimpleNestedClass[,]), "SimpleClass+SimpleNestedClass[,]")]
        [DataRow(typeof(SimpleClass.SimpleNestedClass[,,]), "SimpleClass+SimpleNestedClass[,,]")]
        [DataRow(typeof(SimpleClass.SimpleNestedClass[,,,]), "SimpleClass+SimpleNestedClass[,,,]")]
        [DataRow(typeof(SimpleClass.SimpleNestedClass[][]), "SimpleClass+SimpleNestedClass[][]")]
        [DataRow(typeof(SimpleClass.SimpleNestedClass[][][]), "SimpleClass+SimpleNestedClass[][][]")]
        [DataRow(typeof(SimpleClass.SimpleNestedClass[,][,,][,,,]), "SimpleClass+SimpleNestedClass[,][,,][,,,]")]

        [DataRow(typeof(RootClass<>), "RootClass<T>")]
        [DataRow(typeof(RootClass<>.NestedEnum), "RootClass<T>+NestedEnum")]
        [DataRow(typeof(RootClass<int>.NestedEnum), "RootClass<int>+NestedEnum")]
        [DataRow(typeof(RootClass<int?>.NestedEnum), "RootClass<int?>+NestedEnum")]

        [DataRow(typeof(RootClass<int[]>.NestedEnum[]), "RootClass<int[]>+NestedEnum[]")]
        [DataRow(typeof(RootClass<int[,,]>.NestedEnum[,,,,]), "RootClass<int[,,]>+NestedEnum[,,,,]")]
        [DataRow(typeof(RootClass<int?[]>.NestedEnum[]), "RootClass<int?[]>+NestedEnum[]")]
        [DataRow(typeof(RootClass<int?[,][,,][,,,]>.NestedEnum[,,,][,,][,]), "RootClass<int?[,][,,][,,,]>+NestedEnum[,,,][,,][,]")]
        [DataRow(typeof(RootClass<int?[,,]>.NestedEnum[,,,,]), "RootClass<int?[,,]>+NestedEnum[,,,,]")]

        [DataRow(typeof(RootClass<>.NestedClass.NestedGeneric<>.SuperNestedEnum),
            "RootClass<T>+NestedClass+NestedGeneric<N>+SuperNestedEnum")]
        [DataRow(typeof(RootClass<float>.NestedClass.NestedGeneric<int>.SuperNestedEnum),
            "RootClass<float>+NestedClass+NestedGeneric<int>+SuperNestedEnum")]
        [DataRow(typeof(RootClass<float?>.NestedClass.NestedGeneric<int?>.SuperNestedEnum),
            "RootClass<float?>+NestedClass+NestedGeneric<int?>+SuperNestedEnum")]
        [DataRow(typeof(RootClass<>.NestedGeneric<,>.SuperNestedGeneric<>.UltraNested),
            "RootClass<T>+NestedGeneric<E,F>+SuperNestedGeneric<G>+UltraNested")]
        [DataRow(typeof(RootClass<byte>.NestedGeneric<ushort,uint>.SuperNestedGeneric<ulong>.UltraNested),
            "RootClass<byte>+NestedGeneric<ushort,uint>+SuperNestedGeneric<ulong>+UltraNested")]
        [DataRow(typeof(RootClass<byte?>.NestedGeneric<ushort?, uint?>.SuperNestedGeneric<ulong?>.UltraNested),
            "RootClass<byte?>+NestedGeneric<ushort?,uint?>+SuperNestedGeneric<ulong?>+UltraNested")]

        [DataTestMethod]
        public void TestTypeFormatting(Type type, string expected, string expectedFull = null)
        {
            Assert.AreEqual(expected, type.GetNiceName(), "GetNiceName failed");

            if(expectedFull == null)
                expectedFull = type.Namespace + "." + expected;

            Assert.AreEqual(expectedFull, type.GetNiceFullName(), "GetNiceFullName failed");
        }

        [DataRow(typeof(DummyEnum?[]))]

        [DataRow(typeof(int))]
        [DataRow(typeof(string))]
        [DataRow(typeof(float))]
        [DataRow(typeof(float3))]
        [DataRow(typeof(doubleQ))]
        [DataRow(typeof(colorX))]
        [DataRow(typeof(float3x3))]

        [DataRow(typeof(int?))]
        [DataRow(typeof(float?))]
        [DataRow(typeof(float3?))]
        [DataRow(typeof(doubleQ?))]
        [DataRow(typeof(colorX?))]
        [DataRow(typeof(float3x3?))]

        [DataRow(typeof(DummyEnum))]
        [DataRow(typeof(DummyEnum?))]
        [DataRow(typeof(DummyEnum?[]))]

        [DataRow(typeof(dummy<int?>))]
        [DataRow(typeof(dummy<int>?))]
        [DataRow(typeof(dummy<int?>?))]
        [DataRow(typeof(List<dummy<int?>?>))]

        [DataRow(typeof(SimpleClass))]

        [DataRow(typeof(int[]))]
        [DataRow(typeof(int[][]))]
        [DataRow(typeof(int[][][]))]

        [DataRow(typeof(int[,]))]
        [DataRow(typeof(int[,,]))]
        [DataRow(typeof(int[,][,,][,,,]))]

        [DataRow(typeof(float3[]))]
        [DataRow(typeof(float3x3?[]))]
        [DataRow(typeof(SimpleClass[]))]
        [DataRow(typeof(SimpleClass.SimpleNestedClass[]))]
        [DataRow(typeof(SimpleClass.SimpleNestedClass[][]))]
        [DataRow(typeof(SimpleClass.SimpleNestedClass[][][]))]
        [DataRow(typeof(SimpleClass.SimpleNestedClass[,]))]
        [DataRow(typeof(SimpleClass.SimpleNestedClass[,][,,][,,,]))]
        [DataRow(typeof(SimpleClass.SimpleNestedClass[,,,,,]))]
        [DataRow(typeof(SimpleClass.SimpleNestedClass[,,,,,][,,]))]

        [DataRow(typeof(List<>))]
        [DataRow(typeof(List<int>))]
        [DataRow(typeof(List<int?>))]
        [DataRow(typeof(List<int[]>))]
        [DataRow(typeof(List<int?[]>))]
        [DataRow(typeof(List<int?[,,,]>))]
        [DataRow(typeof(List<List<int>[]>))]
        [DataRow(typeof(List<List<int3>>))]
        [DataRow(typeof(List<List<int3?>>))]
        [DataRow(typeof(List<List<DummyEnum?>>))]
        [DataRow(typeof(List<List<List<List<List<string>>>>>))]

        [DataRow(typeof(List<List<int>.Enumerator>.Enumerator))]
        [DataRow(typeof(Dictionary<Dictionary<List<int>.Enumerator, List<string>.Enumerator>.Enumerator, List<List<float3>.Enumerator[]>>))]

        [DataRow(typeof(List<>.Enumerator))]
        [DataRow(typeof(List<int>.Enumerator))]
        [DataRow(typeof(List<int[]>.Enumerator))]
        [DataRow(typeof(List<List<int>[]>.Enumerator))]
        [DataRow(typeof(List<List<int3>>.Enumerator))]
        [DataRow(typeof(List<List<int3[]>>.Enumerator))]
        [DataRow(typeof(List<List<int3?>>.Enumerator))]
        [DataRow(typeof(List<List<int3?>>.Enumerator[]))]
        [DataRow(typeof(List<List<int3?[]>[]>.Enumerator[]))]
        [DataRow(typeof(List<List<int3?[,,]>[,,,,,]>.Enumerator[,,,,,,,,,,,]))]
        [DataRow(typeof(List<List<List<List<List<string>>>>>.Enumerator))]

        [DataRow(typeof(Dictionary<,>))]
        [DataRow(typeof(Dictionary<string,float>))]
        [DataRow(typeof(Dictionary<string,float?>))]
        [DataRow(typeof(Dictionary<string[], float[]>))]
        [DataRow(typeof(Dictionary<List<int2>, List<string>>))]
        [DataRow(typeof(Dictionary<List<int2?>, List<string>>))]
        [DataRow(typeof(Dictionary<List<int2?[]>[], List<string[]>[]>))]
        [DataRow(typeof(Dictionary<List<DummyEnum?[]>[], List<string[]>[]>))]

        [DataRow(typeof(Dictionary<,>.Enumerator))]
        [DataRow(typeof(Dictionary<string, float>.Enumerator))]
        [DataRow(typeof(Dictionary<string, float?>.Enumerator))]
        [DataRow(typeof(Dictionary<List<int2>, List<string>>.Enumerator))]
        [DataRow(typeof(Dictionary<List<int2?>, List<string>>.Enumerator))]

        [DataRow(typeof(RootClass<>))]
        [DataRow(typeof(RootClass<>.NestedEnum))]
        [DataRow(typeof(RootClass<int>.NestedEnum))]
        [DataRow(typeof(RootClass<int>.NestedEnum[]))]

        [DataRow(typeof(RootClass<>.NestedClass.NestedGeneric<>.SuperNestedEnum))]
        [DataRow(typeof(RootClass<float2>.NestedClass.NestedGeneric<int3>.SuperNestedEnum))]
        [DataRow(typeof(RootClass<>.NestedGeneric<,>.SuperNestedGeneric<>.UltraNested))]
        [DataRow(typeof(RootClass<byte>.NestedGeneric<ushort, uint>.SuperNestedGeneric<ulong>.UltraNested))]
        [DataRow(typeof(RootClass<byte?>.NestedGeneric<ushort?, uint?>.SuperNestedGeneric<ulong?>.UltraNested))]
        [DataRow(typeof(RootClass<byte?[]>.NestedGeneric<ushort?[], uint?[]>.SuperNestedGeneric<ulong?[]>.UltraNested[]))]
        [DataRow(typeof(RootClass<byte?[,][]>.NestedGeneric<ushort?[,][,,][,,,], uint?[][,][,][]>.SuperNestedGeneric<ulong?[,,][]>.UltraNested[][,,,]))]
        [DataRow(typeof(RootClass<byte?[,,,]>.NestedGeneric<ushort?[,,], uint?[,,,,,,]>.SuperNestedGeneric<ulong?[]>.UltraNested[,,]))]
        [DataRow(typeof(RootClass<DummyEnum?[,,,]>.NestedGeneric<DummyEnum?[,,], DummyEnum?[,,,,,,]>.SuperNestedGeneric<DummyEnum?[]>.UltraNested[,,]))]
        [DataRow(typeof(RootClass<dummy<DummyEnum?>?[,,,]>.NestedGeneric<dummy<DummyEnum?>?[,,], dummy<DummyEnum?>?[,,,,,,]>.SuperNestedGeneric<dummy<DummyEnum?>?[]>.UltraNested[,,]))]

        [DataTestMethod]
        public void TestRoundTrip(Type type)
        {
            var formatted = type.GetNiceFullName(includeGenericParameters: false);
            var parsed = NiceTypeParser.TryParse(formatted);

            Assert.AreEqual(type, parsed, $"Formatted: {formatted}");
        }

        [DataRow("System.String", typeof(string))]
        [DataRow("System.Int32", typeof(int))]
        [DataRow("System.UInt64", typeof(ulong))]

        [DataRow("Elements.Core.float2", typeof(float2))]
        [DataRow("Elements.Core.long4", typeof(long4))]
        [DataRow("Elements.Core.double3x3", typeof(double3x3))]
        [DataRow("Elements.Core.color", typeof(color))]
        [DataRow("Elements.Core.colorX", typeof(colorX))]

        [DataRow("System.Collections.Generic.List`1", typeof(List<>))]
        [DataRow("System.Collections.Generic.List`1[[System.Int32, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]]",
            typeof(List<int>))]
        [DataRow("System.Collections.Generic.List`1[[System.Collections.Generic.List`1[[Elements.Core.float2, Elements.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]], mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]]",
            typeof(List<List<float2>>))]
        [DataRow("System.Collections.Generic.List`1[[System.Collections.Generic.List`1+Enumerator[[Elements.Core.float2, Elements.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]], mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]]",
            typeof(List<List<float2>.Enumerator>))]

        [DataRow("System.Collections.Generic.Dictionary`2", typeof(Dictionary<,>))]
        [DataRow("System.Collections.Generic.Dictionary`2[[System.String, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089],[Elements.Core.color, Elements.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]]",
            typeof(Dictionary<string, color>))]
        [DataRow("System.Collections.Generic.Dictionary`2[[System.Collections.Generic.List`1[[Elements.Core.float3, Elements.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]], mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089],[System.Collections.Generic.List`1[[System.Collections.Generic.List`1[[System.String, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]], mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]], mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]]",
            typeof(Dictionary<List<float3>, List<List<string>>>))]

        [DataRow("System.Collections.Generic.List`1+Enumerator", typeof(List<>.Enumerator))]
        [DataRow("System.Collections.Generic.List`1+Enumerator[[System.Int32, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]]",
            typeof(List<int>.Enumerator))]
        [DataRow("System.Collections.Generic.List`1+Enumerator[[System.Collections.Generic.List`1[[Elements.Core.float2, Elements.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]], mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]]",
            typeof(List<List<float2>>.Enumerator))]

        [DataRow("System.Collections.Generic.Dictionary`2+Enumerator", typeof(Dictionary<,>.Enumerator))]
        [DataRow("System.Collections.Generic.Dictionary`2+Enumerator[[System.String, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089],[Elements.Core.color, Elements.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]]",
            typeof(Dictionary<string, color>.Enumerator))]
        [DataRow("System.Collections.Generic.Dictionary`2+Enumerator[[System.Collections.Generic.List`1[[Elements.Core.float3, Elements.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]], mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089],[System.Collections.Generic.List`1[[System.Collections.Generic.List`1[[System.String, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]], mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]], mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]]",
            typeof(Dictionary<List<float3>, List<List<string>>>.Enumerator))]
        [DataRow("System.Collections.Generic.Dictionary`2+Enumerator[[System.Collections.Generic.List`1+Enumerator[[Elements.Core.float3, Elements.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]], mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089],[System.Collections.Generic.List`1[[System.Collections.Generic.List`1[[System.String, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]], mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]], mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]]",
            typeof(Dictionary<List<float3>.Enumerator, List<List<string>>>.Enumerator))]

        [DataTestMethod]
        public void LegacyParsing(string serialized, Type expected)
        {
            var parsed = LegacyTypeParser.TryParse(serialized, (typename, assembly) => Type.GetType(typename));

            Assert.AreEqual(expected, parsed);
        }

        // Pathological cases
        [DataRow("", null)]
        [DataRow("?", null)]
        [DataRow("[]", null)]
        [DataRow("<>", null)]
        [DataRow("+", null)]

        [DataTestMethod]
        public void TestParsing(string serialized, Type expected)
        {
            var parsed = NiceTypeParser.TryParse(serialized);

            Assert.AreEqual(expected, parsed);
        }

        [TestMethod]
        public void AssemblyNamePrefix()
        {
            var parsed = NiceTypeParser.TryParse("[Assembly]System.String[]",
                str =>
                {
                    Console.WriteLine($"InnerType: {str}");

                    if (str == "[Assembly]System.String")
                        return typeof(string);
                    else
                        return null; // Wrong!
                });

            Assert.AreEqual(typeof(string[]), parsed);

            parsed = NiceTypeParser.TryParse("[Root]System.Collections.Generic.List<[Inner]SimpleClass+SimpleNestedClass>",
                str =>
                {
                    Console.WriteLine($"InnerType: {str}");

                    if (str == "[Root]System.Collections.Generic.List`1")
                        return typeof(List<>);
                    else if (str == "[Inner]SimpleClass+SimpleNestedClass")
                        return typeof(SimpleClass.SimpleNestedClass);
                    else
                        return null;
                });

            Assert.AreEqual(typeof(List<SimpleClass.SimpleNestedClass>), parsed);
        }
    }
}
