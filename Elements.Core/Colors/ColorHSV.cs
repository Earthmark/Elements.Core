using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using Newtonsoft.Json;
using JsonIgnore = System.Text.Json.Serialization.JsonIgnoreAttribute;
using Renderite.Shared;

namespace Elements.Core
{
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public struct ColorHSV
    {
        const float SIXTY_R = 60f / 360f;

        public float h, s, v, a;

        [JsonPropertyName("h")]
        [JsonProperty(PropertyName = "h")]
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public readonly float H => h;

        [JsonPropertyName("s")]
        [JsonProperty(PropertyName = "s")]
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public readonly float S => s;

        [JsonPropertyName("v")]
        [JsonProperty(PropertyName = "v")]
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public readonly float V => v;

        [JsonPropertyName("a")]
        [JsonProperty(PropertyName = "a")]
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public readonly float A => a;

        public static color Hue(float hue) => new ColorHSV(hue, 1f, 1f);

        public static float GetHue(in color rgb) => (new ColorHSV(rgb)).h;
        public static float GetSaturation(in color rgb) => (new ColorHSV(rgb)).s;
        public static float GetValue(in color rgb) => (new ColorHSV(rgb)).v;
        public static float GetHue(in colorX rgb) => (new ColorHSV(rgb)).h;
        public static float GetSaturation(in colorX rgb) => (new ColorHSV(rgb)).s;
        public static float GetValue(in colorX rgb) => (new ColorHSV(rgb)).v;

        [System.Text.Json.Serialization.JsonConstructor]
        [Newtonsoft.Json.JsonConstructor]
        public ColorHSV(float h, float s, float v, float a = 1f)
        {
            this.h = h;
            this.s = s;
            this.v = v;
            this.a = a;
        }

        public ColorHSV(in color cRGB)
        {
            h = s = v = a = 0f;
            FromRGB(cRGB);
        }

        // HSV is considered to be color space in-variant as it is a relative color space.  We use the base color as such.
        public ColorHSV(in colorX cRGB)
        {
            h = s = v = a = 0f;
            FromRGB(cRGB.baseColor);
        }

        public void FromRGB(in color cRGB)
        {
            var c = cRGB.NormalizeHDR(out float gain);

            float cmax = MathX.Max(c.r, c.g, c.b);
            float cmin = MathX.Min(c.r, c.g, c.b);
            float delta = cmax - cmin;

            if (delta == 0)
                h = 0;
            else if (cmax == c.r)
                h = MathX.Repeat((c.g - c.b) / delta, 6f) / 6f;
            else if (cmax == c.g)
                h = (((c.b - c.r) / delta) + 2) / 6f;
            else /*if (cmax == cRGB.b)*/
                h = (((c.r - c.g) / delta) + 4) / 6f;

            if (cmax == 0f)
                s = 0f;
            else
                s = delta / cmax;

            v = cmax * gain;

            a = c.a;
        }

        public color ToRGB()
        {
            var gain = MathX.Max(v, 1f);
            v /= gain;

            float c = v * s;
            float x = c * (1 - MathX.Abs(MathX.Repeat(h * 6f, 2f) - 1));
            float m = v - c;

            color cRGB;

            switch ((int)(MathX.Repeat(h, 1f) / SIXTY_R))
            {
                case 0: cRGB = new color(c, x, 0); break;
                case 1: cRGB = new color(x, c, 0); break;
                case 2: cRGB = new color(0, c, x); break;
                case 3: cRGB = new color(0, x, c); break;
                case 4: cRGB = new color(x, 0, c); break;
                case 5: cRGB = new color(c, 0, x); break;

                default: cRGB = new color(0, 0, 0); break;
            }

            return new color(cRGB.rgb + m, a).ConstructHDR(gain);
        }


        public static implicit operator color(ColorHSV cHSV) => cHSV.ToRGB();
        public static implicit operator ColorHSV(color cRGB) => new ColorHSV(cRGB);

        public static ColorHSV operator +(ColorHSV a, ColorHSV b) => new ColorHSV(a.h + b.h, a.s + b.s, a.v + b.v, a.a + b.a);
        public static ColorHSV operator *(ColorHSV c, float n) => new ColorHSV(c.h * n, c.s * n, c.v * n, c.a * n);

        public colorX ToRGB(ColorProfile profile) => new colorX(ToRGB(), profile);

        public override string ToString()
        {
            return $"[H={h:F2}, S={s:F2}, V={v:F2}, A={a:F2}]";
        }
    }
}
