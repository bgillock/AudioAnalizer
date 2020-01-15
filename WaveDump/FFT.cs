using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
/*----------------------------------------------------------------------------
   fft.c - fast Fourier transform and its inverse (both recursively)
   Copyright (C) 2004, Jerome R. Breitenbach.  All rights reserved.

   The author gives permission to anyone to freely copy, distribute, and use
   this file, under the following conditions:
      - No changes are made.
      - No direct commercial advantage is obtained.
      - No liability is attributed to the author for any damages incurred.
  ----------------------------------------------------------------------------*/

/******************************************************************************
 * This file defines a C function fft that, by calling another function       *
 * fft_rec (also defined), calculates an FFT recursively.  Usage:             *
 *   fft(N, x, X);                                                            *
 * Parameters:                                                                *
 *   N: number of points in FFT (must equal 2^n for some integer n >= 1)      *
 *   x: pointer to N time-domain samples given in rectangular form (Re x,     *
 *      Im x)                                                                 *
 *   X: pointer to N frequency-domain samples calculated in rectangular form  *
 *      (Re X, Im X)                                                          *
 * Similarly, a function ifft with the same parameters is defined that        *
 * calculates an inverse FFT (IFFT) recursively.  Usage:                      *
 *   ifft(N, x, X);                                                           *
 * Here, N and X are given, and x is calculated.                              *
 ******************************************************************************/
namespace WaveDump
{
    public class FFT
    {
        /* macros */
        const double TWO_PI = 6.2831853071795864769252867665590057683943;

        /* FFT */
        static void fft(int N, ref double[,] x, ref double[,] X)
        {
            /* Declare a pointer to scratch space. */
            double[,] XX = new double[N, 2];

            /* Calculate FFT by a recursion. */
            fft_rec(N, 0, 1, ref x, ref X, ref XX);
        }

        /* FFT recursion */
        static void fft_rec(int N, int offset, int delta,
                     ref double[,] x, ref double[,] X, ref double[,] XX)
        {
            int N2 = N / 2;            /* half the number of points in FFT */
            int k;                   /* generic index */
            double cs, sn;           /* cosine and sine */
            int k00, k01, k10, k11;  /* indices for butterflies */
            double tmp0, tmp1;       /* temporary storage */

            if (N != 2)  /* Perform recursive step. */
            {
                /* Calculate two (N/2)-point DFT's. */
                fft_rec(N2, offset, 2 * delta, ref x, ref XX, ref X);
                fft_rec(N2, offset + delta, 2 * delta, ref x, ref XX, ref X);

                /* Combine the two (N/2)-point DFT's into one N-point DFT. */
                for (k = 0; k < N2; k++)
                {
                    k00 = offset + k * delta; k01 = k00 + N2 * delta;
                    k10 = offset + 2 * k * delta; k11 = k10 + delta;
                    cs = Math.Cos(TWO_PI * k / (double)N); sn = Math.Sin(TWO_PI * k / (double)N);
                    tmp0 = cs * XX[k11, 0] + sn * XX[k11, 1];
                    tmp1 = cs * XX[k11, 1] - sn * XX[k11, 0];
                    X[k01, 0] = XX[k10, 0] - tmp0;
                    X[k01, 1] = XX[k10, 1] - tmp1;
                    X[k00, 0] = XX[k10, 0] + tmp0;
                    X[k00, 1] = XX[k10, 1] + tmp1;
                }
            }
            else  /* Perform 2-point DFT. */
            {
                k00 = offset; k01 = k00 + delta;
                X[k01, 0] = x[k00, 0] - x[k01, 0];
                X[k01, 1] = x[k00, 1] - x[k01, 1];
                X[k00, 0] = x[k00, 0] + x[k01, 0];
                X[k00, 1] = x[k00, 1] + x[k01, 1];
            }
        }

