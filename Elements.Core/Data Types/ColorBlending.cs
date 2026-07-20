using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Renderite.Shared;

namespace Elements.Core
{
    public partial struct color
    {
        public color SetHue(float hue)
        {
            var hsv = new ColorHSV(this);
            hsv.h = hue;
            return hsv;
        }

        public color SetSaturation(float saturation)
        {
            var hsv = new ColorHSV(this);
            hsv.s = saturation;
            return hsv;
        }

        public color SetValue(float value)
        {
            var hsv = new ColorHSV(this);
            hsv.v = value;
            return hsv;
        }

        public color AddHue(float hue)
        {
            var hsv = new ColorHSV(this);
            hsv.h += hue;
            return hsv;
        }

        public color AddSaturation(float saturation)
        {
            var hsv = new ColorHSV(this);
            hsv.s = MathX.Clamp01(saturation + hsv.s);
            return hsv;
        }

        public color AddValue(float value)
        {
            var hsv = new ColorHSV(this);
            hsv.v = MathX.Clamp01(value + hsv.v);
            return hsv;
        }

        public color AddValueHDR(float value)
        {
            var hsv = new ColorHSV(this);
            hsv.v += value;
            return hsv;
        }

        public color MulHue(float hue)
        {
            var hsv = new ColorHSV(this);
            hsv.h *= hue;
            return hsv;
        }

        public color MulSaturation(float saturation)
        {
            var hsv = new ColorHSV(this);
            hsv.s *= saturation;
            return hsv;
        }

        public color MulValue(float value)
        {
            var hsv = new ColorHSV(this);
            hsv.v *= value;
            return hsv;
        }

        public static color FromHexCode(string hex, color failColor = default)
        {
            if (FromHexCode(hex, out color c))
                return c;

            return failColor;
        }

        public static color FromHexCode(ReadOnlySpan<char> hex, color failColor = default)
        {
            if (FromHexCode(hex, out color c))
                return c;

            return failColor;
        }

        public static bool FromHexCode(ReadOnlySpan<char> hex, out color color, float defaultAlpha = 1f)
            => FromHexCode(hex, out color, out _, defaultAlpha);

        public static bool FromHexCode(ReadOnlySpan<char> hex, out color color, out bool hasAlpha, float defaultAlpha = 1f)
        {
            var success = color32.FromHexCode(hex, out color32 c32, out hasAlpha, (byte)MathX.Clamp(defaultAlpha*255, 0, 255));
            color = c32;
            return success;
        }

        public static bool FromHexCode(string hex, out color color)
        {
            var success = color32.FromHexCode(hex, out color32 c32);
            color = c32;
            return success;
        }

        public static color AlphaBlend(color src, color dst)
        {
            float src_a = MathX.Clamp01(src.a);

            return new color(src.rgb * src_a + dst.rgb * (1 - src_a), MathX.Min(1, src.a + dst.a));
        }

        public static color AdditiveBlend(color src, color dst)
        {
            return new color(src.rgb + dst.rgb, MathX.Min(1, src.a + dst.a));
        }

        public static color SoftAdditiveBlend(color src, color dst)
        {
            return new color(src.rgb * (1 - dst.rgb) + dst.rgb, MathX.Min(1, src.a + dst.a));
        }

        public static color MultiplicativeBlend(color src, color dst)
        {
            return src * dst;
        }
    }

    public partial struct colorX
    {
        public colorX SetHue(float hue)
        {
            var hsv = new ColorHSV(this);
            hsv.h = hue;
            return hsv.ToRGB(profile);
        }

        public colorX SetSaturation(float saturation)
        {
            var hsv = new ColorHSV(this);
            hsv.s = saturation;
            return hsv.ToRGB(profile);
        }

        public colorX SetValue(float value)
        {
            var hsv = new ColorHSV(this);
            hsv.v = value;
            return hsv.ToRGB(profile);
        }

        public colorX AddHue(float hue)
        {
            var hsv = new ColorHSV(this);
            hsv.h += hue;
            return hsv.ToRGB(profile);
        }

        public colorX AddSaturation(float saturation)
        {
            var hsv = new ColorHSV(this);
            hsv.s = MathX.Clamp01(saturation + hsv.s);
            return hsv.ToRGB(profile);
        }

        public colorX AddValue(float value)
        {
            var hsv = new ColorHSV(this);
            hsv.v = MathX.Clamp01(value + hsv.v);
            return hsv.ToRGB(profile);
        }

        public colorX AddValueHDR(float value)
        {
            var hsv = new ColorHSV(this);
            hsv.v += value;
            return hsv.ToRGB(profile);
        }

        public colorX MulHue(float hue)
        {
            var hsv = new ColorHSV(this);
            hsv.h *= hue;
            return hsv.ToRGB(profile);
        }

        public colorX MulSaturation(float saturation)
        {
            var hsv = new ColorHSV(this);
            hsv.s *= saturation;
            return hsv.ToRGB(profile);
        }

        public colorX MulValue(float value)
        {
            var hsv = new ColorHSV(this);
            hsv.v *= value;
            return hsv.ToRGB(profile);
        }

        public static colorX FromHexCode(string hex, colorX failColor = default, ColorProfile profile = ColorProfile.sRGB)
        {
            if (FromHexCode(hex, out colorX c, profile))
                return c;

            return failColor;
        }

        public static colorX FromHexCode(ReadOnlySpan<char> hex, colorX failColor = default)
        {
            if (FromHexCode(hex, out colorX c))
                return c;

            return failColor;
        }

        public static bool FromHexCode(ReadOnlySpan<char> hex, out colorX color, float defaultAlpha = 1f)
            => FromHexCode(hex, out color, out _, defaultAlpha);

        public static bool FromHexCode(ReadOnlySpan<char> hex, out colorX color, out bool hasAlpha, float defaultAlpha = 1f)
        {
            var success = color32.FromHexCode(hex, out color32 c32, out hasAlpha, (byte)MathX.Clamp(defaultAlpha*255, 0, 255));
            color = new colorX(c32);
            return success;
        }

        public static bool FromHexCode(string hex, out colorX color, ColorProfile profile = ColorProfile.Linear)
        {
            var success = color32.FromHexCode(hex, out color32 c32);
            color = new colorX(c32, profile);
            return success;
        }

        public static colorX AlphaBlend(colorX src, colorX dst)
        {
            float src_a = MathX.Clamp01(src.a);

            return new colorX(src.rgb * src_a + dst.rgb * (1 - src_a), MathX.Min(1, src.a + dst.a));
        }

        public static colorX AdditiveBlend(colorX src, colorX dst)
        {
            return new colorX(src.rgb + dst.rgb, MathX.Min(1, src.a + dst.a));
        }

        public static colorX SoftAdditiveBlend(colorX src, colorX dst)
        {
            return new colorX(src.rgb * (1 - dst.rgb) + dst.rgb, MathX.Min(1, src.a + dst.a));
        }

        public static colorX MultiplicativeBlend(colorX src, colorX dst)
        {
            return src * dst;
        }
    }
}
