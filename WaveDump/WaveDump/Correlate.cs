using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WaveDump
{
    public class Correlate
    {
        /// <summary>
        /// Compute the amount to shift array a to match array b
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="windowSize"></param>
        /// <returns></returns>
        public int Shift;
        public double Correlation;

        public Correlate(float[] a, float[] b, int windowSize)
        {
            Shift = Int32.MaxValue;
            Correlation = Double.MaxValue;

            for (int i = 0; i < a.Length - windowSize; i++)
            {
                for (int j = 0; j < b.Length - windowSize; j++)
                {
                    double cor = Cross(ref a, i, ref b, j, windowSize);
                    if (cor < Correlation)
                    {
                        Correlation = cor;
                        Shift = j - i;
                    }
                }
            }
        }
        private double Cross(ref float[] a, int aStart, ref float[] b, int bStart, int size)
        {
            double cr = 0.0;
            for (int i = aStart, j = bStart; i < aStart + size; i++, j++)
            {
                cr += (a[i] - b[j]) * (a[i] - b[j]);
            }
            return Math.Sqrt(cr / size);
        }
        private double Diff(ref float[] a, int aStart, ref float[] b, int bStart, int size)
        {
            double cr = 0.0;
            for (int i = aStart, j = bStart; i < aStart + size; i++, j++)
            {
                cr += Math.Abs(a[i] - b[j]);
            }
            return cr;
        }
    }
}