        /* IFFT */
        static void ifft(int N, ref double[,] x, ref double[,] X)
        {
            int N2 = N / 2;       /* half the number of points in IFFT */
            int i;              /* generic index */
            double tmp0, tmp1;  /* temporary storage */

            /* Calculate IFFT via reciprocity property of DFT. */
            fft(N, ref X, ref x);
            x[0, 0] = x[0, 0] / N; x[0, 1] = x[0, 1] / N;
            x[N2, 0] = x[N2, 0] / N; x[N2, 1] = x[N2, 1] / N;
            for (i = 1; i < N2; i++)
            {
                tmp0 = x[i, 0] / N; tmp1 = x[i, 1] / N;
                x[i, 0] = x[N - i, 0] / N; x[i, 1] = x[N - i, 1] / N;
                x[N - i, 0] = tmp0; x[N - i, 1] = tmp1;
            }
        }
        public static void HighCut(ref float[] xin, int hdx, float taper)
        {
            int i;                    /* generic index */

            int N, fftlen;            /* number of input point, number of points in FFT */

            double[,] xreal;       /* pointer to time-domain samples - real and imaginary */
            double[,] xfreq;       /* pointer to frequency-domain samples - real and imaginary */

            N = xin.Length;

            /* basic checks on length of input series */

            if (N < 2 || N > 32768)
            {
                return;
            }

            fftlen = power_of_2(N);

            /* Allocate time- and frequency-domain memory. */

            xreal = new double[fftlen, 2];
            xfreq = new double[fftlen, 2];

            for (i = 0; i < fftlen; i++)
            {
                if (i < N)
                {
                    xreal[i, 0] = xin[i]; xreal[i, 1] = 0.0; /* transfer float input to real component then zero imaginary component */
                }
                else
                {
                    xreal[i, 0] = 0.0; xreal[i, 1] = 0.0;    /* Pad to the fftlen with zeros */
                }
            }

            /* Calculate FFT. */
            fft(fftlen, ref xreal, ref xfreq);

            /* Clear time-domain samples and calculate IFFT. */
            for (i = 0; i < fftlen; i++) { xreal[i, 0] = 0.0; xreal[i, 1] = 0.0; }

            int tapelen = (int)((float)hdx * taper);
            for (i = 0; i < tapelen; i++)
            {
                float cut = (float)(tapelen - i) / (float)(tapelen + 1);
                xfreq[hdx + i, 0] = xfreq[hdx + i, 0] * cut;
                xfreq[hdx + i, 1] = xfreq[hdx + i, 1] * cut;

                xfreq[fftlen - 1 - hdx - i, 0] = xfreq[fftlen - 1 - hdx - i, 0] * cut;
                xfreq[fftlen - 1 - hdx - i, 1] = xfreq[fftlen - 1 - hdx - i, 1] * cut;
            }

            /* Clear frequency domain past bdx. */
            for (i = hdx + tapelen; i < fftlen - hdx - tapelen; i++)
            {
                xfreq[i, 0] = 0.0;
                xfreq[i, 1] = 0.0;
            }


            ifft(fftlen, ref xreal, ref xfreq);

            /* Return the filtered input in the input array. */

            for (i = 0; i < xin.Length; i++) { xin[i] = (float)xreal[i, 0]; }
            return;
        }

        public static void BandPass(ref float[] xin, int ldx, int hdx, float taper)
        {
          int i;                    /* generic index */
        
          int N, fftlen;            /* number of input point, number of points in FFT */

          double[,] xreal;       /* pointer to time-domain samples - real and imaginary */
          double[,] xfreq;       /* pointer to frequency-domain samples - real and imaginary */

          N = xin.Length;

         /* basic checks on length of input series */

          if(N < 2 || N > 32768)
            {
               return;
            }

          fftlen = power_of_2 (N);

          /* Allocate time- and frequency-domain memory. */

          xreal = new double[fftlen,2];
          xfreq = new double[fftlen,2];

          for ( i=0; i<fftlen; i++)
          {
                if ( i < N ) 
                {
                    xreal [i,0] = xin[i]; xreal [i,1] = 0.0; /* transfer float input to real component then zero imaginary component */
                }
                else 
                {
                    xreal [i,0] = 0.0; xreal [i,1] = 0.0;    /* Pad to the fftlen with zeros */
                }
          }

          /* Calculate FFT. */
          fft(fftlen, ref xreal, ref xfreq);

          /* Clear time-domain samples and calculate IFFT. */
          for(i=0; i<fftlen; i++) { xreal[i,0] = 0.0; xreal[i,1] = 0.0;}

          /* Clear frequency domain bwfore ldx. */
          for (i = 0; i < ldx; i++)
          {
              float cut = 0.0F;
              if (i > 0) cut = (float)(i) / (float)(ldx);
              xfreq[i, 0] = xfreq[i, 0] * cut; xfreq[fftlen - i - 1, 0] = xfreq[fftlen - i - 1, 0] * cut;
              xfreq[i, 1] = xfreq[i, 1] * cut; xfreq[fftlen - i - 1, 1] = xfreq[fftlen - i - 1, 1] * cut;
          }

          int tapelen = (int) ((float)hdx * taper);
          if (hdx < fftlen - tapelen)
          {
              // taper from hdx to tapelen
              for (i = 0; i < tapelen; i++)
              {
                  float cut = (float)(tapelen - i) / (float)(tapelen + 1);
                  xfreq[hdx + i, 0] = xfreq[hdx + i, 0] * cut;
                  xfreq[hdx + i, 1] = xfreq[hdx + i, 1] * cut;

                  xfreq[fftlen - 1 - hdx - i, 0] = xfreq[fftlen - 1 - hdx - i, 0] * cut;
                  xfreq[fftlen - 1 - hdx - i, 1] = xfreq[fftlen - 1 - hdx - i, 1] * cut;
              }

              /* Clear frequency domain past hdx + tapelen. */
              for (i = hdx + tapelen; i < fftlen - hdx - tapelen; i++)
              {
                  xfreq[i, 0] = 0.0;
                  xfreq[i, 1] = 0.0;
              }
          }
          ifft (fftlen, ref xreal, ref xfreq);
       
          /* Return the filtered input in the input array. */

          for (i = 0; i < xin.Length; i++) { xin[i] = (float)xreal[i, 0]; }
          return;
        }

