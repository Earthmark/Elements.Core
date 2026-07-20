using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Renderite.Shared;

namespace Elements.Core
{
    public static partial class MathX
    {
        /// <summary>
        /// Provides a conversion from wavelength to color
        /// <para>
        /// Based on lambda calculus from part 4.2 of: <see href="https://www.baeldung.com/cs/rgb-color-light-frequency#2-cie-color-matching"/> (skips gamma conversion)
        /// </para>
        /// </summary>
        /// <param name="nanometers">Visible wavelength in nanometers (visible wavelength ranges from 380nm to 700nm)</param>
        /// <returns>A linear color that this wavelength corresponds to which closely resembles how the human eye perceives this wavelength</returns>
        public static color WavelengthColor(float nanometers)
        {
            // Calc X
            float xt1 = (nanometers - xt1sub) * (nanometers < xt1sub ? 0.0624f : 0.0374f);
            float xt2 = (nanometers - xt2sub) * (nanometers < xt2sub ? 0.0264f : 0.0323f);
            float xt3 = (nanometers - xt3sub) * (nanometers < xt3sub ? 0.0490f : 0.0382f);

            float x =
                0.362f * MathX.Pow(MathX.E, -0.5f * MathX.Pow(xt1, 2f)) +
                1.056f * MathX.Pow(MathX.E, -0.5f * MathX.Pow(xt2, 2f)) -
                0.065f * MathX.Pow(MathX.E, -0.5f * MathX.Pow(xt3, 2f));


            // Calc Y
            float yt1 = (nanometers - yt1sub) * (nanometers < yt1sub ? 0.0213f : 0.0247f);
            float yt2 = (nanometers - yt2sub) * (nanometers < yt2sub ? 0.0613f : 0.0322f);

            float y =
                0.821f * MathX.Pow(MathX.E, -0.5f * MathX.Pow(yt1, 2f)) +
                0.286f * MathX.Pow(MathX.E, -0.5f * MathX.Pow(yt2, 2f));


            // Calc Z
            float zt1 = (nanometers - zt1sub) * (nanometers < zt1sub ? 0.0845f : 0.0278f);
            float zt2 = (nanometers - zt2sub) * (nanometers < zt2sub ? 0.0385f : 0.0725f);

            float z =
                1.217f * MathX.Pow(MathX.E, -0.5f * MathX.Pow(zt1, 2f)) +
                0.681f * MathX.Pow(MathX.E, -0.5f * MathX.Pow(zt2, 2f));


            // Construct color using RGB coefficients
            var result = new color(
                 3.2406255f * x + -1.537208f * y + -0.4986286f * z,
                -0.9689307f * x + 1.8757561f * y + 0.0415175f * z,
                 0.0557101f * x + -0.2040211f * y + 1.0569959f * z,
                 1f
            );

            return MathX.Clamp(result, 0f, 1f);
        }



        /// <inheritdoc cref="WavelengthColor(float)"/>
        public static colorX WavelengthColorX(float nanometers)
        {
            return new colorX(WavelengthColor(nanometers), ColorProfile.Linear);
        }



        /// <inheritdoc cref="BlackBodyColor(float)"/>
        public static colorX BlackBodyColorX(float temperature)
        {
            return new colorX(BlackBodyColor(temperature), ColorProfile.Linear);
        }



        /// <summary>
        /// <para>
        /// Gets the linear RGB color of black-body radiation at a specified kelvin temperature
        /// </para>
        ///
        /// <para>
        /// Corresponds to CIE 1964 10 degree color-matching functions (CMFs)<b/>
        /// Values correspond to the '10deg' rows here: http://www.vendian.org/mncharity/dir3/blackbody/UnstableURLs/bbr_color.html
        /// </para>
        /// </summary>
        /// <param name="temperature">Temperature, in degrees kelvin</param>
        /// <returns>Color of black-body radiation at the specified temperature</returns>
        public static color BlackBodyColor(float temperature)
        {
            //Get the temperature value
            var index = temperature;

            //Calculate the InverseLerp of Temperature
            index = MathX.InverseLerp(1000f, 40000f, index);
            index *= BlackBodyRadiation.Length;

            //Clamp the value
            var i0 = (int)index;
            i0 = MathX.Clamp(i0, 0, BlackBodyRadiation.Length);

            var i1 = MathX.Min(i0 + 1, BlackBodyRadiation.Length - 1);

            var lerp = index - i0;

            //Lerp the two colors
            return MathX.Lerp(BlackBodyRadiation[i0], BlackBodyRadiation[i1], lerp);
        }


