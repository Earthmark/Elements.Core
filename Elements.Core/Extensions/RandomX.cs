using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Renderite.Shared;

namespace Elements.Core
{
    public static class RandomX
    {
        public const string LOWERCASE_ALPHABET = "abcdefghijklmnopqrstuvwxyz";
        public const string UPPERCASE_ALPHABET = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        public const string DIGITS = "0123456789";
        public const string LETTERS_AND_DIGITS = LOWERCASE_ALPHABET + DIGITS;

        static RandomXGenerator r = new RandomXGenerator();

        public static T Generate<T>(Func<RandomXGenerator, T> gen)
        {
            lock (r)
                return gen(r);
        }

        public static float Value { get { lock (r) return r.Value; } }
        public static double Double { get { lock (r) return r.Double; } }
        public static int Int { get { lock (r) return r.Int; } }
        public static bool Bool { get { lock (r) return r.Bool; } }
        public static float2 Float2 { get { lock (r) return r.Float2; } }
        public static float3 Float3 { get { lock (r) return r.Float3; } }
        public static float4 Float4 { get { lock (r) return r.Float4; } }
        public static float3 BarycentricCoordinate { get { lock (r) return r.BarycentricCoordinate; } }

        public static bool Chance(float probability) { lock (r) return r.Chance(probability); }

        public static int Range(int max) { lock (r) return r.Range(max); }
        public static int2 Range(in int2 max) { lock (r) return r.Range(max); }
        public static int3 Range(in int3 max) { lock (r) return r.Range(max); }
        public static int4 Range(in int4 max) { lock (r) return r.Range(max); }

        public static sbyte Range(sbyte min, sbyte max) { lock (r) return r.Range(min, max); }
        public static short Range(short min, short max) { lock (r) return r.Range(min, max); }
        public static int Range(int min, int max) { lock (r) return r.Range(min, max); }
        public static long Range(long min, long max) { lock (r) return r.Range(min, max); }

        public static byte Range(byte min, byte max) { lock (r) return r.Range(min, max); }
        public static ushort Range(ushort min, ushort max) { lock (r) return r.Range(min, max); }
        public static uint Range(uint min, uint max) { lock (r) return r.Range(min, max); }
        public static ulong Range(ulong min, ulong max) { lock (r) return r.Range(min, max); }

        public static sbyte2 Range(sbyte2 min, sbyte2 max) { lock (r) return r.Range(min, max); }
        public static sbyte3 Range(sbyte3 min, sbyte3 max) { lock (r) return r.Range(min, max); }
        public static sbyte4 Range(sbyte4 min, sbyte4 max) { lock (r) return r.Range(min, max); }

        public static short2 Range(short2 min, short2 max) { lock (r) return r.Range(min, max); }
        public static short3 Range(short3 min, short3 max) { lock (r) return r.Range(min, max); }
        public static short4 Range(short4 min, short4 max) { lock (r) return r.Range(min, max); }

        public static int2 Range(int2 min, int2 max) { lock (r) return r.Range(min, max); }
        public static int3 Range(int3 min, int3 max) { lock (r) return r.Range(min, max); }
        public static int4 Range(int4 min, int4 max) { lock (r) return r.Range(min, max); }

        public static long2 Range(long2 min, long2 max) { lock (r) return r.Range(min, max); }
        public static long3 Range(long3 min, long3 max) { lock (r) return r.Range(min, max); }
        public static long4 Range(long4 min, long4 max) { lock (r) return r.Range(min, max); }

        public static byte2 Range(byte2 min, byte2 max) { lock (r) return r.Range(min, max); }
        public static byte3 Range(byte3 min, byte3 max) { lock (r) return r.Range(min, max); }
        public static byte4 Range(byte4 min, byte4 max) { lock (r) return r.Range(min, max); }

        public static ushort2 Range(ushort2 min, ushort2 max) { lock (r) return r.Range(min, max); }
        public static ushort3 Range(ushort3 min, ushort3 max) { lock (r) return r.Range(min, max); }
        public static ushort4 Range(ushort4 min, ushort4 max) { lock (r) return r.Range(min, max); }