        public static void Resample(ref float[] xin, ref float[] yin, int bdx, float taper, int resamp, out float[] xout, out float[] yout)
        {
            int i;                    /* generic index */

            int N, fftlen;            /* number of input point, number of points in FFT */

            double[,] xreal;       /* pointer to time-domain samples - real and imaginary */
            double[,] xfreq;       /* pointer to frequency-domain samples - real and imaginary */

            N = xin.Length;
            xout = null;
            yout = null;

            /* basic checks on length of input series */

            if (N < 2 || N > 32768)
            {
                return;
            }

            fftlen = power_of_2(N);

            /* Allocate time- and frequency-domain memory. */

            xreal = new double[fftlen, 2];
            xfreq = new double[fftlen, 2];

            for (i = 0; i < fftlen; i++)
            {
                if (i < N)
                {
                    xreal[i, 0] = xin[i]; xreal[i, 1] = 0.0; /* transfer float input to real component then zero imaginary component */
                }
                else
                {
                    xreal[i, 0] = 0.0; xreal[i, 1] = 0.0;    /* Pad to the fftlen with zeros */
                }
            }

            /* Calculate FFT. */
            fft(fftlen, ref xreal, ref xfreq);

            /* Clear time-domain samples and calculate IFFT. */
            for (i = 0; i < fftlen; i++) { xreal[i, 0] = 0.0; xreal[i, 1] = 0.0; }

            int tapelen = (int)((float)bdx * taper);
            for (i = 0; i < tapelen; i++)
            {
                float cut = (float)(tapelen - i) / (float)(tapelen + 1);
                xfreq[bdx + i, 0] = xfreq[bdx + i, 0] * cut;
                xfreq[bdx + i, 1] = xfreq[bdx + i, 1] * cut;

                xfreq[fftlen - 1 - bdx - i, 0] = xfreq[fftlen - 1 - bdx - i, 0] * cut;
                xfreq[fftlen - 1 - bdx - i, 1] = xfreq[fftlen - 1 - bdx - i, 1] * cut;
            }

            /* Clear frequency domain past bdx. */
            for (i = bdx + tapelen; i < fftlen-bdx-tapelen; i++)
            {
                xfreq[i, 0] = 0.0;
                xfreq[i, 1] = 0.0;
            }

            int newfftlen = fftlen / resamp;

            ifft(newfftlen, ref xreal, ref xfreq);

            /* Return the filtered input in the input array. */
            xout = new float[xin.Length / resamp];
            yout = new float[xin.Length / resamp];

            int j;
            for (i = 0, j = 0; i < xin.Length && j < xout.Length; i += resamp, j++) 
            { 
                xout[j] = (float)xreal[j, 0]; 
                yout[j] = yin[i]; 
            }
            return;
        }
        public static void freqs(ref float[] xin, out float[] xout)
        {
            int i;                    /* generic index */

            int N, fftlen;            /* number of input point, number of points in FFT */

            double[,] xreal;       /* pointer to time-domain samples - real and imaginary */
            double[,] xfreq;       /* pointer to frequency-domain samples - real and imaginary */

            N = xin.Length;
            xout = new float[xin.Length];

            /* basic checks on length of input series */

            if (N < 2 || N > 32768)
            {
                return;
            }

            fftlen = power_of_2(N);

            /* Allocate time- and frequency-domain memory. */

            xreal = new double[fftlen, 2];
            xfreq = new double[fftlen, 2];

            for (i = 0; i < fftlen; i++)
            {
                if (i < N)
                {
                    xreal[i, 0] = xin[i]; xreal[i, 1] = 0.0; /* transfer float input to real component then zero imaginary component */
                }
                else
                {
                    xreal[i, 0] = 0.0; xreal[i, 1] = 0.0;    /* Pad to the fftlen with zeros */
                }
            }

            /* Calculate FFT. */
            fft(fftlen, ref xreal, ref xfreq);
         
            /* Return the frequencies in the output array. */
            for (i = 0; i < xout.Length; i++) { xout[i] = (float)Math.Sqrt((xfreq[i, 0]*xfreq[i,0])+(xfreq[i, 1]*xfreq[i,1])); }
            return;
        }
        public static double LinearInterp(double a, double b, double c, double e, double f)
        {
            if (c == a) return e;
            return e + ((c - a) / (b - a)) * (f - e);
        }
        public static void FreqsWindow(ref float[] xin, int sdx, int n, ref float[] xout)
        {
            int i;                    /* generic index */

            int N, fftlen;            /* number of input point, number of points in FFT */

            double[,] xreal;       /* pointer to time-domain samples - real and imaginary */
            double[,] xfreq;       /* pointer to frequency-domain samples - real and imaginary */

            N = n;
            xout = new float[n/2];

            /* basic checks on length of input series */

            if (N < 2 || N > 32768)
            {
                return;
            }

            fftlen = power_of_2(N);

            /* Allocate time- and frequency-domain memory. */

            xreal = new double[fftlen, 2];
            xfreq = new double[fftlen, 2];

            for (i = 0; i < fftlen; i++)
            {
                if (i < N)
                {
                    xreal[i, 0] = xin[i+sdx]; xreal[i, 1] = 0.0; /* transfer float input to real component then zero imaginary component */
                }
                else
                {
                    xreal[i, 0] = 0.0; xreal[i, 1] = 0.0;    /* Pad to the fftlen with zeros */
                }
            }

            /* Calculate FFT. */
            fft(fftlen, ref xreal, ref xfreq);

            /* Return the frequencies in the output array. */
            for (i = 0; i < xout.Length; i++) { xout[i] = (float)Math.Sqrt((xfreq[i, 0] * xfreq[i, 0]) + (xfreq[i, 1] * xfreq[i, 1])); }
//            for (i = 0; i < xout.Length; i++) { xout[i] = (float)xfreq[i, 1]; }
            return;
        }

