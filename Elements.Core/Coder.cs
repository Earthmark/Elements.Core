using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.CompilerServices;

namespace Elements.Core
{
    enum UriData
    {
        Null,
        Absolute,
        Relative
    }

    delegate bool Equaler<E>(E a, E b);
    delegate int Comparer<E>(E a, E b);

    delegate E UniOperator<E>(E a);
    delegate E BiOperator<E>(E a, E b);
    delegate E TriOperator<E>(E a, E b, E c);

    delegate bool BoolOperator<E>(E a);
    delegate bool BiBoolOperator<E>(E a, E b);

    delegate E FloatOperator<E>(E e, float s);

    delegate E Lerper<E>(E a, E b, float ratio);
    delegate float InverseLerper<E>(E a, E b, E value);
    delegate E ConstantLerper<E>(E current, E target, float delta);
    delegate E SmoothLerper<E>(E current, E target, ref E intermediate, float delta);

    delegate void Encoder<E>(E value, BinaryWriter bw);
    delegate E Decoder<E>(BinaryReader br);

    delegate E QuantizedEncoder<E>(E value, E min, E max, int bits, BitBinaryWriterX bw);
    delegate E QuantizedDecoder<E>(E min, E max, int bits, BitBinaryReaderX br);

    delegate DataTreeNode Saver<E>(E value);
    delegate E Loader<E>(DataTreeNode node);

    delegate string StringEncoder<E>(E value);
    delegate E StringDecoder<E>(string str);

    delegate float Measurer<E>(E a, E b);

    delegate bool TryParser<E>(string str, out E value);

    public static partial class Coder
    {
        public static int BaseEnginePrimitiveCount => _baseEnginePrimitives.Count;

        /// <summary>
        /// Indicates is given type is a base engine primitive - this includes numeric types, bool and strings.
        /// The set of base primitives is fixed and updated rarely - it is not extensible by users.
        /// This means types like enums and such are excluded from this, because those can be freely declared.
        /// </summary>
        /// <param name="type">The type to determine if it's engine primitive or not</param>
        /// <returns>Whether given type is base engine primitive or not</returns>
        public static bool IsBaseEnginePrimitive(this Type type) => _baseEnginePrimitives.Contains(type);

        /// <summary>
        /// List of all base engine primitives
        /// </summary>
        public static IEnumerable<Type> BaseEnginePrimitives => _baseEnginePrimitives;

        /// <summary>
        /// This indicates if given type is engine primitive - either base or expanded. This means that it
        /// can be potentially used within data model, because it has all necessary encoding functions defined.
        /// However it still must be checked if it's an allowed type within given session (or within engine at all).
        /// E.g. any Enums can be generically included in the data model - but they must be marked explicitly to be
        /// allowed to be included int he data model.
        /// </summary>
        /// <param name="type">Type to determine</param>
        /// <returns>Whether it's an engine primitive or not</returns>
        public static bool IsEnginePrimitive(this Type type)
        {
            if (type.IsBaseEnginePrimitive())
                return true;

            // It's a generic type definition, this means we can't call any methods on it at all
            if (type.ContainsGenericParameters)
                return false;

            return (bool)typeof(Coder<>).MakeGenericType(type).GetProperty("IsEnginePrimitive").GetValue(null);
        }

        public static bool SupportsScale(this Type type)
        {
            return (bool)typeof(Coder<>).MakeGenericType(type).GetProperty("SupportsScale").GetValue(null);
        }

        public static bool SupportsConstantLerp(this Type type)
        {
            return (bool)typeof(Coder<>).MakeGenericType(type).GetProperty("SupportsConstantLerp").GetValue(null);
        }

        public static bool SupportsSmoothLerp(this Type type)
        {
            return (bool)typeof(Coder<>).MakeGenericType(type).GetProperty("SupportsSmoothLerp").GetValue(null);
        }

        public static object GetIdentity(this Type type)
        {
            return typeof(Coder<>).MakeGenericType(type).GetProperty("Identity").GetValue(null);
        }

        public static object GetDefault(this Type type)
        {
            return typeof(Coder<>).MakeGenericType(type).GetProperty("Default").GetValue(null);
        }
    }

    public static partial class Coder<T>
    {
        static readonly Equaler<T> _equaler;
        static readonly Equaler<T> _approximately;
        static readonly Comparer<T> _comparer;

        static readonly BiBoolOperator<T> _lessThan;
        static readonly BiBoolOperator<T> _lessThanOrEqual;
        static readonly BiBoolOperator<T> _greaterThan;
        static readonly BiBoolOperator<T> _greaterThanOrEqual;

        static readonly Measurer<T> _distance;

        static readonly UniOperator<T> _neg;
        static readonly UniOperator<T> _abs;

        static readonly UniOperator<T> _round;

        static readonly BoolOperator<T> _validCheck;
        static readonly BiOperator<T> _invalidFilter;

        static readonly BiOperator<T> _add;
        static readonly BiOperator<T> _sub;
        static readonly BiOperator<T> _mul;
        static readonly BiOperator<T> _div;
        static readonly BiOperator<T> _mod;
        static readonly BiBoolOperator<T> _canDivide;
        static readonly BoolOperator<T> _canDivideBy;

        static readonly BiOperator<T> _min;
        static readonly BiOperator<T> _max;

        static readonly BiOperator<T> _repeat;

        static readonly TriOperator<T> _clamp;

