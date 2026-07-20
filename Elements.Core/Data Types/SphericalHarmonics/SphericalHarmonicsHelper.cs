using Renderite.Shared;
using System;
using System.Collections.Generic;
using System.Text;

namespace Elements.Core
{
    // Based on: https://github.com/google/spherical-harmonics/blob/master/sh/spherical_harmonics.cc#L103
    public static class SphericalHarmonicsHelper
    {
        public const float SH00_MAGNITUDE = 0.282095f;

        public static RenderSH2 ToRender(this SphericalHarmonicsL2<colorX> sh, ColorProfile targetProfile = ColorProfile.Linear)
        {
            return new RenderSH2(
                sh.sh0.ToProfile(targetProfile).rgb,
                sh.sh1.ToProfile(targetProfile).rgb,
                sh.sh2.ToProfile(targetProfile).rgb,
                sh.sh3.ToProfile(targetProfile).rgb,
                sh.sh4.ToProfile(targetProfile).rgb,
                sh.sh5.ToProfile(targetProfile).rgb,
                sh.sh6.ToProfile(targetProfile).rgb,
                sh.sh7.ToProfile(targetProfile).rgb,
                sh.sh8.ToProfile(targetProfile).rgb
                );
        }

        public static bool IsSupported(Type sphericalHarmonicType)
        {
            var size = sphericalHarmonicType.SphericalHarmonicSize();

            if (size > ReflectionExtensions.MAX_VALUE_SIZE)
                return false;

            return Coder.IsEnginePrimitive(sphericalHarmonicType);
        }

        public static T FilterInvalid<T>(T sh, T fallback = default)
            where T : ISphericalHarmonics
        {
            if (sh.IsValid)
                return sh;

            return fallback;
        }

        public static int CoefficientCount(int order) => (order + 1) * (order + 1);
        public static int CoefficientIndex(int order, int degree) => order * (order + 1) + degree;

        public static T Evaluate<T>(this ISphericalHarmonics<T> harmonics, float3 dir)
            where T : unmanaged
        {
            T sum = default;

            // Remap the direction to the coordinate space we use
            dir = new float3(-dir.x, -dir.y, dir.z);

            for (int order = 0; order <= harmonics.Order; order++)
                for (int degree = -order; degree <= order; degree++)
                {
                    var value = harmonics[CoefficientIndex(order, degree)];
                    var scale = EvaluateScale(order, degree, dir);

                    sum = Coder<T>.Add(sum, Coder<T>.Scale(value, scale));
                }

            return sum;
        }

        public static string FormatToString<T>(this ISphericalHarmonics<T> harmonics)
            where T : unmanaged
        {
            var str = new StringBuilder();

            str.Append("[");

            var coeffCount = CoefficientCount(harmonics.Order);

            for (int i = 0; i < coeffCount; i++)
            {
                str.Append(harmonics[i].ToString());

                if (i != coeffCount - 1)
                    str.Append(";");
            }

            str.Append("]");

            return str.ToString();
        }

        // IMPORTANT!!! This method assumes that the direction has been transformed to the coordinate space of
        // the original math. It should only be used internally in this helper, otherwise the directions are
        // not gonna match our coordinate space. The X and Y axes need to be flipped.
        // The other alternative would have been to flip all the evaluation math, but that will diverge it from
        // the original source equations and potentially sneak in errors there, so I have avoided doing that for now
        // The flipping happening just once in the main evaluation method is pretty cheap (especially compared to
        // all the math of the harmonics itself)
        // TODO: Optimize the evaluation math by flipping all the directions in the equations themselves. But if we
        // were to do that, please make unit tests first and test that everything matches before and after!
        static float EvaluateScale(int order, int degree, in float3 dir)
        {
            switch(order)
            {
                case 0: return HardcodedSH00(dir);
                case 1: return EvalOrder1(degree, dir);
                case 2: return EvalOrder2(degree, dir);
                case 3: return EvalOrder3(degree, dir);
                case 4: return EvalOrder4(degree, dir);

                default:
                    throw new ArgumentOutOfRangeException(nameof(order));
            }
        }

