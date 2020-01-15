using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WaveDump
{
    public static class FloatUtils
    {
        static public float[] Window(float[] a, int start, int end)
        {
            float[] outFloat = new float[end - start + 1];
            for (int i = 0, j = start; (j <= end) && (j < a.Length); i++, j++)
            {
                outFloat[i] = a[j];
            }
            return outFloat;
        }
        static public float[] Window(float[] a, int sampleRateIn, float start, float end, int sampleRateOut)
        {
            int nOut = (int)Math.Ceiling((end - start) * (float)sampleRateOut) + 1;
            float[] outFloat = new float[nOut];
            
            int startIn = (int)(start * sampleRateIn) - 2;
            int endIn = (int)(end * sampleRateIn) + 2;
            float[] x = new float[endIn - startIn + 5];
            float[] y = new float[x.Length];
            for (int i=0; i<x.Length; i++)
            {
                x[i] = (float)((float)(startIn + i) / (float)sampleRateIn);
                y[i] = a[startIn + i];
            }

            for (int i = 0; i < nOut; i++)
            {
                double thisTime = start + ((float)i / (float)sampleRateOut);
                int sidx = (int)((thisTime - x[0]) * (float)sampleRateIn) - 2;
                if (sidx < 0) sidx = 0;
                double thisAmp;
                dd_apprx(x,y,sidx,5,thisTime,out thisAmp);
                outFloat[i] = (float)thisAmp;
            }
            return outFloat;
        }
        static public void dd_apprx(
                float[] x,
                float[] y,
                int sidx,
                int num_pts,
                double x_intrp,
                out double y_intrp)
        {
            int i, j;
            double[] d = new double[num_pts];
            double diff_prod;

            for (i = 0; i < num_pts; i++)
                d[i] = y[sidx + i];

            for (j = 1; j < num_pts; j++)
                for (i = num_pts - 1; i >= j; i--)
                    d[i] = (d[i] - d[i - 1]) / (x[sidx + i] - x[sidx + i - j]);

            y_intrp = d[0];

            for (i = 1; i < num_pts; i++)
            {
                diff_prod = 1.0;

                for (j = 0; j < i; j++)
                    diff_prod *= (x_intrp - x[sidx + j]);

                y_intrp += d[i] * diff_prod;
            }
        } 

        static public double Distortion(float[] a, float[] b)
        {
            if (a.Length != b.Length) return double.MaxValue;

            double sum = 0.0;
            for (int s = 0; s <b.Length; s++)
            {
                sum += (a[s] - b[s]) * (a[s] - b[s]);
   
            }
            return Math.Sqrt(sum / (double)(a.Length));
        }
        static public float Min(float[] a)
        {
            float min = float.MaxValue;           
            for (int i=0;i<a.Length; i++)
            {
                min = Math.Min(min, a[i]);
            }
            return min;
        }
        static public float Max(float[] a)
        {
            float max = float.MinValue;
            for (int i = 0; i < a.Length; i++)
            {
                max = Math.Max(max, a[i]);
            }
            return max;
        }
        static public void Normalize(ref float[] a, double aMax, double toMax)
        {
            double factor = 1.0f;

            factor = toMax / aMax;
            for (int i = 0; i < a.Length; i++)
            {
                a[i] = (float)((double)a[i] * factor);
            }
        }
        static public void Subtract(ref float[] a, float[] b)
        {
            for (int i = 0; i < a.Length; i++)
            {
                a[i] -= b[i];
            }
        }
        static public double RMS(float[] a)
        {
            double sum = 0.0;
            for (int i = 0; i < a.Length; i++)
            {
                sum += a[i] * a[i];
            }
            return Math.Sqrt(sum/a.Length);
        }

        static public double[] FrequencySpectrum(float[] a, int windowSize)
        {
            double[] outputFreq = new double[windowSize / 2];

            for (int i = 0; i <a.Length; i++)
            {
                float[] xout = null;
                FFT.FreqsWindow(ref a, i, windowSize, ref xout);
                for (int s = 0; s < xout.Length; s++)
                {
                    outputFreq[s] += xout[s];
                }
            }
            double min = double.MaxValue;
            double max = double.MinValue;
            for (int i = 0; i < outputFreq.Length; i++)
            {
                min = Math.Min(min, outputFreq[i]);
                max = Math.Max(max, outputFreq[i]);
            }
            for (int i = 0; i < outputFreq.Length; i++)
            {
                outputFreq[i] = FFT.LinearInterp(min, max, outputFreq[i], 0.0, 1.0);
            }
            return outputFreq;
        }
        static public double Cross(ref float[] a, int aStart, ref float[] b, int bStart, int size)
        {
            double cr = 0.0;
            for (int i = aStart, j = bStart; i < aStart + size; i++, j++)
            {
                cr += (a[i] - b[j]) * (a[i] - b[j]);
            }
            return Math.Sqrt(cr / size);
        }
        static public double Cross(ref float[] a, int aStart, ref float[] b, int bStart, int size, int dec)
        {
            double cr = 0.0;
            for (int i = aStart, j = bStart; i < aStart + size; i+=dec, j+=dec)
            {
                cr += (a[i] - b[j]) * (a[i] - b[j]);
            }
            return Math.Sqrt(cr / (size/dec));
        }
    }

}