        public static uint2 Range(uint2 min, uint2 max) { lock (r) return r.Range(min, max); }
        public static uint3 Range(uint3 min, uint3 max) { lock (r) return r.Range(min, max); }
        public static uint4 Range(uint4 min, uint4 max) { lock (r) return r.Range(min, max); }

        public static ulong2 Range(ulong2 min, ulong2 max) { lock (r) return r.Range(min, max); }
        public static ulong3 Range(ulong3 min, ulong3 max) { lock (r) return r.Range(min, max); }
        public static ulong4 Range(ulong4 min, ulong4 max) { lock (r) return r.Range(min, max); }

        public static float Range(float min, float max) { lock (r) return r.Range(min, max); }
        public static float2 Range(in float2 min, in float2 max) { lock (r) return r.Range(min, max); }
        public static float3 Range(in float3 min, in float3 max) { lock (r) return r.Range(min, max); }
        public static float4 Range(in float4 min, in float4 max) { lock (r) return r.Range(min, max); }

        public static float2 Lerp(in float2 min, in float2 max) { lock (r) return r.Lerp(min, max); }
        public static float3 Lerp(in float3 min, in float3 max) { lock (r) return r.Lerp(min, max); }
        public static float4 Lerp(in float4 min, in float4 max) { lock (r) return r.Lerp(min, max); }

        public static double Range(double min, double max) { lock (r) return r.Range(min, max); }
        public static double2 Range(in double2 min, in double2 max) { lock (r) return r.Range(min, max); }
        public static double3 Range(in double3 min, in double3 max) { lock (r) return r.Range(min, max); }
        public static double4 Range(in double4 min, in double4 max) { lock (r) return r.Range(min, max); }

        public static color Lerp(in color min, in color max) { lock (r) return r.Lerp(min, max); }
        public static colorX Lerp(in colorX min, in colorX max,
            ColorProfileAwareOperation profileHandling = ColorProfileAwareOperation.UseLinear) { lock (r) return r.Lerp(min, max, profileHandling); }

        public static floatQ Slerp(in floatQ from, in floatQ to) { lock (r) return r.Slerp(from, to); }

        public static float2 OnUnitSquare { get { lock (r) return r.OnUnitSquare; } }
        public static float2 InsideUnitSquare { get { lock (r) return r.InsideUnitSquare; } }

        public static float3 OnUnitCube { get { lock (r) return r.OnUnitCube; } }
        public static float3 InsideUnitCube { get { lock (r) return r.InsideUnitCube; } }

        public static float3 OnUnitSphere { get { lock (r) return r.OnUnitSphere; } }
        public static float3 InsideUnitSphere { get { lock (r) return r.InsideUnitSphere; } }

        public static float2 OnUnitCircle { get { lock (r) return r.OnUnitCircle; } }
        public static float2 InsideUnitCircle { get { lock (r) return r.InsideUnitCircle; } }

        public static floatQ Rotation { get { lock (r) return r.Rotation; } }

        public static colorX RGB { get { lock (r) return r.RGB; } }
        public static colorX RGBA { get { lock (r) return r.RGBA; } }
        public static colorX Grayscale { get { lock (r) return r.Grayscale; } }
        public static colorX Hue { get { lock (r) return r.Hue; } }

        public static float3 InsideCone(float height, float baseRadius) { lock (r) return r.InsideCone(height, baseRadius); }
        public static float3 OnCone(float height, float baseRadius) { lock (r) return r.OnCone(height, baseRadius); }

        // string
        public static string String(string characters, int length) { lock (r) return r.String(characters, length); }
        public static void Bytes(byte[] buffer) { lock(r) { r.Bytes(buffer); } }
        public static byte[] Bytes(int count) { lock (r) return r.Bytes(count); }
        public static string LowercaseAlphabet(int length) { lock (r) return r.LowercaseAlphabet(length); }
        public static string UppercaseAlphabet(int length) { lock (r) return r.UppercaseAlphabet(length); }
        public static string Digits(int length) { lock (r) return r.Digits(length); }
        public static string LettersAndDigits(int length) { lock (r) return r.LettersAndDigits(length); }

