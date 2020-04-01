using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WaveDump
{
    public class AudioReader
    {
        public string _fname;
        public long fileSize;
        public string format;
        public short numChannels;
        public int sampleRate;
        public int nSamples;
        public short bitsPerSample;
        public int bytesPerSample;
        public double trackLength;
        public float[] left;
        public float[] right;
        public int[] histogramLeft;
        public int[] histogramRight;
 

        public AudioReader(string fileName)
        {
            _fname = fileName;
        }

        virtual public void Dump()
        {
            System.Diagnostics.Debug.WriteLine("fileName=" + _fname);
            System.Diagnostics.Debug.WriteLine("fileSize=" + fileSize);
            System.Diagnostics.Debug.WriteLine("Format=" + format);
            System.Diagnostics.Debug.WriteLine("numChannels=" + numChannels);
            System.Diagnostics.Debug.WriteLine("nSamples=" + nSamples);
            System.Diagnostics.Debug.WriteLine("sampleRate=" + sampleRate);
            System.Diagnostics.Debug.WriteLine("trackLength=" + trackLength);
            System.Diagnostics.Debug.WriteLine("bitsPerSample=" + bitsPerSample);

        }
        
    }
}
