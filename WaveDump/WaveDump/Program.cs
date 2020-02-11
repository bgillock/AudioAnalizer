using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace WaveDump
{
    class Program
    {

        static int Dominant(double[] freqs)
        {
            int retValue = 0;
            double max = freqs[0];
            for (int i = 1; i < freqs.Length; i++)
            {
                if (freqs[i] > max) { retValue = i; max = freqs[i]; }
            }
            return retValue;
        }
        static void Main(string[] args)
        {
            
            float[] a = { -1f, 2f, 3f, 2f, -1f, -4f,-2f, 1f, 2f };

            //float[] r = FloatUtils.Window(a, 8, 0.33f, 0.66f, 6 );


            string fileName = args[0];

            if (args.Length == 1)
            {
                var wr0 = new WaveReader(args[0], 0);
                wr0.Dump();
                return;
            }
            if (!System.IO.File.Exists(args[0])) { System.Console.WriteLine("File " + args[0] + " does not exist."); return; }
            if (!System.IO.File.Exists(args[1])) { System.Console.WriteLine("File " + args[1] + " does not exist."); return; }

            var wr1 = new WaveReader(args[0], 0);
            var wr2 = new WaveReader(args[1], 0);


            // if (wr1.sampleRate != wr2.sampleRate) { System.Console.WriteLine("Sample rates don't match"); return; }
            /*
            float windowSize = float.Parse(args[2]);
            float windowStep = float.Parse(args[3]);

            for (float startTime = 0.0f; startTime < wr1.trackLength - windowSize; startTime += windowStep)
            {
                float startWindow = startTime;
                float endWindow = startTime + windowSize;

                double error = Distortion(wr1, wr2, startWindow, endWindow);

                double[] f1l = wr1.FrequencySpectrumLeft(1024, startWindow, endWindow);
                double[] f2l = wr2.FrequencySpectrumLeft(1024, startWindow, endWindow);
                double[] difff = new double[f1l.Length];

                for (int i = 0; i < f1l.Length; i++)
                {
                    difff[i] = Math.Abs(f1l[i] - f2l[i]);
                }
                for (int i = 0; i < f1l.Length; i++)
                {
                    //               System.Console.WriteLine(difff[i]);
                }

                // wr1.Dump(startWindow, endWindow);
                // System.Console.WriteLine("---------------------------------------");
                // wr2.Dump(startWindow, endWindow);
                var freqError = Distortion(f1l, f2l);
                System.Console.WriteLine(startTime + "," + error + "," + freqError + "," + Dominant(f1l));
            }
            */

            float startTime = float.Parse(args[2]);
            float endTime = float.Parse(args[3]);

            float[] clean = FloatUtils.Window(wr1.left, (int)(startTime * wr1.sampleRate), (int)(endTime * wr1.sampleRate));
            float[] dirty = FloatUtils.Window(wr2.left, (int)(startTime * wr2.sampleRate), (int)(endTime * wr2.sampleRate));

            float maxDirty = FloatUtils.Max(dirty);
            float maxClean = FloatUtils.Max(clean);
            FloatUtils.Normalize(ref wr1.left, (double)maxClean, (double)maxDirty);
            FloatUtils.Subtract(ref wr2.left, wr1.left);
            WaveWriter.Save2Channel16Bit(wr2.left,wr2.left,wr2.sampleRate,System.IO.Path.GetDirectoryName(fileName)+@"\cleamed.wav");
           
            return;
            /*
            float threshold = 1.0f;
            if (args.Length > 4) threshold = float.Parse(args[4]);

            wr1.Dump();
            wr2.Dump();

            float maxTime = Math.Min((float)wr1.trackLength, (float)wr2.trackLength);
            maxTime = 55.0f;

            int shiftWr2 = wr1.FindDecimatedShift(wr2, threshold, maxOffset, 5000, 20);
            float minTime = ((float)shiftWr2 / (float)wr2.sampleRate);
            maxTime -= minTime;
            System.Console.WriteLine("threshold," + threshold);
            System.Console.WriteLine("maxOffset," + maxOffset);  
            System.Console.WriteLine("Shift," + shiftWr2);
            System.Console.WriteLine("WindowSize," + windowSize);

            int nWin = 50;
            for (float windowStart = minTime + (maxTime / nWin); windowStart < maxTime; windowStart += (maxTime / nWin))
            {
                float windowStop = windowStart + windowSize;
                int startSample = (int)Math.Truncate(windowStart * (float)wr1.sampleRate);
                int endSample = (int)Math.Truncate(windowStop * (double)wr1.sampleRate);

                float[] win1Left = FloatUtils.Window(wr1.left, startSample, endSample);
                float[] win1Right = FloatUtils.Window(wr1.right, startSample, endSample);
                float[] win2Left = FloatUtils.Window(wr2.left, startSample + shiftWr2, endSample + shiftWr2);
                float[] win2Right = FloatUtils.Window(wr2.right, startSample + shiftWr2, endSample + shiftWr2);


                var rms1Left = FloatUtils.RMS(win1Left);
                var rms1Right = FloatUtils.RMS(win1Right);
                var rms2Left = FloatUtils.RMS(win2Left);
                var rms2Right = FloatUtils.RMS(win2Right);

                var min1Left = FloatUtils.Min(win1Left);
                var min1Right = FloatUtils.Min(win1Right);
                var max1Left = FloatUtils.Max(win1Left);
                var max1Right = FloatUtils.Max(win1Right);
                
                var min2Left = FloatUtils.Min(win2Left);
                var min2Right = FloatUtils.Min(win2Right);
                var max2Left = FloatUtils.Max(win2Left);
                var max2Right = FloatUtils.Max(win2Right);
                
                FloatUtils.Normalize(ref win2Left, Math.Max(Math.Abs(min2Left),max2Left),
                                            Math.Max(Math.Abs(min1Left),max1Left));

                FloatUtils.Normalize(ref win2Right, Math.Max(Math.Abs(min2Right),max2Right),
                                            Math.Max(Math.Abs(min1Right),max1Right));

                var distLeft = FloatUtils.Distortion(win1Left, win2Left);
                var distRight = FloatUtils.Distortion(win1Right, win2Right);

                System.Console.WriteLine(windowStart + "," +
                                     (rms1Left + rms1Right) / 2.0 + "," +
                                     (rms2Left + rms2Right) / 2.0 + "," +
                                      distLeft + "," +
                                      distRight);
            }
             */
           // System.Console.WriteLine(" ");
        }

            /*
        SignalGenerator sine = new SignalGenerator(44100, 1);
        sine.Frequency = frequency;
        sine.Type = SignalGeneratorType.Sin;
        sine.Gain = 0.5;

        int nSamples = 4000;
        float[] left = new float[nSamples];
        float[] right = new float[nSamples];
        sine.Read(left, 0, nSamples);
        sine.Read(right, 0, nSamples);
            
        for (int i = 0; i < left.Length; i++) System.Console.WriteLine(i + "," + left[i]);
           
        long[] err = new long[65536];

        for (int i = 0; i < wr1.nSamples; i++)
        {
            if (Math.Abs(wr1.shortl[i] - wr2.shortl[i]) < 32000)
            {
                err[32768 - (wr1.shortl[i] - wr2.shortl[i])]++;
            }
            if (Math.Abs(wr1.shortr[i] - wr2.shortr[i]) < 32000)
            {
                err[32768 - (wr1.shortr[i] - wr2.shortr[i])]++;
            }
        }
            
        for (int i = 0; i < err.Length; i+=binsize)
        {
            long sum = 0;
            for (int j = i; j < i + binsize; j++) if ((j != 32768) && (j < 65536)) sum += err[j];

            System.Console.WriteLine(i-32768 + "," + sum);
        }
         */
       
    }
}