        public static E Enum<E>() { lock (r) return r.Enum<E>(); }
    }

    // TODO!!! Update algorithms for generating points in a volume with better ones with better distribution

    // Instance, in case someone wants to create one with custom seed
    public class RandomXGenerator
    {
        const float FLOAT_RATIO = 1f / int.MaxValue;

        Random random;

        public RandomXGenerator()
        {
            random = new Random();
        }

        public RandomXGenerator(int? seed)
        {
            if (seed != null)
                random = new Random(seed.Value);
            else
                random = new Random();
        }

        public float Value => random.Next() * FLOAT_RATIO;
        public double Double => random.NextDouble();

        public int Int => random.Next();

        public float2 Float2 => new float2(Value, Value);
        public float3 Float3 => new float3(Value, Value, Value);
        public float4 Float4 => new float4(Value, Value, Value, Value);

        public float3 BarycentricCoordinate
        {
            get
            {
                var r = Value;
                var s = Value;

                if(r + s > 1)
                {
                    r = 1 - r;
                    s = 1 - s;
                }

                return new float3(r, s, 1 - (r + s));
            }
        }

        public bool Bool => random.Next() > (int.MaxValue >> 1);
        public bool Chance(float probability) => Value < probability;

        public int Range(int max) => random.Next(max);
        public int2 Range(in int2 max) => new int2(Range(max.x), Range(max.y));
        public int3 Range(in int3 max) => new int3(Range(max.x), Range(max.y), Range(max.z));
        public int4 Range(in int4 max) => new int4(Range(max.x), Range(max.y), Range(max.z), Range(max.w));


        public sbyte Range(sbyte min, sbyte max) => unchecked((sbyte)random.Next(min, max));
        public short Range(short min, short max) => unchecked((short)random.Next(min, max));
        public int Range(int min, int max) => random.Next(min, max);
        public long Range(long min, long max) => random.NextInt64(min, max);

        public byte Range(byte min, byte max) => unchecked((byte)random.Next(min, max));
        public ushort Range(ushort min, ushort max) => unchecked((ushort)random.Next(min, max));
        public uint Range(uint min, uint max) => unchecked((uint)random.NextInt64(min, max));
        public ulong Range(ulong min, ulong max) => MathX.LerpUnclamped(min, max, random.NextDouble());

        public sbyte2 Range(in sbyte2 min, in sbyte2 max) => new sbyte2(Range(min.x, max.x), Range(min.y, max.y));
        public sbyte3 Range(in sbyte3 min, in sbyte3 max) => new sbyte3(Range(min.x, max.x), Range(min.y, max.y), Range(min.z, max.z));
        public sbyte4 Range(in sbyte4 min, in sbyte4 max) => new sbyte4(Range(min.x, max.x), Range(min.y, max.y), Range(min.z, max.z), Range(min.w, max.w));


        public short2 Range(in short2 min, in short2 max) => new short2(Range(min.x, max.x), Range(min.y, max.y));
        public short3 Range(in short3 min, in short3 max) => new short3(Range(min.x, max.x), Range(min.y, max.y), Range(min.z, max.z));
        public short4 Range(in short4 min, in short4 max) => new short4(Range(min.x, max.x), Range(min.y, max.y), Range(min.z, max.z), Range(min.w, max.w));

        public int2 Range(in int2 min, in int2 max) => new int2(Range(min.x, max.x), Range(min.y, max.y));
        public int3 Range(in int3 min, in int3 max) => new int3(Range(min.x, max.x), Range(min.y, max.y), Range(min.z, max.z));
        public int4 Range(in int4 min, in int4 max) => new int4(Range(min.x, max.x), Range(min.y, max.y), Range(min.z, max.z), Range(min.w, max.w));

