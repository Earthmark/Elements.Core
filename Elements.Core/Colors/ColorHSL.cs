using Renderite.Shared;

namespace Elements.Core
{
    public struct ColorHSL
    {
        public float h, s, l, a;

        const float SIXTY_R = 60f / 360f;

        public ColorHSL(float h, float s, float l, float a = 1f)
        {
            this.h = h;
            this.s = s;
            this.l = l;
            this.a = a;
        }

        public ColorHSL(color cRGB)
        {
            h = s = l = a = 0f;
            FromRGB(cRGB);
        }

        public ColorHSL(colorX cRGB)
        {
            h = s = l = a = 0f;
            FromRGB(cRGB.baseColor);
        }

        public void FromRGB(color cRGB)
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

            l = (cmax + cmin) / 2f;

            if (delta == 0f)
                s = 0f;
            else
                s = delta / (1 - MathX.Abs(2 * l - 1));

            l *= gain;

            a = cRGB.a;
        }

        public color ToRGB()
        {
            var gain = MathX.Max(l, 1f);
            l /= gain;

            float c = (1f - MathX.Abs(2 * l - 1)) * s;
            float x = c * (1 - MathX.Abs(MathX.Repeat(h / SIXTY_R, 2f) - 1));
            float m = l - c / 2f;

            color cRGB = new color();

            switch ((int)(h / SIXTY_R))
            {
                case 0: cRGB = new color(c, x, 0); break;
                case 1: cRGB = new color(x, c, 0); break;
                case 2: cRGB = new color(0, c, x); break;
                case 3: cRGB = new color(0, x, c); break;
                case 4: cRGB = new color(x, 0, c); break;
                case 5: cRGB = new color(c, 0, x); break;
            }

            return new color(cRGB.rgb + m, a).ConstructHDR(gain);
        }

        public colorX ToRGB(ColorProfile profile) => new colorX(ToRGB(), profile);

        public static implicit operator color(ColorHSL cHSL) { return cHSL.ToRGB(); }
        public static implicit operator ColorHSL(color cRGB) { return new ColorHSL(cRGB); }
        public static implicit operator colorX(ColorHSL cHSL) { return cHSL.ToRGB(ColorProfile.sRGB); }
    }

}
