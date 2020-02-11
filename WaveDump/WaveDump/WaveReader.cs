using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WaveDump
{
    public class WaveReader
    {
        private string _fname;
        private unsafe void Flip(ref byte* f)
        {
            byte temp;
            temp = f[0];
            f[0] = f[3];
            f[3] = temp;
            temp = f[1];
            f[1] = f[2];
            f[2] = temp;

        }
        private string GetString(ref byte[] input, int index, int size)
        {
            string retValue = "";
            for (int i = index; i < index + size; i++)
            {
                retValue += (char)input[i];
            }
            return retValue;
        }
        private unsafe int GetFlipInt(ref byte[] input, int index)
        {
            int output = 0;

            try
            {
                fixed (byte* b = input)
                {
                    byte* c = b + index;
                    Flip(ref c);
                    int* i = (int*)c;
                    output = *i;
                }
            }
            catch (Exception ex)
            {
            }
            return output;
        }
        private unsafe int GetInt(ref byte[] input, int index)
        {
            int output = 0;
            try
            {
                fixed (byte* b = input)
                {
                    byte* c = b + index;

                    int* i = (int*)c;
                    output = *i;
                }
            }
            catch (Exception ex)
            {
            }
            return output;
        }
        private unsafe short GetShort(ref byte[] input, int index)
        {
            short output = 0;
            try
            {
                fixed (byte* b = input)
                {
                    byte* c = b + index;

                    short* i = (short*)c;
                    output = *i;
                }
            }
            catch (Exception ex)
            {
            }
            return output;
        }
        private byte[] readChunk(BinaryReader reader, string desiredChunkID)
        {
            reader.BaseStream.Seek(12, SeekOrigin.Begin);
            byte[] wavein = reader.ReadBytes(8);
            string thisChunkID = GetString(ref wavein, 0, 4);   
            int chunkSize = GetInt(ref wavein, 4);
            while ((thisChunkID != desiredChunkID))
            {
                reader.BaseStream.Seek(chunkSize, SeekOrigin.Current);
                wavein = reader.ReadBytes(8);
                thisChunkID = GetString(ref wavein, 0, 4);
                chunkSize = GetInt(ref wavein, 4);
            }
            if (thisChunkID == desiredChunkID)
            {
                //System.Console.WriteLine(thisChunkID + " ChunkSize= " + chunkSize); 
                byte[] wave = new byte[chunkSize];
                wave = reader.ReadBytes(chunkSize);

                return wave;
            }

            return null;        
        }
        private unsafe int Get24(ref byte[] input, int index)
        {
            int output = 0;
            try
            {
                if ((input[index] & 0x80) == 0)
                {
                    byte[] b24 = new byte[4];
                    b24[0] = 0xFF;
                    b24[1] = input[index];
                    b24[2] = input[index + 1];
                    b24[3] = input[index + 2];
                    fixed (byte* b = b24)
                    {
                        byte* c = b;

                        int* i = (int*)c;
                        output = *i;
                    }
                }
                else
                {
                    byte[] b24 = new byte[4];
                    b24[0] = 0x00;
                    b24[1] = input[index];
                    b24[2] = input[index + 1];
                    b24[3] = input[index + 2];
                    fixed (byte* b = b24)
                    {
                        byte* c = b;

                        int* i = (int*)c;
                        output = *i;
                    }
                }
            }
            catch (Exception ex)
            {
            }
            return output;
        }
        private unsafe void Set24(ref byte[] input, int index, int s)
        {
            int output = 0;
            try
            {
                if ((input[index] & 0x80) == 0)
                {
                    byte[] b24 = new byte[4];
                    b24[0] = 0xFF;
                    b24[1] = input[index];
                    b24[2] = input[index + 1];
                    b24[3] = input[index + 2];
                    fixed (byte* b = b24)
                    {
                        byte* c = b;

                        int* i = (int*)c;
                        output = *i;
                    }
                }
                else
                {
                    byte[] b24 = new byte[4];
                    b24[0] = 0x00;
                    b24[1] = input[index];
                    b24[2] = input[index + 1];
                    b24[3] = input[index + 2];
                    fixed (byte* b = b24)
                    {
                        byte* c = b;

                        int* i = (int*)c;
                        output = *i;
                    }
                }
            }
            catch (Exception ex)
            {
            }
            return;
        }
        public int chunkSize;
        public string format;

        public int subChunk1Size;
        public short audioFormat;
        public short numChannels;
        public int sampleRate;
        public int nSamples;
        public int byteRate;
        public short blockAlign;
        public short bitsPerSample;
        public int bytesPerSample;
        public int subChunk2Size;
        public double trackLength;
        public float[] left;
        public float[] right;

        public WaveReader(string fileName, int shift)
        {
            _fname = fileName;
             using (BinaryReader reader = new BinaryReader(File.Open(_fname, FileMode.Open)))
            {
                int pos = 0;
                int length = (int)reader.BaseStream.Length;
                byte[] wavein = reader.ReadBytes(12);
                pos += 4; // RIFF
                chunkSize = GetInt(ref wavein, pos); pos += 4;
                format = GetString(ref wavein, pos, 4); 

                wavein = readChunk(reader, "fmt ");
                
                pos = 0;
                subChunk1Size = wavein.Length;
                audioFormat = GetShort(ref wavein, pos); pos += 2;

                numChannels = GetShort(ref wavein, pos); pos += 2;
                sampleRate = GetInt(ref wavein, pos); pos += 4;
                byteRate = GetInt(ref wavein, pos); pos += 4;
                blockAlign = GetShort(ref wavein, pos); pos += 2;
                bitsPerSample = GetShort(ref wavein, pos); pos += 2;
                bytesPerSample = (bitsPerSample / 8) * numChannels; 

                byte[] audio = readChunk(reader, "data");

                subChunk2Size = audio.Length;
                pos = 0 ;
                nSamples = subChunk2Size / bytesPerSample;
                nSamples += shift;
                trackLength = nSamples / (double)sampleRate;
                left = new float[nSamples];
                right = new float[nSamples];

                pos = 0;

                int startSample = shift;
                if (startSample < 0)
                {
                    pos += -startSample * bytesPerSample;
                    startSample = 0;
                }
                int endSample = nSamples - 1;            

                int sample = startSample;

                while ((sample <= endSample) && (audioFormat == 1))
                {
                    left[sample] = 0;
                    if (bitsPerSample == 16)
                    {
                        short l = GetShort(ref audio, pos); pos += 2;
                        left[sample] = (float)l;
                    }
                    if (bitsPerSample == 24)
                    {
                        int s = Get24(ref audio, pos); pos += 3;
                        left[sample] = (float)s;
                    }

                  
                    right[sample] = 0;

                    if (numChannels > 1)
                    {
                        if (bitsPerSample == 16)
                        {
                            short r = GetShort(ref audio, pos); pos += 2;
                            right[sample] = (float)r;
                        }
                        if (bitsPerSample == 24)
                        {
                            int s = Get24(ref audio, pos); pos += 3;
                            right[sample] = (float)s;
                        }
                    }
                  
                    sample++;
                }
            }
        }
        
        public int FindShift(WaveReader wr2, float threshold, float windowSize, int correlationSize)
        {
            var wr1 = this;
            // Find start of wr1
            int wr1Start = 0;
            while (wr1Start < wr1.left.Length)
            {
                if ((Math.Abs(wr1.left[wr1Start]) > threshold) || (Math.Abs(wr1.right[wr1Start]) > threshold)) break;
                wr1Start++;

            }

            System.Console.WriteLine("windowSize," + windowSize);
            System.Console.WriteLine("correlationSize," + correlationSize);
            System.Console.WriteLine("wr1Start," + wr1Start + "," + wr1.left[wr1Start] + "," + wr1.right[wr1Start]);

            float[] win1Left = FloatUtils.Window(wr1.left, wr1Start, wr1Start + (int)(windowSize * sampleRate));
            float[] win1Right = FloatUtils.Window(wr1.right, wr1Start, wr1Start + (int)(windowSize * sampleRate));

            float[] win2Left = FloatUtils.Window(wr2.left, 0, (int)(windowSize * sampleRate));
            float[] win2Right = FloatUtils.Window(wr2.right, 0, (int)(windowSize * sampleRate));

            var min1Left = FloatUtils.Min(win1Left);
            var min1Right = FloatUtils.Min(win1Right);
            var max1Left = FloatUtils.Max(win1Left);
            var max1Right = FloatUtils.Max(win1Right);

            var min2Left = FloatUtils.Min(win2Left);
            var min2Right = FloatUtils.Min(win2Right);
            var max2Left = FloatUtils.Max(win2Left);
            var max2Right = FloatUtils.Max(win2Right);

            FloatUtils.Normalize(ref win2Left, Math.Max(Math.Abs(min2Left), max2Left),
                                        Math.Max(Math.Abs(min1Left), max1Left));

            FloatUtils.Normalize(ref win2Right, Math.Max(Math.Abs(min2Right), max2Right),
                                        Math.Max(Math.Abs(min1Right), max1Right));

            int wr2Start = 0;
            while (wr2Start < win2Left.Length)
            {
                if ((Math.Abs(win2Left[wr2Start]) > threshold) || (Math.Abs(win2Right[wr2Start]) > threshold)) break;
                wr2Start++;
            }
            if (wr2Start < win2Left.Length) 

            System.Console.WriteLine("wr2Start," + wr2Start + "," + win2Left[wr2Start] + "," + win2Right[wr2Start]);

            // Find the signature from this point till correlation size, in the original            
            float[] cor2Left = FloatUtils.Window(wr2.left, wr2Start, wr2Start + correlationSize);
            float[] cor2Right = FloatUtils.Window(wr2.right, wr2Start, wr2Start + correlationSize);

            FloatUtils.Normalize(ref cor2Left, Math.Max(Math.Abs(min2Left), max2Left),
                                       Math.Max(Math.Abs(min1Left), max1Left));

            FloatUtils.Normalize(ref cor2Right, Math.Max(Math.Abs(min2Right), max2Right),
                                        Math.Max(Math.Abs(min1Right), max1Right));
            int Shift = Int32.MaxValue;
            double Correlation = Double.MaxValue;

            for (int c = 0; c < 1000; c++) // 2000 should make up for the lag 
            {
                float[] cor1Left = FloatUtils.Window(wr1.left, wr1Start + c, wr1Start + c + correlationSize);
                float[] cor1Right = FloatUtils.Window(wr1.right, wr1Start + c, wr1Start + c + correlationSize);

                double corLeft = FloatUtils.Cross(ref cor1Left, 0, ref cor2Left, 0, correlationSize);
                double corRight = FloatUtils.Cross(ref cor1Right, 0, ref cor2Right, 0, correlationSize);
                if ((corLeft + corRight) < Correlation)
                {
                    Correlation = corLeft + corRight;
                    Shift = c;
                }
            }
            System.Console.WriteLine("Corr Shift," + Shift );

            return wr2Start - wr1Start - Shift;
        }
        public int FindDecimatedShift(WaveReader wr2, float threshold, float windowSize, int correlationSize, int decimation)
        {
            var wr1 = this;

            System.Console.WriteLine("windowSize," + windowSize);
            System.Console.WriteLine("correlationSize," + correlationSize);
            System.Console.WriteLine("decimation," + correlationSize);
            int windowSizeSamples = (int)(windowSize * sampleRate);
            float[] win1Left = FloatUtils.Window(wr1.left, 0, windowSizeSamples );
            float[] win1Right = FloatUtils.Window(wr1.right, 0, windowSizeSamples );

            float[] win2Left = FloatUtils.Window(wr2.left, 0, (int)(windowSize * sampleRate));
            float[] win2Right = FloatUtils.Window(wr2.right, 0, (int)(windowSize * sampleRate));

            var min1Left = FloatUtils.Min(win1Left);
            var min1Right = FloatUtils.Min(win1Right);
            var max1Left = FloatUtils.Max(win1Left);
            var max1Right = FloatUtils.Max(win1Right);

            var min2Left = FloatUtils.Min(win2Left);
            var min2Right = FloatUtils.Min(win2Right);
            var max2Left = FloatUtils.Max(win2Left);
            var max2Right = FloatUtils.Max(win2Right);

            FloatUtils.Normalize(ref win2Left, Math.Max(Math.Abs(min2Left), max2Left),
                                        Math.Max(Math.Abs(min1Left), max1Left));

            FloatUtils.Normalize(ref win2Right, Math.Max(Math.Abs(min2Right), max2Right),
                                        Math.Max(Math.Abs(min1Right), max1Right));
            int wr2Start = 0;
            while (wr2Start < win2Left.Length)
            {
                if ((Math.Abs(win2Left[wr2Start]) > threshold) || (Math.Abs(win2Right[wr2Start]) > threshold)) break;
                wr2Start++;
            }
            if (wr2Start > win2Left.Length) return 0;

            System.Console.WriteLine("wr2Start," + wr2Start + "," + win2Left[wr2Start] + "," + win2Right[wr2Start]);

            int Shift = Int32.MaxValue;
            double Correlation = Double.MaxValue;
            System.Console.WriteLine("wim2Length," + win2Left.Length);
            for (int w2 = wr2Start; w2 < win2Left.Length - correlationSize; w2 += correlationSize)
            {
                for (int w1 = 0; w1 < win2Left.Length - correlationSize; w1++)
                {
                    double corLeft = FloatUtils.Cross(ref win1Left, w1, ref win2Left, w2, correlationSize, decimation);
                    double corRight = FloatUtils.Cross(ref win1Right, w1, ref win2Right, w2, correlationSize, decimation);

                    if ((corLeft + corRight) < Correlation)
                    {
                        Correlation = corLeft + corRight;
                        Shift = w2 - w1;
                    }
                }
                System.Console.Write(w2 + ",");
            }

            System.Console.WriteLine("Decimated Corr Shift," + Shift + ",Correlation," + Correlation);

            return Shift;
        }
        public int Shift(WaveReader wr2, float maxTime, float windowSize, int nWindows, int correlationSize)
        {
            var wr1 = this;
            int nWin = nWindows + 1;
            int[] shifts = new int[nWindows * 2];
            int ns = 0;
            for (float windowStart = (maxTime / nWin); windowStart < maxTime; windowStart += (maxTime / nWin))
            {
                float windowStop = windowStart + windowSize;
                int startSample = (int)Math.Truncate(windowStart * (float)wr1.sampleRate);
                int endSample = (int)Math.Truncate(windowStop * (double)wr1.sampleRate);

                float[] win1Left = FloatUtils.Window(wr1.left, startSample, endSample);
                float[] win1Right = FloatUtils.Window(wr1.right, startSample, endSample);
                float[] win2Left = FloatUtils.Window(wr2.left, startSample, endSample);
                float[] win2Right = FloatUtils.Window(wr2.right, startSample, endSample);

                var corLeft = new Correlate(win1Left, win2Left, correlationSize);
                var corRight = new Correlate(win1Right, win2Right, correlationSize);
                shifts[ns++] = corLeft.Shift;
                shifts[ns++] = corRight.Shift;
                System.Console.WriteLine("Shift," + corLeft.Shift + "," + corLeft.Correlation + "," + corRight.Shift + "," + corRight.Correlation); 
            }

            int sum = 0;
            for (int i = 0; i < shifts.Length; i++)
            {
                sum += shifts[i];
            }
            return sum / shifts.Length;
        }
        public void Dump()
        {
            System.Diagnostics.Debug.WriteLine("fileName," + _fname);
            System.Diagnostics.Debug.WriteLine("fileSize," + chunkSize);
            System.Diagnostics.Debug.WriteLine("Format," + format);
            System.Diagnostics.Debug.WriteLine("subChunk1Size," + subChunk1Size);
            System.Diagnostics.Debug.WriteLine("audioFormat," + audioFormat);
            System.Diagnostics.Debug.WriteLine("numChannels," + numChannels);
            System.Diagnostics.Debug.WriteLine("sampleRate," + sampleRate);
            System.Diagnostics.Debug.WriteLine("byteRate," + byteRate);
            System.Diagnostics.Debug.WriteLine("blockAlign," + blockAlign);
            System.Diagnostics.Debug.WriteLine("bitsPerSample," + bitsPerSample);
            System.Diagnostics.Debug.WriteLine("bytesPerSample," + bytesPerSample);
            System.Diagnostics.Debug.WriteLine("subChunk2Size," + subChunk2Size);
            System.Diagnostics.Debug.WriteLine("nSamples," + nSamples);
            System.Diagnostics.Debug.WriteLine("trackLength," + trackLength );
        }
        
    }
}