        public long2 Range(in long2 min, in long2 max) => new long2(Range(min.x, max.x), Range(min.y, max.y));
        public long3 Range(in long3 min, in long3 max) => new long3(Range(min.x, max.x), Range(min.y, max.y), Range(min.z, max.z));
        public long4 Range(in long4 min, in long4 max) => new long4(Range(min.x, max.x), Range(min.y, max.y), Range(min.z, max.z), Range(min.w, max.w));

        public byte2 Range(in byte2 min, in byte2 max) => new byte2(Range(min.x, max.x), Range(min.y, max.y));
        public byte3 Range(in byte3 min, in byte3 max) => new byte3(Range(min.x, max.x), Range(min.y, max.y), Range(min.z, max.z));
        public byte4 Range(in byte4 min, in byte4 max) => new byte4(Range(min.x, max.x), Range(min.y, max.y), Range(min.z, max.z), Range(min.w, max.w));


        public ushort2 Range(in ushort2 min, in ushort2 max) => new ushort2(Range(min.x, max.x), Range(min.y, max.y));
        public ushort3 Range(in ushort3 min, in ushort3 max) => new ushort3(Range(min.x, max.x), Range(min.y, max.y), Range(min.z, max.z));
        public ushort4 Range(in ushort4 min, in ushort4 max) => new ushort4(Range(min.x, max.x), Range(min.y, max.y), Range(min.z, max.z), Range(min.w, max.w));

        public uint2 Range(in uint2 min, in uint2 max) => new uint2(Range(min.x, max.x), Range(min.y, max.y));
        public uint3 Range(in uint3 min, in uint3 max) => new uint3(Range(min.x, max.x), Range(min.y, max.y), Range(min.z, max.z));
        public uint4 Range(in uint4 min, in uint4 max) => new uint4(Range(min.x, max.x), Range(min.y, max.y), Range(min.z, max.z), Range(min.w, max.w));

        public ulong2 Range(in ulong2 min, in ulong2 max) => new ulong2(Range(min.x, max.x), Range(min.y, max.y));
        public ulong3 Range(in ulong3 min, in ulong3 max) => new ulong3(Range(min.x, max.x), Range(min.y, max.y), Range(min.z, max.z));
        public ulong4 Range(in ulong4 min, in ulong4 max) => new ulong4(Range(min.x, max.x), Range(min.y, max.y), Range(min.z, max.z), Range(min.w, max.w));


        public float Range(float min, float max)
        {
            // TODO!!! Verify if this optimization makes things faster overall
            if (min == max)
                return min;

            return MathX.LerpUnclamped(min, max, Value);
        }

        public float2 Range(in float2 min, in float2 max)
        {
            if (min == max)
                return min;

            return new float2(
                Range(min.x, max.x),
                Range(min.y, max.y));
        }

        public float3 Range(in float3 min, in float3 max)
        {
            if (min == max)
                return min;

            return new float3(
                Range(min.x, max.x),
                Range(min.y, max.y),
                Range(min.z, max.z) );
        }

        public float4 Range(in float4 min, in float4 max)
        {
            if (min == max)
                return min;

            return new float4(
                Range(min.x, max.x),
                Range(min.y, max.y),
                Range(min.z, max.z),
                Range(min.w, max.w));
        }

        public float2 Lerp(in float2 min, in float2 max)
        {
            if (min == max)
                return min;

            return MathX.LerpUnclamped(min, max, Value);
        }

        public float3 Lerp(in float3 min, in float3 max)
        {
            if (min == max)
                return min;

            return MathX.LerpUnclamped(min, max, Value);
        }

        public float4 Lerp(in float4 min, in float4 max)
        {
            if (min == max)
                return min;

            return MathX.LerpUnclamped(min, max, Value);
        }

        public double Range(double min, double max)
        {
            // TODO!!! Verify if this optimization makes things faster overall
            if (min == max)
                return min;

            return MathX.LerpUnclamped(min, max, Double);
        }

