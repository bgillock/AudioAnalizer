using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WaveDump
{
    public class WaveReader : AudioReader
    {
        public enum Channel
        {
            LEFT,
            RIGHT,
            HISTOGRAM
        }
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
                if ((input[index+2] & 0x80) != 0)
                {
                    byte[] b24 = new byte[4];
                    b24[0] = input[index + 0];
                    b24[1] = input[index + 1];
                    b24[2] = input[index + 2];
                    b24[3] = 0xFF;
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
                    b24[0] = input[index + 0];
                    b24[1] = input[index + 1];
                    b24[2] = input[index + 2];
                    b24[3] = 0x00;
                    fixed (byte* b = b24)
                    {
                        byte* c = b;

                        int* i = (int*)c;
                        output = *i;
                        if (output > 0)
                        {
                            float a = (float)output;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
            }
            return output;
        }
        private unsafe int GetU24(ref byte[] input, int index)
        {
            int output = 0;
            try
            {
                    byte[] b24 = new byte[4];
                    b24[0] = input[index + 2];
                    b24[1] = input[index + 1];
                    b24[2] = input[index];
                    b24[3] = 0x00;
                fixed (byte* b = b24)
                    {
                        byte* c = b;

                        int* i = (int*)c;
                        output = *i;
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
                if ((input[index] & 0x80) != 0)
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
        public int subChunk1Size;
        public short audioFormat;
        public int byteRate;
        public short blockAlign;
        public int bytesPerSample;
        public int subChunk2Size;

        public WaveReader(string fileName) : base(fileName)
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

                trackLength = nSamples / (double)sampleRate;
                left = new float[nSamples];
                right = new float[nSamples];

                int histogramSize = (int)Math.Pow(2, (double)bitsPerSample);
                histogramLeft = new int[histogramSize];
                histogramRight = new int[histogramSize];
                for (int i = 0; i < histogramSize; i++) { histogramLeft[i] = histogramRight[i] = 0; }

                pos = 0;
                int startSample = 0;
                int endSample = nSamples - 1;            
                int sample = startSample;

                while ((sample <= endSample) && (audioFormat == 1))
                {
                    left[sample] = 0;
                    if (bitsPerSample == 16)
                    {
                        short s = GetShort(ref audio, pos); pos += 2;
                        int hi = s + (histogramSize / 2);
                        if ((hi >= 0) && (hi < histogramLeft.Length)) histogramLeft[hi]++;
                        left[sample] = (float)s;
                    }
                    if (bitsPerSample == 24)
                    {
                        int s24 = Get24(ref audio, pos); pos += 3;

                        int hi = s24 + (histogramSize / 2);
                        if ((hi >= 0) && (hi < histogramLeft.Length)) histogramLeft[hi]++;
                        left[sample] = (float)s24;
                    }

                    right[sample] = 0;
                    if (numChannels > 1)
                    {
                        if (bitsPerSample == 16)
                        {
                            short s = GetShort(ref audio, pos); pos += 2;
                            int hi = s + (histogramSize / 2);
                            if ((hi >= 0) && (hi < histogramRight.Length)) histogramRight[hi]++;
                            right[sample] = (float)s;
                        }
                        if (bitsPerSample == 24)
                        {
                            int s24 = Get24(ref audio, pos); pos += 3;
                            int hi = s24 + (histogramSize / 2);
                            if ((hi >= 0) && (hi < histogramRight.Length)) histogramRight[hi]++;
                            right[sample] = (float)s24;
                        }
                    }
                  
                    sample++;
                }
            }
        }
        override public void Dump()
        {
            base.Dump();
            System.Diagnostics.Debug.WriteLine("subChunk1Size," + subChunk1Size);
            System.Diagnostics.Debug.WriteLine("audioFormat," + audioFormat);
            System.Diagnostics.Debug.WriteLine("byteRate," + byteRate);
            System.Diagnostics.Debug.WriteLine("blockAlign," + blockAlign);
            System.Diagnostics.Debug.WriteLine("bytesPerSample," + bytesPerSample);
            System.Diagnostics.Debug.WriteLine("subChunk2Size," + subChunk2Size);
        }
        
    }
}
