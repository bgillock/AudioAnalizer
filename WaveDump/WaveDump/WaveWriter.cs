using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WaveDump
{
    public class WaveWriter
    {
        public static void Save2Channel16Bit(float[] left, float[] right, int samplesPerSecond, string fileName)
        {
            var mStrm = new FileStream(fileName, FileMode.CreateNew);
            BinaryWriter writer = new BinaryWriter(mStrm);

            int formatChunkSize = 16;
            int headerSize = 8;
            short formatType = 1;
            short tracks = 2;
            short bitsPerSample = 16;
            short frameSize = (short)(tracks * ((bitsPerSample + 7) / 8));
            int bytesPerSecond = samplesPerSecond * frameSize;
            int waveSize = 4;
            int samples = left.Length;
            int dataChunkSize = samples * frameSize;
            int fileSize = waveSize + headerSize + formatChunkSize + headerSize + dataChunkSize;
            // var encoding = new System.Text.UTF8Encoding();
            writer.Write(0x46464952); // = encoding.GetBytes("RIFF")
            writer.Write(fileSize);
            writer.Write(0x45564157); // = encoding.GetBytes("WAVE")
            writer.Write(0x20746D66); // = encoding.GetBytes("fmt ")
            writer.Write(formatChunkSize);
            writer.Write(formatType);
            writer.Write(tracks);
            writer.Write(samplesPerSecond);
            writer.Write(bytesPerSecond);
            writer.Write(frameSize);
            writer.Write(bitsPerSample);
            writer.Write(0x61746164); // = encoding.GetBytes("data")
            writer.Write(dataChunkSize);
            {
                for (int step = 0; step < samples; step++)
                {
                    writer.Write((short)left[step]);
                    writer.Write((short)right[step]);
                }
            }

            writer.Close();
            mStrm.Close();
        } 
    }
}