        public double2 Range(in double2 min, in double2 max)
        {
            if (min == max)
                return min;

            return new double2(
                Range(min.x, max.x),
                Range(min.y, max.y));
        }

        public double3 Range(in double3 min, in double3 max)
        {
            if (min == max)
                return min;

            return new double3(
                Range(min.x, max.x),
                Range(min.y, max.y),
                Range(min.z, max.z));
        }

        public double4 Range(in double4 min, in double4 max)
        {
            if (min == max)
                return min;

            return new double4(
                Range(min.x, max.x),
                Range(min.y, max.y),
                Range(min.z, max.z),
                Range(min.w, max.w));
        }

        public color Lerp(in color min, in color max)
        {
            if (min == max)
                return min;

            return MathX.LerpUnclamped(min, max, Value);
        }

        public colorX Lerp(in colorX min, in colorX max, ColorProfileAwareOperation profileHandling = ColorProfileAwareOperation.UseLinear)
        {
            if (min == max)
                return min;

            return MathX.LerpUnclamped(min, max, Value, profileHandling);
        }

        public floatQ Slerp(in floatQ from, in floatQ to) => MathX.Slerp(from, to, Value);

        public float2 InsideUnitSquare => Range(new float2(-0.5f, -0.5f), new float2(0.5f, 0.5f));
        public float2 OnUnitSquare
        {
            get
            {
                switch(Range(4))
                {
                    case 0: return new float2(-0.5f, Range(-0.5f, 0.5f));
                    case 1: return new float2(+0.5f, Range(-0.5f, 0.5f));
                    case 2: return new float2(Range(-0.5f, 0.5f), -0.5f);
                    case 3: return new float2(Range(-0.5f, 0.5f), +0.5f);

                    default: // shouldn't happen
                        return float2.Zero;
                }
            }
        }

        public float3 InsideUnitCube => Range(new float3(-0.5f, -0.5f, -0.5f), new float3(0.5f, 0.5f, 0.5f));
        public float3 OnUnitCube
        {
            get
            {
                var square = InsideUnitSquare;

                switch(Range(6))
                {
                    case 0: return new float3(-0.5f, square);
                    case 1: return new float3(+0.5f, square);
                    case 2: return new float3(square.x, -0.5f, square.y);
                    case 3: return new float3(square.x, +0.5f, square.y);
                    case 4: return new float3(square, -0.5f);
                    case 5: return new float3(square, +0.5f);
                    default: return float3.Zero; // shouldn't happen
                }
            }
        }

        public void OnUnitCubeWithNormal(out float3 point, out float3 normal)
        {
            var square = InsideUnitSquare;

            switch (Range(6))
            {
                case 0:
                    point = new float3(-0.5f, square);
                    normal = new float3(-1f, 0f, 0f);
                    break;

                case 1:
                    point = new float3(+0.5f, square);
                    normal = new float3(+1f, 0f, 0f);
                    break;

                case 2:
                    point = new float3(square.x, -0.5f, square.y);
                    normal = new float3(0f, -1f, 0f);
                    break;

                case 3:
                    point = new float3(square.x, +0.5f, square.y);
                    normal = new float3(0f, +1f, 0f);
                    break;

                case 4:
                    point = new float3(square, -0.5f);
                    normal = new float3(0f, 0f, -1f);
                    break;

                case 5:
                    point = new float3(square, +0.5f);
                    normal = new float3(0f, 0f, +1f);
                    break;

                default:
                    // shouldn't ever happen
                    point = default;
                    normal = default;
                    break;
            }
        }

        public float3 OnUnitSphere
        {
            get
            {
                // https://stackoverflow.com/questions/5531827/random-point-on-a-given-sphere
                var u = Value;
                var v = Value;
                var theta = MathX.TAU * u;
                var phi = MathX.Acos(2 * v - 1);

                var x = MathX.Sin(phi) * MathX.Cos(theta);
                var y = MathX.Sin(phi) * MathX.Sin(theta);
                var z = MathX.Cos(phi);

                return new float3(x, y, z);
            }
        }