        static int power_of_2(int number)
        {
            int near_ptr;
            int power_ptr;
            /* Loop until power of 2 greater than or
             * equal to number is found.
             */
            power_ptr = 0;
            near_ptr = 1;

            while (near_ptr < number)
            {
                near_ptr *= 2;
                power_ptr += 1;
            }

            return near_ptr;

        }
        /* end power_of_2 */
    }
/*==============================================================================
 * Program output (example)
 *==============================================================================
 * Input file for time-domain samples xreal (n)? data.txt
 * N = 8
 * xreal (n):
 *    n=0:     3.600000     2.600000
 *    n=1:     2.900000     6.300000
 *    n=2:     5.600000     4.000000
 *    n=3:     4.800000     9.100000
 *    n=4:     3.300000     0.400000
 *    n=5:     5.900000     4.800000
 *    n=6:     5.000000     2.600000
 *    n=7:     4.300000     4.100000
 * X(k):
 *    k=0:    35.400000    33.900000
 *    k=1:     3.821320     0.892893
 *    k=2:    -5.800000    -3.300000
 *    k=3:     5.971068     7.042641
 *    k=4:    -0.400000   -14.700000
 *    k=5:    -0.421320     2.307107
 *    k=6:    -1.600000    -3.900000
 *    k=7:    -8.171068    -1.442641
 * xreal (n):
 *    n=0:     3.600000     2.600000
 *    n=1:     2.900000     6.300000
 *    n=2:     5.600000     4.000000
 *    n=3:     4.800000     9.100000
 *    n=4:     3.300000     0.400000
 *    n=5:     5.900000     4.800000
 *    n=6:     5.000000     2.600000
 *    n=7:     4.300000     4.100000
 * Output file for frequency-domain samples X(k)?
 *    (if none, abort program): X.txt
 * Samples X(k) were written to file X.txt.
 */

}