        #region EVALUATION Order 0

        // 0.5 * sqrt(1/pi)
        public static float HardcodedSH00(in float3 dir) => SH00_MAGNITUDE;

        #endregion

        #region EVALUATION Order 1

        public static float EvalOrder1(int m, in float3 dir)
        {
            switch(m)
            {
                case -1: return HardcodedSH1n1(dir);
                case 0: return HardcodedSH10(dir);
                case 1: return HardcodedSH1p1(dir);

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        // -sqrt(3/(4pi)) * y
        public static float HardcodedSH1n1(in float3 dir) => -0.488603f * dir.y;

        // sqrt(3/(4pi)) * z
        public static float HardcodedSH10(in float3 dir) => 0.488603f * dir.z;

        // -sqrt(3/(4pi)) * x
        public static float HardcodedSH1p1(in float3 dir) => -0.488603f * dir.x;

        #endregion

        #region EVALUATION Order 2

        public static float EvalOrder2(int m, in float3 dir)
        {
            switch(m)
            {
                case -2: return HardcodedSH2n2(dir);
                case -1: return HardcodedSH2n1(dir);
                case 0: return HardcodedSH20(dir);
                case 1: return HardcodedSH2p1(dir);
                case 2: return HardcodedSH2p2(dir);

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        // 0.5 * sqrt(15/pi) * x * y
        public static float HardcodedSH2n2(in float3 dir) => 1.092548f * dir.x * dir.y;

        // -0.5 * sqrt(15/pi) * y * z
        public static float HardcodedSH2n1(in float3 dir) => -1.092548f * dir.y * dir.z;

        // 0.25 * sqrt(5/pi) * (-x^2-y^2+2z^2)
        public static float HardcodedSH20(in float3 dir) => 0.315392f * (-dir.x * dir.x - dir.y * dir.y + 2.0f * dir.z * dir.z);

        // -0.5 * sqrt(15/pi) * x * z
        public static float HardcodedSH2p1(in float3 dir) => -1.092548f * dir.x * dir.z;

        // 0.25 * sqrt(15/pi) * (x^2 - y^2)
        public static float HardcodedSH2p2(in float3 dir) => 0.546274f * (dir.x * dir.x - dir.y * dir.y);

        #endregion

        #region EVALUATION Order 3

        public static float EvalOrder3(int m, in float3 dir)
        {
            switch(m)
            {
                case -3: return HardcodedSH3n3(dir);
                case -2: return HardcodedSH3n2(dir);
                case -1: return HardcodedSH3n1(dir);
                case 0: return HardcodedSH30(dir);
                case 1: return HardcodedSH3p1(dir);
                case 2: return HardcodedSH3p2(dir);
                case 3: return HardcodedSH3p3(dir);

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        // -0.25 * sqrt(35/(2pi)) * y * (3x^2 - y^2)
        public static float HardcodedSH3n3(in float3 dir) => -0.590044f * dir.y * (3.0f * dir.x * dir.x - dir.y * dir.y);

        // 0.5 * sqrt(105/pi) * x * y * z
        public static float HardcodedSH3n2(in float3 dir) => 2.890611f * dir.x * dir.y * dir.z;

        // -0.25 * sqrt(21/(2pi)) * y * (4z^2-x^2-y^2)
        public static float HardcodedSH3n1(in float3 dir) => -0.457046f * dir.y * (4.0f * dir.z * dir.z - dir.x * dir.x - dir.y * dir.y);

        // 0.25 * sqrt(7/pi) * z * (2z^2 - 3x^2 - 3y^2)
        public static float HardcodedSH30(in float3 dir) => 0.373176f * dir.z * (2.0f * dir.z * dir.z - 3.0f * dir.x * dir.x - 3.0f * dir.y * dir.y);

        // -0.25 * sqrt(21/(2pi)) * x * (4z^2-x^2-y^2)
        public static float HardcodedSH3p1(in float3 dir) => -0.457046f * dir.x * (4.0f * dir.z * dir.z - dir.x * dir.x - dir.y * dir.y);

        // 0.25 * sqrt(105/pi) * z * (x^2 - y^2)
        public static float HardcodedSH3p2(in float3 dir) => 1.445306f * dir.z * (dir.x * dir.x - dir.y * dir.y);

        // -0.25 * sqrt(35/(2pi)) * x * (x^2-3y^2)
        public static float HardcodedSH3p3(in float3 dir) => -0.590044f * dir.x * (dir.x * dir.x - 3.0f * dir.y * dir.y);

        #endregion

        #region EVALUATION Order 4

        public static float EvalOrder4(int m, in float3 dir)
        {
            switch(m)
            {
                case -4: return HardcodedSH4n4(dir);
                case -3: return HardcodedSH4n3(dir);
                case -2: return HardcodedSH4n2(dir);
                case -1: return HardcodedSH4n1(dir);
                case 0: return HardcodedSH40(dir);
                case 1: return HardcodedSH4p1(dir);
                case 2: return HardcodedSH4p2(dir);
                case 3: return HardcodedSH4p3(dir);
                case 4: return HardcodedSH4p4(dir);

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        // 0.75 * sqrt(35/pi) * x * y * (x^2-y^2)
        public static float HardcodedSH4n4(in float3 dir) => 2.503343f * dir.x * dir.y * (dir.x * dir.x - dir.y * dir.y);

        // -0.75 * sqrt(35/(2pi)) * y * z * (3x^2-y^2)
        public static float HardcodedSH4n3(in float3 dir) => -1.770131f * dir.y * dir.z * (3.0f * dir.x * dir.x - dir.y * dir.y);

        // 0.75 * sqrt(5/pi) * x * y * (7z^2-1)
        public static float HardcodedSH4n2(in float3 dir) => 0.946175f * dir.x * dir.y * (7.0f * dir.z * dir.z - 1.0f);

        // -0.75 * sqrt(5/(2pi)) * y * z * (7z^2-3)
        public static float HardcodedSH4n1(in float3 dir) => -0.669047f * dir.y * dir.z * (7.0f * dir.z * dir.z - 3.0f);

        // 3/16 * sqrt(1/pi) * (35z^4-30z^2+3)
        public static float HardcodedSH40(in float3 dir)
        {
            var zz = dir.z * dir.z;
            return 0.105786f * (35.0f * zz * zz - 30.0f * zz + 3.0f);
        }

        // -0.75 * sqrt(5/(2pi)) * x * z * (7z^2-3)
        public static float HardcodedSH4p1(in float3 dir) => -0.669047f * dir.x * dir.z * (7.0f * dir.z * dir.z - 3.0f);

        // 3/8 * sqrt(5/pi) * (x^2 - y^2) * (7z^2 - 1)
        public static float HardcodedSH4p2(in float3 dir) => 0.473087f * (dir.x * dir.x - dir.y * dir.y) * (7.0f * dir.z * dir.z - 1.0f);

        // -0.75 * sqrt(35/(2pi)) * x * z * (x^2 - 3y^2)
        public static float HardcodedSH4p3(in float3 dir) => -1.770131f * dir.x * dir.z * (dir.x * dir.x - 3.0f * dir.y * dir.y);

        // 3/16*sqrt(35/pi) * (x^2 * (x^2 - 3y^2) - y^2 * (3x^2 - y^2))
        public static float HardcodedSH4p4(in float3 dir)
        {
            var xx = dir.x * dir.x;
            var yy = dir.y * dir.y;

            return 0.625836f * (xx * (xx - 3.0f * yy) - yy * (3.0f * xx - yy));
        }

        #endregion
    }
}