        public float3 InsideUnitSphere
        {
            get
            {
                // based on: https://stackoverflow.com/questions/5408276/sampling-uniformly-distributed-random-points-inside-a-spherical-volume
                // TODO!!! Implement more efficient version? Discard method?
                var phi = Value * MathX.TAU;
                var costheta = Range(-1f, 1f);
                var u = Value;

                var theta = MathX.Acos(costheta);
                var r = MathX.Pow(u, 1f / 3f);

                var sinTheta = MathX.Sin(theta);

                return new float3(
                    r * sinTheta * MathX.Cos(phi),
                    r * sinTheta * MathX.Sin(phi),
                    r * MathX.Cos(theta)
                    );
            }
        }

        public float2 OnUnitCircle
        {
            get
            {
                var angle = Value * MathX.TAU;
                return new float2(MathX.Sin(angle), MathX.Cos(angle));
            }
        }

        public float2 InsideUnitCircle => OnUnitCircle * MathX.Sqrt(Value);

        public floatQ Rotation => new floatQ(Range(-1f, 1f), Range(-1f, 1f), Range(-1f, 1f), Range(-1f, 1f)).Normalized;

        public colorX RGB
        {
            get { return new colorX(Value, Value, Value, profile: ColorProfile.sRGB); }
        }

        public colorX RGBA
        {
            get { return new colorX(Value, Value, Value, Value, ColorProfile.sRGB); }
        }

        public colorX Grayscale
        {
            get
            {
                var v = Value;
                return new colorX(v, v, v, profile: ColorProfile.sRGB);
            }
        }

        public colorX Hue => new colorX(ColorHSV.Hue(Value), ColorProfile.Linear);

        // based on https://stackoverflow.com/questions/41749411/uniform-sampling-by-volume-within-a-cone

        public float3 InsideCone(float height, float baseRadius)
        {
            var halfHeight = height * 0.5f;

            var h = height * MathX.Pow(Value, 1/3f);
            var r = (baseRadius / height) * h * MathX.Sqrt(Value);
            var t = MathX.TAU * Value;

            return new float3(r * MathX.Cos(t), halfHeight - h, r * MathX.Sin(t));
        }

        public float3 OnCone(float height, float baseRadius)
        {
            var halfHeight = height * 0.5f;

            if (Value < baseRadius / (baseRadius + MathX.Sqrt(height*height + baseRadius* baseRadius)))
            {
                // point on base
                var h = height;
                var r = baseRadius * MathX.Sqrt(Value);
                var t = MathX.TAU * Value;

                return new float3(r * MathX.Cos(t), halfHeight - h, r * MathX.Sin(t));
            }
            else
            {
                // point on side
                var h = height * MathX.Sqrt(Value);
                var r = (baseRadius / height) * h;
                var t = MathX.TAU * Value;

                return new float3(r * MathX.Cos(t), halfHeight - h, r * MathX.Sin(t));
            }
        }

        // string
        public string String(string characters, int length)
        {
            var str = Pool.BorrowStringBuilder();

            for (int i = 0; i < length; i++)
                str.Append(characters[Range(characters.Length)]);

            var _str = str.ToString();
            Pool.Return(ref str);

            return _str;
        }

        public void Bytes(byte[] buffer) => random.NextBytes(buffer);
        public byte[] Bytes(int count)
        {
            var buffer = new byte[count];
            Bytes(buffer);
            return buffer;
        }

        public string LowercaseAlphabet(int length) => String(RandomX.LOWERCASE_ALPHABET, length);
        public string UppercaseAlphabet(int length) => String(RandomX.UPPERCASE_ALPHABET, length);
        public string Digits(int length) => String(RandomX.DIGITS, length);
        public string LettersAndDigits(int length) => String(RandomX.LETTERS_AND_DIGITS, length);

        public E Enum<E>()
        {
            var values = System.Enum.GetValues(typeof(E));
            var value = values.GetValue(RandomX.Range(0, values.Length));

            return (E)value;
        }
    }
}
