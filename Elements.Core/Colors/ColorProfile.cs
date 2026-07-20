using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Renderite.Shared;
using Elements.Data;

namespace Elements.Core
{
    [DataModelType]
    public enum ColorProfileAwareOperation
    {
        /// <summary>
        /// Prefer the left hand operand's color profile.
        /// </summary>
        UseLHS,
        /// <summary>
        /// Prefer the right hand operand's color profile.
        /// </summary>
        UseRHS,
        /// <summary>
        /// Apply the operation in linear space.
        /// </summary>
        UseLinear,
        /// <summary>
        /// Apply the operation in linear space only if the operands are not in the same profile.
        /// </summary>
        LinearIfUnequal
    }

    public readonly struct ColorAwareResult
    {
        /// <summary>
        /// Left hand side of the operation.
        /// </summary>
        public readonly color leftHand;
        /// <summary>
        /// Right hand side of the operation.
        /// </summary>
        public readonly color rightHand;
        /// <summary>
        /// Color profile derived from the operation.
        /// </summary>
        public readonly ColorProfile profile;

        public ColorAwareResult(in color left, in color right, in ColorProfile profile)
        {
            this.leftHand = left;
            this.rightHand = right;
            this.profile = profile;
        }
    }

    public static class ColorProfileHelper
    {
        public const float GAMMA_22 = 2.2f;
        public const float INVERSE_GAMMA_22 = 1.0f / GAMMA_22;

        public static bool HandlesAlpha(this ColorProfile profile) => profile switch
        {
            ColorProfile.sRGBAlpha => true,
            _ => false
        };

        /// <summary>
        /// Converts an input color from a given profile, to a different profile.
        /// </summary>
        /// <param name="inProfile">The color's profile.</param>
        /// <param name="outProfile">The target profile.</param>
        /// <param name="color">The color.</param>
        /// <returns>A converted color.</returns>
        public static color ConvertProfile(this in color color, ColorProfile inProfile, ColorProfile outProfile)
        {
            if (inProfile == outProfile)
                return color;

            switch (inProfile)
            {
                case ColorProfile.Linear:
                case ColorProfile.sRGB:
                case ColorProfile.sRGBAlpha:
                    return new color(
                        ConvertProfile(color.r, inProfile, outProfile),
                        ConvertProfile(color.g, inProfile, outProfile),
                        ConvertProfile(color.b, inProfile, outProfile),
                        ConvertProfile(color.a,
                            inProfile.HandlesAlpha() ? inProfile : ColorProfile.Linear,
                            outProfile.HandlesAlpha() ? outProfile : ColorProfile.Linear));

                default:
                    throw new Exception("Unsupported color profile for conversion!");
            }
        }

        /// <summary>
        /// Converts an input color in a given profile to linear
        /// </summary>
        /// <param name="profile">The color's profile.</param>
        /// <param name="color">The color.</param>
        /// <returns>A converted color.</returns>
        public static color ToLinear(this in color color, ColorProfile profile)
        {
            switch (profile)
            {
                case ColorProfile.Linear:
                case ColorProfile.sRGB:
                    return new color(
                        ProfileToLinear(color.r, profile),
                        ProfileToLinear(color.g, profile),
                        ProfileToLinear(color.b, profile),
                        color.a);

                case ColorProfile.sRGBAlpha:
                    return new color(
                        ProfileToLinear(color.r, profile),
                        ProfileToLinear(color.g, profile),
                        ProfileToLinear(color.b, profile),
                        ProfileToLinear(color.a, profile));

                default:
                    throw new Exception("Unsupported color profile for linearization!");
            }
        }

        /// <summary>
        /// Converts an input linear color to a particular profile
        /// </summary>
        /// <param name="profile">The target profile.</param>
        /// <param name="color">The color.</param>
        /// <returns>A converted color.</returns>
        public static color ToProfile(this in color color, ColorProfile profile)
        {
            switch (profile)
            {
                case ColorProfile.Linear:
                case ColorProfile.sRGB:
                    return new color(
                        LinearToProfile(color.r, profile),
                        LinearToProfile(color.g, profile),
                        LinearToProfile(color.b, profile),
                        color.a);

                case ColorProfile.sRGBAlpha:
                    return new color(
                        LinearToProfile(color.r, profile),
                        LinearToProfile(color.g, profile),
                        LinearToProfile(color.b, profile),
                        (profile == ColorProfile.Linear) ? LinearToProfile(color.a, ColorProfile.sRGB) : color.a);
                default:
                    throw new Exception("Unsupported color profile for linearization!");
            }
        }