        /// <summary>
        /// <para>
        /// Lookup table for black-body radiation values.
        /// </para>
        ///
        /// <para>
        /// Corresponds to CIE 1964 10 degree color-matching functions (CMFs)<b/>
        /// Values correspond to the '10deg' rows here: http://www.vendian.org/mncharity/dir3/blackbody/UnstableURLs/bbr_color.html
        /// </para>
        /// </summary>
        static color[] BlackBodyRadiation =
        {
            new color(1.0000f, 0.0401f, 0.0000f, 1.0f), //1000 K
            new color(1.0000f, 0.0631f, 0.0000f, 1.0f), //1100 K
            new color(1.0000f, 0.0860f, 0.0000f, 1.0f), //1200 K
            new color(1.0000f, 0.1085f, 0.0000f, 1.0f), //1300 K
            new color(1.0000f, 0.1303f, 0.0000f, 1.0f), //1400 K
            new color(1.0000f, 0.1515f, 0.0000f, 1.0f), //1500 K
            new color(1.0000f, 0.1718f, 0.0000f, 1.0f), //1600 K
            new color(1.0000f, 0.1912f, 0.0000f, 1.0f), //1700 K
            new color(1.0000f, 0.2097f, 0.0000f, 1.0f), //1800 K
            new color(1.0000f, 0.2272f, 0.0000f, 1.0f), //1900 K
            new color(1.0000f, 0.2484f, 0.0061f, 1.0f), //2000 K
            new color(1.0000f, 0.2709f, 0.0153f, 1.0f), //2100 K
            new color(1.0000f, 0.2930f, 0.0257f, 1.0f), //2200 K
            new color(1.0000f, 0.3149f, 0.0373f, 1.0f), //2300 K
            new color(1.0000f, 0.3364f, 0.0501f, 1.0f), //2400 K
            new color(1.0000f, 0.3577f, 0.0640f, 1.0f), //2500 K
            new color(1.0000f, 0.3786f, 0.0790f, 1.0f), //2600 K
            new color(1.0000f, 0.3992f, 0.0950f, 1.0f), //2700 K
            new color(1.0000f, 0.4195f, 0.1119f, 1.0f), //2800 K
            new color(1.0000f, 0.4394f, 0.1297f, 1.0f), //2900 K
            new color(1.0000f, 0.4589f, 0.1483f, 1.0f), //3000 K
            new color(1.0000f, 0.4781f, 0.1677f, 1.0f), //3100 K
            new color(1.0000f, 0.4970f, 0.1879f, 1.0f), //3200 K
            new color(1.0000f, 0.5155f, 0.2087f, 1.0f), //3300 K
            new color(1.0000f, 0.5336f, 0.2301f, 1.0f), //3400 K
            new color(1.0000f, 0.5515f, 0.2520f, 1.0f), //3500 K
            new color(1.0000f, 0.5689f, 0.2745f, 1.0f), //3600 K
            new color(1.0000f, 0.5860f, 0.2974f, 1.0f), //3700 K
            new color(1.0000f, 0.6028f, 0.3207f, 1.0f), //3800 K
            new color(1.0000f, 0.6193f, 0.3444f, 1.0f), //3900 K
            new color(1.0000f, 0.6354f, 0.3684f, 1.0f), //4000 K
            new color(1.0000f, 0.6511f, 0.3927f, 1.0f), //4100 K
            new color(1.0000f, 0.6666f, 0.4172f, 1.0f), //4200 K
            new color(1.0000f, 0.6817f, 0.4419f, 1.0f), //4300 K
            new color(1.0000f, 0.6966f, 0.4668f, 1.0f), //4400 K
            new color(1.0000f, 0.7111f, 0.4919f, 1.0f), //4500 K
            new color(1.0000f, 0.7253f, 0.5170f, 1.0f), //4600 K
            new color(1.0000f, 0.7392f, 0.5422f, 1.0f), //4700 K
            new color(1.0000f, 0.7528f, 0.5675f, 1.0f), //4800 K
            new color(1.0000f, 0.7661f, 0.5928f, 1.0f), //4900 K
            new color(1.0000f, 0.7792f, 0.6180f, 1.0f), //5000 K
            new color(1.0000f, 0.7919f, 0.6433f, 1.0f), //5100 K
            new color(1.0000f, 0.8044f, 0.6685f, 1.0f), //5200 K
            new color(1.0000f, 0.8167f, 0.6937f, 1.0f), //5300 K
            new color(1.0000f, 0.8286f, 0.7187f, 1.0f), //5400 K
            new color(1.0000f, 0.8403f, 0.7437f, 1.0f), //5500 K
            new color(1.0000f, 0.8518f, 0.7686f, 1.0f), //5600 K
            new color(1.0000f, 0.8630f, 0.7933f, 1.0f), //5700 K
            new color(1.0000f, 0.8740f, 0.8179f, 1.0f), //5800 K
            new color(1.0000f, 0.8847f, 0.8424f, 1.0f), //5900 K
            new color(1.0000f, 0.8952f, 0.8666f, 1.0f), //6000 K
            new color(1.0000f, 0.9055f, 0.8907f, 1.0f), //6100 K
            new color(1.0000f, 0.9156f, 0.9147f, 1.0f), //6200 K
            new color(1.0000f, 0.9254f, 0.9384f, 1.0f), //6300 K
            new color(1.0000f, 0.9351f, 0.9619f, 1.0f), //6400 K
            new color(1.0000f, 0.9445f, 0.9853f, 1.0f), //6500 K
            new color(0.9917f, 0.9458f, 1.0000f, 1.0f), //6600 K
            new color(0.9696f, 0.9336f, 1.0000f, 1.0f), //6700 K
            new color(0.9488f, 0.9219f, 1.0000f, 1.0f), //6800 K
            new color(0.9290f, 0.9107f, 1.0000f, 1.0f), //6900 K
            new color(0.9102f, 0.9000f, 1.0000f, 1.0f), //7000 K
            new color(0.8923f, 0.8897f, 1.0000f, 1.0f), //7100 K
            new color(0.8753f, 0.8799f, 1.0000f, 1.0f), //7200 K
            new color(0.8591f, 0.8704f, 1.0000f, 1.0f), //7300 K
            new color(0.8437f, 0.8614f, 1.0000f, 1.0f), //7400 K
            new color(0.8289f, 0.8527f, 1.0000f, 1.0f), //7500 K
            new color(0.8149f, 0.8443f, 1.0000f, 1.0f), //7600 K
            new color(0.8014f, 0.8363f, 1.0000f, 1.0f), //7700 K
            new color(0.7885f, 0.8285f, 1.0000f, 1.0f), //7800 K
            new color(0.7762f, 0.8211f, 1.0000f, 1.0f), //7900 K
            new color(0.7644f, 0.8139f, 1.0000f, 1.0f), //8000 K
            new color(0.7531f, 0.8069f, 1.0000f, 1.0f), //8100 K
            new color(0.7423f, 0.8002f, 1.0000f, 1.0f), //8200 K
            new color(0.7319f, 0.7938f, 1.0000f, 1.0f), //8300 K
            new color(0.7219f, 0.7875f, 1.0000f, 1.0f), //8400 K
            new color(0.7123f, 0.7815f, 1.0000f, 1.0f), //8500 K
            new color(0.7030f, 0.7757f, 1.0000f, 1.0f), //8600 K
            new color(0.6941f, 0.7700f, 1.0000f, 1.0f), //8700 K
            new color(0.6856f, 0.7645f, 1.0000f, 1.0f), //8800 K
            new color(0.6773f, 0.7593f, 1.0000f, 1.0f), //8900 K
            new color(0.6693f, 0.7541f, 1.0000f, 1.0f), //9000 K
            new color(0.6617f, 0.7492f, 1.0000f, 1.0f), //9100 K
            new color(0.6543f, 0.7444f, 1.0000f, 1.0f), //9200 K
            new color(0.6471f, 0.7397f, 1.0000f, 1.0f), //9300 K
            new color(0.6402f, 0.7352f, 1.0000f, 1.0f), //9400 K
            new color(0.6335f, 0.7308f, 1.0000f, 1.0f), //9500 K
            new color(0.6271f, 0.7265f, 1.0000f, 1.0f), //9600 K
            new color(0.6208f, 0.7224f, 1.0000f, 1.0f), //9700 K
            new color(0.6148f, 0.7183f, 1.0000f, 1.0f), //9800 K
            new color(0.6089f, 0.7144f, 1.0000f, 1.0f), //9900 K
            new color(0.6033f, 0.7106f, 1.0000f, 1.0f), //10000 K
            new color(0.5978f, 0.7069f, 1.0000f, 1.0f), //10100 K
            new color(0.5925f, 0.7033f, 1.0000f, 1.0f), //10200 K
            new color(0.5873f, 0.6998f, 1.0000f, 1.0f), //10300 K
            new color(0.5823f, 0.6964f, 1.0000f, 1.0f), //10400 K
            new color(0.5774f, 0.6930f, 1.0000f, 1.0f), //10500 K
            new color(0.5727f, 0.6898f, 1.0000f, 1.0f), //10600 K
            new color(0.5681f, 0.6866f, 1.0000f, 1.0f), //10700 K
            new color(0.5637f, 0.6836f, 1.0000f, 1.0f), //10800 K
            new color(0.5593f, 0.6806f, 1.0000f, 1.0f), //10900 K
            new color(0.5551f, 0.6776f, 1.0000f, 1.0f), //11000 K
            new color(0.5510f, 0.6748f, 1.0000f, 1.0f), //11100 K
            new color(0.5470f, 0.6720f, 1.0000f, 1.0f), //11200 K
            new color(0.5432f, 0.6693f, 1.0000f, 1.0f), //11300 K
            new color(0.5394f, 0.6666f, 1.0000f, 1.0f), //11400 K
            new color(0.5357f, 0.6640f, 1.0000f, 1.0f), //11500 K
            new color(0.5322f, 0.6615f, 1.0000f, 1.0f), //11600 K
            new color(0.5287f, 0.6590f, 1.0000f, 1.0f), //11700 K
            new color(0.5253f, 0.6566f, 1.0000f, 1.0f), //11800 K
            new color(0.5220f, 0.6542f, 1.0000f, 1.0f), //11900 K
            new color(0.5187f, 0.6519f, 1.0000f, 1.0f), //12000 K
            new color(0.5156f, 0.6497f, 1.0000f, 1.0f), //12100 K
            new color(0.5125f, 0.6474f, 1.0000f, 1.0f), //12200 K
            new color(0.5095f, 0.6453f, 1.0000f, 1.0f), //12300 K
            new color(0.5066f, 0.6432f, 1.0000f, 1.0f), //12400 K
            new color(0.5037f, 0.6411f, 1.0000f, 1.0f), //12500 K
            new color(0.5009f, 0.6391f, 1.0000f, 1.0f), //12600 K
            new color(0.4982f, 0.6371f, 1.0000f, 1.0f), //12700 K
            new color(0.4955f, 0.6351f, 1.0000f, 1.0f), //12800 K
            new color(0.4929f, 0.6332f, 1.0000f, 1.0f), //12900 K
            new color(0.4904f, 0.6314f, 1.0000f, 1.0f), //13000 K
            new color(0.4879f, 0.6295f, 1.0000f, 1.0f), //13100 K
            new color(0.4854f, 0.6277f, 1.0000f, 1.0f), //13200 K
            new color(0.4831f, 0.6260f, 1.0000f, 1.0f), //13300 K
            new color(0.4807f, 0.6243f, 1.0000f, 1.0f), //13400 K
            new color(0.4785f, 0.6226f, 1.0000f, 1.0f), //13500 K
            new color(0.4762f, 0.6209f, 1.0000f, 1.0f), //13600 K
            new color(0.4740f, 0.6193f, 1.0000f, 1.0f), //13700 K
            new color(0.4719f, 0.6177f, 1.0000f, 1.0f), //13800 K
            new color(0.4698f, 0.6161f, 1.0000f, 1.0f), //13900 K
            new color(0.4677f, 0.6146f, 1.0000f, 1.0f), //14000 K
            new color(0.4657f, 0.6131f, 1.0000f, 1.0f), //14100 K
            new color(0.4638f, 0.6116f, 1.0000f, 1.0f), //14200 K
            new color(0.4618f, 0.6102f, 1.0000f, 1.0f), //14300 K
            new color(0.4599f, 0.6087f, 1.0000f, 1.0f), //14400 K
            new color(0.4581f, 0.6073f, 1.0000f, 1.0f), //14500 K
            new color(0.4563f, 0.6060f, 1.0000f, 1.0f), //14600 K
            new color(0.4545f, 0.6046f, 1.0000f, 1.0f), //14700 K
            new color(0.4527f, 0.6033f, 1.0000f, 1.0f), //14800 K
            new color(0.4510f, 0.6020f, 1.0000f, 1.0f), //14900 K
            new color(0.4493f, 0.6007f, 1.0000f, 1.0f), //15000 K
            new color(0.4477f, 0.5994f, 1.0000f, 1.0f), //15100 K
            new color(0.4460f, 0.5982f, 1.0000f, 1.0f), //15200 K
            new color(0.4445f, 0.5970f, 1.0000f, 1.0f), //15300 K
            new color(0.4429f, 0.5958f, 1.0000f, 1.0f), //15400 K
            new color(0.4413f, 0.5946f, 1.0000f, 1.0f), //15500 K
            new color(0.4398f, 0.5935f, 1.0000f, 1.0f), //15600 K
            new color(0.4384f, 0.5923f, 1.0000f, 1.0f), //15700 K
            new color(0.4369f, 0.5912f, 1.0000f, 1.0f), //15800 K
            new color(0.4355f, 0.5901f, 1.0000f, 1.0f), //15900 K
            new color(0.4341f, 0.5890f, 1.0000f, 1.0f), //16000 K
            new color(0.4327f, 0.5879f, 1.0000f, 1.0f), //16100 K
            new color(0.4313f, 0.5869f, 1.0000f, 1.0f), //16200 K
            new color(0.4300f, 0.5859f, 1.0000f, 1.0f), //16300 K
            new color(0.4287f, 0.5848f, 1.0000f, 1.0f), //16400 K
            new color(0.4274f, 0.5838f, 1.0000f, 1.0f), //16500 K
            new color(0.4261f, 0.5829f, 1.0000f, 1.0f), //16600 K
            new color(0.4249f, 0.5819f, 1.0000f, 1.0f), //16700 K
            new color(0.4236f, 0.5809f, 1.0000f, 1.0f), //16800 K
            new color(0.4224f, 0.5800f, 1.0000f, 1.0f), //16900 K
            new color(0.4212f, 0.5791f, 1.0000f, 1.0f), //17000 K
            new color(0.4201f, 0.5781f, 1.0000f, 1.0f), //17100 K
            new color(0.4189f, 0.5772f, 1.0000f, 1.0f), //17200 K
            new color(0.4178f, 0.5763f, 1.0000f, 1.0f), //17300 K
            new color(0.4167f, 0.5755f, 1.0000f, 1.0f), //17400 K
            new color(0.4156f, 0.5746f, 1.0000f, 1.0f), //17500 K
            new color(0.4145f, 0.5738f, 1.0000f, 1.0f), //17600 K
            new color(0.4134f, 0.5729f, 1.0000f, 1.0f), //17700 K
            new color(0.4124f, 0.5721f, 1.0000f, 1.0f), //17800 K
            new color(0.4113f, 0.5713f, 1.0000f, 1.0f), //17900 K
            new color(0.4103f, 0.5705f, 1.0000f, 1.0f), //18000 K
            new color(0.4093f, 0.5697f, 1.0000f, 1.0f), //18100 K
            new color(0.4083f, 0.5689f, 1.0000f, 1.0f), //18200 K
            new color(0.4074f, 0.5681f, 1.0000f, 1.0f), //18300 K
            new color(0.4064f, 0.5674f, 1.0000f, 1.0f), //18400 K
            new color(0.4055f, 0.5666f, 1.0000f, 1.0f), //18500 K
            new color(0.4045f, 0.5659f, 1.0000f, 1.0f), //18600 K
            new color(0.4036f, 0.5652f, 1.0000f, 1.0f), //18700 K
            new color(0.4027f, 0.5644f, 1.0000f, 1.0f), //18800 K
            new color(0.4018f, 0.5637f, 1.0000f, 1.0f), //18900 K
            new color(0.4009f, 0.5630f, 1.0000f, 1.0f), //19000 K
            new color(0.4001f, 0.5623f, 1.0000f, 1.0f), //19100 K
            new color(0.3992f, 0.5616f, 1.0000f, 1.0f), //19200 K
            new color(0.3984f, 0.5610f, 1.0000f, 1.0f), //19300 K
            new color(0.3975f, 0.5603f, 1.0000f, 1.0f), //19400 K
            new color(0.3967f, 0.5596f, 1.0000f, 1.0f), //19500 K
            new color(0.3959f, 0.5590f, 1.0000f, 1.0f), //19600 K
            new color(0.3951f, 0.5584f, 1.0000f, 1.0f), //19700 K
            new color(0.3943f, 0.5577f, 1.0000f, 1.0f), //19800 K
            new color(0.3935f, 0.5571f, 1.0000f, 1.0f), //19900 K
            new color(0.3928f, 0.5565f, 1.0000f, 1.0f), //20000 K
            new color(0.3920f, 0.5559f, 1.0000f, 1.0f), //20100 K
            new color(0.3913f, 0.5553f, 1.0000f, 1.0f), //20200 K
            new color(0.3905f, 0.5547f, 1.0000f, 1.0f), //20300 K
            new color(0.3898f, 0.5541f, 1.0000f, 1.0f), //20400 K
            new color(0.3891f, 0.5535f, 1.0000f, 1.0f), //20500 K
            new color(0.3884f, 0.5529f, 1.0000f, 1.0f), //20600 K
            new color(0.3877f, 0.5524f, 1.0000f, 1.0f), //20700 K
            new color(0.3870f, 0.5518f, 1.0000f, 1.0f), //20800 K
            new color(0.3863f, 0.5513f, 1.0000f, 1.0f), //20900 K
            new color(0.3856f, 0.5507f, 1.0000f, 1.0f), //21000 K
            new color(0.3850f, 0.5502f, 1.0000f, 1.0f), //21100 K
            new color(0.3843f, 0.5496f, 1.0000f, 1.0f), //21200 K
            new color(0.3836f, 0.5491f, 1.0000f, 1.0f), //21300 K
            new color(0.3830f, 0.5486f, 1.0000f, 1.0f), //21400 K
            new color(0.3824f, 0.5481f, 1.0000f, 1.0f), //21500 K
            new color(0.3817f, 0.5476f, 1.0000f, 1.0f), //21600 K
            new color(0.3811f, 0.5471f, 1.0000f, 1.0f), //21700 K
            new color(0.3805f, 0.5466f, 1.0000f, 1.0f), //21800 K
            new color(0.3799f, 0.5461f, 1.0000f, 1.0f), //21900 K
            new color(0.3793f, 0.5456f, 1.0000f, 1.0f), //22000 K
            new color(0.3787f, 0.5451f, 1.0000f, 1.0f), //22100 K
            new color(0.3781f, 0.5446f, 1.0000f, 1.0f), //22200 K
            new color(0.3776f, 0.5441f, 1.0000f, 1.0f), //22300 K
            new color(0.3770f, 0.5437f, 1.0000f, 1.0f), //22400 K
            new color(0.3764f, 0.5432f, 1.0000f, 1.0f), //22500 K
            new color(0.3759f, 0.5428f, 1.0000f, 1.0f), //22600 K
            new color(0.3753f, 0.5423f, 1.0000f, 1.0f), //22700 K
            new color(0.3748f, 0.5419f, 1.0000f, 1.0f), //22800 K
            new color(0.3742f, 0.5414f, 1.0000f, 1.0f), //22900 K
            new color(0.3737f, 0.5410f, 1.0000f, 1.0f), //23000 K
            new color(0.3732f, 0.5405f, 1.0000f, 1.0f), //23100 K
            new color(0.3726f, 0.5401f, 1.0000f, 1.0f), //23200 K
            new color(0.3721f, 0.5397f, 1.0000f, 1.0f), //23300 K
            new color(0.3716f, 0.5393f, 1.0000f, 1.0f), //23400 K
            new color(0.3711f, 0.5389f, 1.0000f, 1.0f), //23500 K
            new color(0.3706f, 0.5384f, 1.0000f, 1.0f), //23600 K
            new color(0.3701f, 0.5380f, 1.0000f, 1.0f), //23700 K
            new color(0.3696f, 0.5376f, 1.0000f, 1.0f), //23800 K
            new color(0.3692f, 0.5372f, 1.0000f, 1.0f), //23900 K
            new color(0.3687f, 0.5368f, 1.0000f, 1.0f), //24000 K
            new color(0.3682f, 0.5365f, 1.0000f, 1.0f), //24100 K
            new color(0.3677f, 0.5361f, 1.0000f, 1.0f), //24200 K
            new color(0.3673f, 0.5357f, 1.0000f, 1.0f), //24300 K
            new color(0.3668f, 0.5353f, 1.0000f, 1.0f), //24400 K
            new color(0.3664f, 0.5349f, 1.0000f, 1.0f), //24500 K
            new color(0.3659f, 0.5346f, 1.0000f, 1.0f), //24600 K
            new color(0.3655f, 0.5342f, 1.0000f, 1.0f), //24700 K
            new color(0.3650f, 0.5338f, 1.0000f, 1.0f), //24800 K
            new color(0.3646f, 0.5335f, 1.0000f, 1.0f), //24900 K
            new color(0.3642f, 0.5331f, 1.0000f, 1.0f), //25000 K
            new color(0.3637f, 0.5328f, 1.0000f, 1.0f), //25100 K
            new color(0.3633f, 0.5324f, 1.0000f, 1.0f), //25200 K
            new color(0.3629f, 0.5321f, 1.0000f, 1.0f), //25300 K
            new color(0.3625f, 0.5317f, 1.0000f, 1.0f), //25400 K
            new color(0.3621f, 0.5314f, 1.0000f, 1.0f), //25500 K
            new color(0.3617f, 0.5310f, 1.0000f, 1.0f), //25600 K
            new color(0.3613f, 0.5307f, 1.0000f, 1.0f), //25700 K
            new color(0.3609f, 0.5304f, 1.0000f, 1.0f), //25800 K
            new color(0.3605f, 0.5300f, 1.0000f, 1.0f), //25900 K
            new color(0.3601f, 0.5297f, 1.0000f, 1.0f), //26000 K
            new color(0.3597f, 0.5294f, 1.0000f, 1.0f), //26100 K
            new color(0.3593f, 0.5291f, 1.0000f, 1.0f), //26200 K
            new color(0.3589f, 0.5288f, 1.0000f, 1.0f), //26300 K
            new color(0.3586f, 0.5284f, 1.0000f, 1.0f), //26400 K
            new color(0.3582f, 0.5281f, 1.0000f, 1.0f), //26500 K
            new color(0.3578f, 0.5278f, 1.0000f, 1.0f), //26600 K
            new color(0.3575f, 0.5275f, 1.0000f, 1.0f), //26700 K
            new color(0.3571f, 0.5272f, 1.0000f, 1.0f), //26800 K
            new color(0.3567f, 0.5269f, 1.0000f, 1.0f), //26900 K
            new color(0.3564f, 0.5266f, 1.0000f, 1.0f), //27000 K
            new color(0.3560f, 0.5263f, 1.0000f, 1.0f), //27100 K
            new color(0.3557f, 0.5260f, 1.0000f, 1.0f), //27200 K
            new color(0.3553f, 0.5257f, 1.0000f, 1.0f), //27300 K
            new color(0.3550f, 0.5255f, 1.0000f, 1.0f), //27400 K
            new color(0.3546f, 0.5252f, 1.0000f, 1.0f), //27500 K
            new color(0.3543f, 0.5249f, 1.0000f, 1.0f), //27600 K
            new color(0.3540f, 0.5246f, 1.0000f, 1.0f), //27700 K
            new color(0.3536f, 0.5243f, 1.0000f, 1.0f), //27800 K
            new color(0.3533f, 0.5241f, 1.0000f, 1.0f), //27900 K
            new color(0.3530f, 0.5238f, 1.0000f, 1.0f), //28000 K
            new color(0.3527f, 0.5235f, 1.0000f, 1.0f), //28100 K
            new color(0.3524f, 0.5232f, 1.0000f, 1.0f), //28200 K
            new color(0.3520f, 0.5230f, 1.0000f, 1.0f), //28300 K
            new color(0.3517f, 0.5227f, 1.0000f, 1.0f), //28400 K
            new color(0.3514f, 0.5225f, 1.0000f, 1.0f), //28500 K
            new color(0.3511f, 0.5222f, 1.0000f, 1.0f), //28600 K
            new color(0.3508f, 0.5219f, 1.0000f, 1.0f), //28700 K
            new color(0.3505f, 0.5217f, 1.0000f, 1.0f), //28800 K
            new color(0.3502f, 0.5214f, 1.0000f, 1.0f), //28900 K
            new color(0.3499f, 0.5212f, 1.0000f, 1.0f), //29000 K
            new color(0.3496f, 0.5209f, 1.0000f, 1.0f), //29100 K
            new color(0.3493f, 0.5207f, 1.0000f, 1.0f), //29200 K
            new color(0.3490f, 0.5204f, 1.0000f, 1.0f), //29300 K
            new color(0.3487f, 0.5202f, 1.0000f, 1.0f), //29400 K
            new color(0.3485f, 0.5200f, 1.0000f, 1.0f), //29500 K
            new color(0.3482f, 0.5197f, 1.0000f, 1.0f), //29600 K
            new color(0.3479f, 0.5195f, 1.0000f, 1.0f), //29700 K
            new color(0.3476f, 0.5192f, 1.0000f, 1.0f), //29800 K
            new color(0.3473f, 0.5190f, 1.0000f, 1.0f), //29900 K
            new color(0.3471f, 0.5188f, 1.0000f, 1.0f), //30000 K
            new color(0.3468f, 0.5186f, 1.0000f, 1.0f), //30100 K
            new color(0.3465f, 0.5183f, 1.0000f, 1.0f), //30200 K
            new color(0.3463f, 0.5181f, 1.0000f, 1.0f), //30300 K
            new color(0.3460f, 0.5179f, 1.0000f, 1.0f), //30400 K
            new color(0.3457f, 0.5177f, 1.0000f, 1.0f), //30500 K
            new color(0.3455f, 0.5174f, 1.0000f, 1.0f), //30600 K
            new color(0.3452f, 0.5172f, 1.0000f, 1.0f), //30700 K
            new color(0.3450f, 0.5170f, 1.0000f, 1.0f), //30800 K
            new color(0.3447f, 0.5168f, 1.0000f, 1.0f), //30900 K
            new color(0.3444f, 0.5166f, 1.0000f, 1.0f), //31000 K
            new color(0.3442f, 0.5164f, 1.0000f, 1.0f), //31100 K
            new color(0.3439f, 0.5161f, 1.0000f, 1.0f), //31200 K
            new color(0.3437f, 0.5159f, 1.0000f, 1.0f), //31300 K
            new color(0.3435f, 0.5157f, 1.0000f, 1.0f), //31400 K
            new color(0.3432f, 0.5155f, 1.0000f, 1.0f), //31500 K
            new color(0.3430f, 0.5153f, 1.0000f, 1.0f), //31600 K
            new color(0.3427f, 0.5151f, 1.0000f, 1.0f), //31700 K
            new color(0.3425f, 0.5149f, 1.0000f, 1.0f), //31800 K
            new color(0.3423f, 0.5147f, 1.0000f, 1.0f), //31900 K
            new color(0.3420f, 0.5145f, 1.0000f, 1.0f), //32000 K
            new color(0.3418f, 0.5143f, 1.0000f, 1.0f), //32100 K
            new color(0.3416f, 0.5141f, 1.0000f, 1.0f), //32200 K
            new color(0.3413f, 0.5139f, 1.0000f, 1.0f), //32300 K
            new color(0.3411f, 0.5137f, 1.0000f, 1.0f), //32400 K
            new color(0.3409f, 0.5135f, 1.0000f, 1.0f), //32500 K
            new color(0.3407f, 0.5133f, 1.0000f, 1.0f), //32600 K
            new color(0.3404f, 0.5132f, 1.0000f, 1.0f), //32700 K
            new color(0.3402f, 0.5130f, 1.0000f, 1.0f), //32800 K
            new color(0.3400f, 0.5128f, 1.0000f, 1.0f), //32900 K
            new color(0.3398f, 0.5126f, 1.0000f, 1.0f), //33000 K
            new color(0.3396f, 0.5124f, 1.0000f, 1.0f), //33100 K
            new color(0.3393f, 0.5122f, 1.0000f, 1.0f), //33200 K
            new color(0.3391f, 0.5120f, 1.0000f, 1.0f), //33300 K
            new color(0.3389f, 0.5119f, 1.0000f, 1.0f), //33400 K
            new color(0.3387f, 0.5117f, 1.0000f, 1.0f), //33500 K
            new color(0.3385f, 0.5115f, 1.0000f, 1.0f), //33600 K
            new color(0.3383f, 0.5113f, 1.0000f, 1.0f), //33700 K
            new color(0.3381f, 0.5112f, 1.0000f, 1.0f), //33800 K
            new color(0.3379f, 0.5110f, 1.0000f, 1.0f), //33900 K
            new color(0.3377f, 0.5108f, 1.0000f, 1.0f), //34000 K
            new color(0.3375f, 0.5106f, 1.0000f, 1.0f), //34100 K
            new color(0.3373f, 0.5105f, 1.0000f, 1.0f), //34200 K
            new color(0.3371f, 0.5103f, 1.0000f, 1.0f), //34300 K
            new color(0.3369f, 0.5101f, 1.0000f, 1.0f), //34400 K
            new color(0.3367f, 0.5100f, 1.0000f, 1.0f), //34500 K
            new color(0.3365f, 0.5098f, 1.0000f, 1.0f), //34600 K
            new color(0.3363f, 0.5096f, 1.0000f, 1.0f), //34700 K
            new color(0.3361f, 0.5095f, 1.0000f, 1.0f), //34800 K
            new color(0.3359f, 0.5093f, 1.0000f, 1.0f), //34900 K
            new color(0.3357f, 0.5091f, 1.0000f, 1.0f), //35000 K
            new color(0.3356f, 0.5090f, 1.0000f, 1.0f), //35100 K
            new color(0.3354f, 0.5088f, 1.0000f, 1.0f), //35200 K
            new color(0.3352f, 0.5087f, 1.0000f, 1.0f), //35300 K
            new color(0.3350f, 0.5085f, 1.0000f, 1.0f), //35400 K
            new color(0.3348f, 0.5084f, 1.0000f, 1.0f), //35500 K
            new color(0.3346f, 0.5082f, 1.0000f, 1.0f), //35600 K
            new color(0.3345f, 0.5080f, 1.0000f, 1.0f), //35700 K
            new color(0.3343f, 0.5079f, 1.0000f, 1.0f), //35800 K
            new color(0.3341f, 0.5077f, 1.0000f, 1.0f), //35900 K
            new color(0.3339f, 0.5076f, 1.0000f, 1.0f), //36000 K
            new color(0.3338f, 0.5074f, 1.0000f, 1.0f), //36100 K
            new color(0.3336f, 0.5073f, 1.0000f, 1.0f), //36200 K
            new color(0.3334f, 0.5071f, 1.0000f, 1.0f), //36300 K
            new color(0.3332f, 0.5070f, 1.0000f, 1.0f), //36400 K
            new color(0.3331f, 0.5068f, 1.0000f, 1.0f), //36500 K
            new color(0.3329f, 0.5067f, 1.0000f, 1.0f), //36600 K
            new color(0.3327f, 0.5066f, 1.0000f, 1.0f), //36700 K
            new color(0.3326f, 0.5064f, 1.0000f, 1.0f), //36800 K
            new color(0.3324f, 0.5063f, 1.0000f, 1.0f), //36900 K
            new color(0.3322f, 0.5061f, 1.0000f, 1.0f), //37000 K
            new color(0.3321f, 0.5060f, 1.0000f, 1.0f), //37100 K
            new color(0.3319f, 0.5058f, 1.0000f, 1.0f), //37200 K
            new color(0.3317f, 0.5057f, 1.0000f, 1.0f), //37300 K
            new color(0.3316f, 0.5056f, 1.0000f, 1.0f), //37400 K
            new color(0.3314f, 0.5054f, 1.0000f, 1.0f), //37500 K
            new color(0.3313f, 0.5053f, 1.0000f, 1.0f), //37600 K
            new color(0.3311f, 0.5052f, 1.0000f, 1.0f), //37700 K
            new color(0.3309f, 0.5050f, 1.0000f, 1.0f), //37800 K
            new color(0.3308f, 0.5049f, 1.0000f, 1.0f), //37900 K
            new color(0.3306f, 0.5048f, 1.0000f, 1.0f), //38000 K
            new color(0.3305f, 0.5046f, 1.0000f, 1.0f), //38100 K
            new color(0.3303f, 0.5045f, 1.0000f, 1.0f), //38200 K
            new color(0.3302f, 0.5044f, 1.0000f, 1.0f), //38300 K
            new color(0.3300f, 0.5042f, 1.0000f, 1.0f), //38400 K
            new color(0.3299f, 0.5041f, 1.0000f, 1.0f), //38500 K
            new color(0.3297f, 0.5040f, 1.0000f, 1.0f), //38600 K
            new color(0.3296f, 0.5038f, 1.0000f, 1.0f), //38700 K
            new color(0.3294f, 0.5037f, 1.0000f, 1.0f), //38800 K
            new color(0.3293f, 0.5036f, 1.0000f, 1.0f), //38900 K
            new color(0.3291f, 0.5035f, 1.0000f, 1.0f), //39000 K
            new color(0.3290f, 0.5033f, 1.0000f, 1.0f), //39100 K
            new color(0.3288f, 0.5032f, 1.0000f, 1.0f), //39200 K
            new color(0.3287f, 0.5031f, 1.0000f, 1.0f), //39300 K
            new color(0.3286f, 0.5030f, 1.0000f, 1.0f), //39400 K
            new color(0.3284f, 0.5028f, 1.0000f, 1.0f), //39500 K
            new color(0.3283f, 0.5027f, 1.0000f, 1.0f), //39600 K
            new color(0.3281f, 0.5026f, 1.0000f, 1.0f), //39700 K
            new color(0.3280f, 0.5025f, 1.0000f, 1.0f), //39800 K
            new color(0.3279f, 0.5024f, 1.0000f, 1.0f), //39900 K
            new color(0.3277f, 0.5022f, 1.0000f, 1.0f), //40000 K
        };



        const float xt1sub = 442f;
        const float xt2sub = 599.8f;
        const float xt3sub = 501.1f;

        const float yt1sub = 568.8f;
        const float yt2sub = 530.9f;

        const float zt1sub = 437f;
        const float zt2sub = 459f;
    }
}