        static readonly FloatOperator<T> _shift;
        static readonly FloatOperator<T> _scale;
        static readonly FloatOperator<T> _power;
        static readonly FloatOperator<T> _powerMagnitude;

        static readonly Lerper<T> _lerper;
        static readonly Lerper<T> _lerperUnclamped;
        static readonly InverseLerper<T> _inverseLerper;
        static readonly ConstantLerper<T> _constantLerper;
        static readonly SmoothLerper<T> _smoothLerper;

        static readonly Encoder<T> _encoder;
        static readonly Decoder<T> _decoder;

        static readonly QuantizedEncoder<T> _qEncoder;
        static readonly QuantizedDecoder<T> _qDecoder;

        static readonly Saver<T> _saver;
        static readonly Loader<T> _loader;

        static readonly StringEncoder<T> _strEncoder;
        static readonly StringDecoder<T> _strDecoder;

        static readonly TryParser<T> _tryParser;

        public static T Identity { get; private set; }
        public static T Default { get; private set; }
        public static T MinValue { get; private set; }
        public static T MaxValue { get; private set; }

        public static bool IsSupported { get; private set; }
        public static bool IsEnginePrimitive { get; private set; }

        public static bool SupportsApproximateComparison => _approximately != null;
        public static bool SupportsComparison => _comparer != null;
        public static bool SupportsDistance => _distance != null;
        public static bool SupportsAddSub => _add != null && _sub != null;
        public static bool SupportsNegate => _neg != null;
        public static bool SupportsMul => _mul != null;
        public static bool SupportsScale => _scale != null;
        public static bool SupportsDiv => _div != null;
        public static bool SupportsMod => _mod != null;
        public static bool SupportsMinMax => _min != null && _max != null;
        public static bool SupportsAbs => _abs != null;
        public static bool SupportsEncoding => _encoder != null && _decoder != null;
        public static bool SupportsStringCoding => _strEncoder != null && _strDecoder != null;
        public static bool SupportsLerp => _lerper != null;
        public static bool SupportsInverseLerp => _inverseLerper != null;
        public static bool SupportsConstantLerp => _constantLerper != null;
        public static bool SupportsSmoothLerp => _smoothLerper != null;
        public static bool SupportsRepeat => _repeat != null;
        public static bool SupportsFilterInvalid => _invalidFilter != null;

        public static void Dummy() {  }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Equals(T a, T b) => _equaler(a, b);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Approximately(T a, T b) => _approximately(a, b);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Compare(T a, T b) => _comparer(a, b);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool LessThan(T a, T b) => _lessThan(a, b);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool LessOrEqual(T a, T b) => _lessThanOrEqual(a, b);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool GreaterThan(T a, T b) => _greaterThan(a, b);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool GreaterOrEqual(T a, T b) => _greaterThanOrEqual(a, b);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Distance(T a, T b)
        {
            return _distance(a, b);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Negate(T a)
        {
            return _neg(a);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Abs(T a)
        {
            return _abs(a);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Round(T a)
        {
            return _round(a);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsValid(T a)
        {
            return _validCheck(a);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T FilterInvalid(T a, T fallback = default)
        {
            return _invalidFilter(a, fallback);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Add(T a, T b)
        {
            return _add(a, b);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Sub(T a, T b)
        {
            return _sub(a, b);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Mul(T a, T b)
        {
            return _mul(a, b);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Div(T a, T b)
        {
            return _div(a, b);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Mod(T a, T b)
        {
            return _mod(a, b);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool CanDivide(T dividend, T divisor)
        {
            return _canDivide(dividend, divisor);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool CanDivideBy( T divisor)
        {
            return _canDivideBy(divisor);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Min(T a, T b)
        {
            return _min(a, b);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Max(T a, T b)
        {
            return _max(a, b);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Repeat(T value, T max)
        {
            return _repeat(value, max);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Clamp(T n, T min, T max)
        {
            return _clamp(n, min, max);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Scale(T t, float s)
        {
            return _scale(t, s);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Pow(T t, float s)
        {
            return _power(t, s);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T PowMagnitude(T t, float s)
        {
            return _powerMagnitude(t, s);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Shift(T t, float s)
        {
            return _shift(t, s);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Lerp(T a, T b, float ratio)
        {
            return _lerper(a, b, ratio);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T LerpUnclamped(T a, T b, float ratio)
        {
            return _lerperUnclamped(a, b, ratio);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float InverseLerp(T a, T b, T value)
        {
            return _inverseLerper(a, b, value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T ConstantLerp(T current, T target, float delta)
        {
            return _constantLerper(current, target, delta);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T SmoothLerp(T current, T target, ref T intermediate, float delta)
        {
            return _smoothLerper(current, target, ref intermediate, delta);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Encode(T value, BinaryWriter bw)
        {
            _encoder(value, bw);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Decode(BinaryReader br)
        {
            return _decoder(br);
        }

        // Returns the written value that was quantized
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T EncodeQuantized(T value, T min, T max, int bits, BitBinaryWriterX bw)
        {
            return _qEncoder(value, min, max, bits, bw);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T DecodeQuantized(T min, T max, int bits, BitBinaryReaderX br)
        {
            return _qDecoder(min, max, bits, br);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DataTreeNode Save(T value)
        {
            return _saver(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Load(DataTreeNode node)
        {
            return _loader(node);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string EncodeToString(T value) => _strEncoder(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T DecodeFromString(string str) => _strDecoder(str);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryParse(string str, out T value) => _tryParser(str, out value);
    }
}