        public static float ConvertProfile(float value, ColorProfile inProfile, ColorProfile outProfile) =>
            LinearToProfile(ProfileToLinear(value, inProfile), outProfile);

        /// <summary>
        /// Converts an already linear value to a profile.
        /// </summary>
        /// <param name="value">The already linearized input value.</param>
        /// <param name="profile">The target profile.</param>
        /// <returns>The profile-correct value.</returns>
        public static float LinearToProfile(float value, ColorProfile profile)
        {
            switch (profile)
            {
                // For reasons of "correctness" we assume that any values > 1 are by default linear (as they should be with HDR)

                case ColorProfile.sRGBAlpha:
                case ColorProfile.sRGB:
                    var sign = 1f;

                    if (value < 0)
                    {
                        value = -value;
                        sign = -1f;
                    }

                    if (value < 1.0f)
                        return ((value <= 0.0031308f) ? 12.92f * value : (MathX.Pow(value, 1f / 2.4f) * 1.055f - 0.055f)) * sign;
                    else
                        return value;

                case ColorProfile.Linear:
                    return value;

                default:
                    // If a new profile gets added and we don't implement it here, it'll yell at us to add it
                    throw new NotSupportedException($"Unsupported color profile: {profile}");
            }
        }

        /// <summary>
        /// Converts a value in a non-linear profile to linear.
        /// </summary>
        /// <param name="value">The non-linear value.</param>
        /// <param name="profile">The source profile.</param>
        /// <returns>A linear value.</returns>
        public static float ProfileToLinear(float value, ColorProfile profile)
        {
            switch (profile)
            {
                case ColorProfile.sRGBAlpha:
                case ColorProfile.sRGB:
                    var sign = 1f;

                    if (value < 0)
                    {
                        value = -value;
                        sign = -1f;
                    }

                    return (value < 1.0f ? (value <= 0.04045f) ? value / 12.92f : MathX.Pow((value + 0.055f) / 1.055f, 2.4f) : value) * sign;

                case ColorProfile.Linear:
                    return value;

                default:
                    // If a new profile gets added and we don't implement it here, it'll yell at us to add it
                    throw new NotSupportedException($"Unsupported color profile: {profile}");
            }
        }

        /// <summary>
        /// Used for color profile aware operations, such as lerp.
        /// </summary>
        /// <param name="lhs">Left hand color</param>
        /// <param name="rhs">Right hand color</param>
        /// <param name="option">The profile option, e.g., left hand profile, right hand profile, linear, etc.</param>
        /// <returns>The colors in the targeted color profile.</returns>
        /// <exception cref="Exception"></exception>
        public static ColorAwareResult GetOperands(in colorX lhs, in colorX rhs, ColorProfileAwareOperation option)
        {
            switch (option)
            {
                case ColorProfileAwareOperation.UseLHS:
                    return new ColorAwareResult(lhs.baseColor, rhs.ConvertProfile(lhs.profile).baseColor, lhs.profile);

                case ColorProfileAwareOperation.UseRHS:
                    return new ColorAwareResult(lhs.ConvertProfile(rhs.profile).baseColor, rhs.baseColor, rhs.profile);

                case ColorProfileAwareOperation.UseLinear:
                    return new ColorAwareResult(lhs.ConvertProfile(ColorProfile.Linear).baseColor,
                        rhs.ConvertProfile(ColorProfile.Linear).baseColor, ColorProfile.Linear);

                case ColorProfileAwareOperation.LinearIfUnequal:
                    return (lhs.profile != rhs.profile)
                        ? new ColorAwareResult(lhs.ConvertProfile(ColorProfile.Linear).baseColor,
                            rhs.ConvertProfile(ColorProfile.Linear).baseColor, ColorProfile.Linear)
                        : new ColorAwareResult(lhs.baseColor, rhs.baseColor, lhs.profile);
                default:
                    throw new Exception("Unsupported color space aware operation!");
            }
        }
    }
}
