using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Elements.Core
{
    public static partial class SignalX
    {
        // Amplitude-processing
        public static void Normalize(float[] input, float[] output,
            int? count = null, int inputOffset = 0, int outputOffset = 0)
        {
            // find the minimum and maximum first
            float min = input.Min();
            float max = input.Max();

            Process(input, output, i => MathX.InverseLerp(min, max, i)*2 - 1,
                count, inputOffset, outputOffset);
        }

        public static void Gain(float[] input, float[] output, float gain,
            int? count = null, int inputOffset = 0, int outputOffset = 0)
        {
            Process(input, output, i => i * gain, count, inputOffset, outputOffset);
        }

        // resampling
        public enum Interpolation
        {
            NearestNeighbor,
            Linear,
            Floor,
            Ceil,
        }

        public static void Resample(float[] input, float[] output)
        {
            double rate = output.Length / (double)input.Length;

            for(int i = 0; i < output.Length; i++)
                output[i] = input.Sample(i * rate);
        }

        public static void Resample(float[] input, float[] output, float inRate, float outRate, 
            Interpolation interpolation = Interpolation.NearestNeighbor,
            int? count = null, int inputOffset = 0, int outputOffset = 0)
        {
            int _count = count ?? (input.Length - inputOffset);

            // compute the read rate of the input to correspond to the output
            float readRate = inRate / outRate;

            int outLength = ResampledLength(_count, inRate, outRate);

            if ((output.Length - outputOffset) < outLength)
                throw new Exception("Output doesn't have enough space!");

            float pos = inputOffset;

            // resample
            for (int i = 0; i < outLength; i++)
            {
                int _i = i + outputOffset;

                switch (interpolation)
                {
                    case Interpolation.NearestNeighbor:
                        output[_i] = input[MathX.RoundToInt(pos)];
                        break;

                    case Interpolation.Floor:
                        output[_i] = input[MathX.FloorToInt(pos)];
                        break;

                    case Interpolation.Ceil:
                        output[_i] = input[MathX.CeilToInt(pos)];
                        break;

                    case Interpolation.Linear:
                        int _pos = (int)pos;
                        float _frac = pos - _pos; // fractional part of the position
                        float i0 = input[_pos];
                        float i1 = input[Math.Min(_pos + 1, input.Length-1)];

                        output[_i] = MathX.Lerp(i0, i1, _frac);

                        break;
                }

                // advance the position
                pos += readRate;
                if (pos > input.Length-1)
                    pos = input.Length-1;
            }
        }

        public static int ResampledLength(float[] input, float inRate, float outRate)
        {
            return ResampledLength(input.Length, inRate, outRate);
        }

        public static int ResampledLength(int length, float inRate, float outRate)
        {
            float readRate = inRate / outRate;
            return MathX.RoundToInt(length / readRate);
        }

        // converting float to integer types
        //public static void Convert(float[] input, int[] output,
        //    int? count = null, int inputOffset = 0, int outputOffset = 0)
        //{
        //    Process(input, output, i => (int)MathX.Clamp(
        //        MathX.Lerp((float)int.MinValue, (float)int.MaxValue, (i+1f)*0.5f),
        //        int.MinValue, int.MaxValue),
        //        count, inputOffset, outputOffset);
        //}

        //public static void Convert(float[] input, short[] output,
        //    int? count = null, int inputOffset = 0, int outputOffset = 0)
        //{
        //    Process(input, output, i => (short)MathX.Clamp(
        //        MathX.Lerp((float)short.MinValue, (float)short.MaxValue, (i + 1f) * 0.5f),
        //        short.MinValue, short.MaxValue),
        //        count, inputOffset, outputOffset);
        //}
        
        //public static void Convert(float[] input, byte[] output,
        //    int? count = null, int inputOffset = 0, int outputOffset = 0)
        //{
        //    Process(input, output, i => (byte)MathX.Clamp(
        //        MathX.Lerp((float)byte.MinValue, (float)byte.MaxValue, (i + 1f) * 0.5f),
        //        byte.MinValue, byte.MaxValue),
        //        count, inputOffset, outputOffset);
        //}

        // converting integer types to float 0...1 range

        //public static void Convert(int[] input, float[] output,
        //    int? count = null, int inputOffset = 0, int outputOffset = 0)
        //{
        //    Process(input, output, i => MathX.InverseLerp(int.MinValue, int.MaxValue, i)*2 - 1,
        //        count, inputOffset, outputOffset);
        //}

        //public static void Convert(short[] input, float[] output,
        //    int? count = null, int inputOffset = 0, int outputOffset = 0)
        //{
        //    Process(input, output, i => MathX.InverseLerp(short.MinValue, short.MaxValue, i) * 2 - 1,
        //        count, inputOffset, outputOffset);
        //}

        //public static void Convert(byte[] input, float[] output,
        //    int? count = null, int inputOffset = 0, int outputOffset = 0)
        //{
        //    Process(input, output, i => MathX.InverseLerp(byte.MinValue, byte.MaxValue, i) * 2 - 1,
        //        count, inputOffset, outputOffset);
        //}

        // Generic signal processing function

        public static void Process<I,O>(I[] input, O[] output, Func<I,O> operation,
            int? count = null, int inputOffset = 0, int outputOffset = 0)
        {
            int _count = count ?? (input.Length - inputOffset);
            if (_count > (output.Length - outputOffset))
                throw new Exception("Output buffer doesn't have enough space");

            if (input.Equals(output) && inputOffset > outputOffset)
            {
                // run it backwards, to prevent from overwriting the output
                for (int i = _count-1; i >= 0; i++)
                    output[i + outputOffset] = operation(input[i + inputOffset]);
            }
            else
            {
                // run it forwards
                for (int i = 0; i < _count; i++)
                    output[i + outputOffset] = operation(input[i + inputOffset]);
            }
        }
    }
}
