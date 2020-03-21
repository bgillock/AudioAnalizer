using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WaveDump
{
    public class FlacReader
    {
        private string _fname;
        private enum BlockType { StreamInfo, Padding, Application, SeekTable, VorbisComment, CueSheet, Picture, Unknown };

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

        private string GetHexString(ref byte[] input, int index, int size)
        {
            StringBuilder hex = new StringBuilder(size * 2);
            for (int i = index; i < index + size; i++)
            {
                hex.AppendFormat("{0:x2}", input[i]);
            }
            return hex.ToString();
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

        struct SubStreamInfo
        {
            public uint sampleRate;
            public uint numChannels;
            public uint bitsPerSample;
            public ulong totalSamplesInStream;
        } 
        private unsafe SubStreamInfo GetSubStreamInfo(ref byte[] input, int index)
        {
            SubStreamInfo ss = new SubStreamInfo
            {
                sampleRate = 44100,
                numChannels = 1,
                bitsPerSample = 15,
                totalSamplesInStream = 1000000
            };

            // Extract sampleRate
            byte[] b24 = new byte[4];
            b24[0] = input[index + 2];
            b24[1] = input[index + 1];
            b24[2] = input[index];
            b24[3] = 0x00;
            fixed (byte* b = b24)
            {
                uint* i = (uint*)b;
                uint si = *i >> 4;
                ss.sampleRate = si;
            }

            // Extract numChannels
            byte mask = 0x0E;
            b24[0] = (byte)(input[index + 2] & mask);
            b24[1] = 0x00;
            b24[2] = 0x00;
            b24[3] = 0x00;
            fixed (byte* b = b24)
            {
                uint* i = (uint*)b;
                uint si = *i >> 1;
                ss.numChannels = si;
            }

            // Extract bitsPerSample
            mask = 0xF0;
            b24[0] = (byte)(input[index + 3] & mask);
            mask = 0x01;
            b24[1] = (byte)(input[index + 2] & mask);
            b24[2] = 0x00;
            b24[3] = 0x00;
            fixed (byte* b = b24)
            {
                uint* i = (uint*)b;
                uint si = *i >> 4;
                ss.bitsPerSample = si;
            }

            // Extract totalSamples
            byte[] b64 = new byte[8];
            b64[0] = input[index + 7];
            b64[1] = input[index + 6];
            b64[2] = input[index + 5];
            b64[3] = input[index + 4];
            mask = 0x0F;
            b64[4] = (byte)(input[index + 3] & mask);
            b64[5] = 0x00;
            b64[6] = 0x00;
            b64[7] = 0x00;
            fixed (byte* b = b64)
            {
                ulong* i = (ulong*)b;
                ulong si = *i;
                ss.totalSamplesInStream = si;
            }
            return ss;
        }
        private byte[] readChunk(BinaryReader reader, string desiredChunkID)
        {
            reader.BaseStream.Seek(12, SeekOrigin.Begin);
            byte[] flacin = reader.ReadBytes(8);
            string thisChunkID = GetString(ref flacin, 0, 4);   
            int chunkSize = GetInt(ref flacin, 4);
            while ((thisChunkID != desiredChunkID))
            {
                reader.BaseStream.Seek(chunkSize, SeekOrigin.Current);
                flacin = reader.ReadBytes(8);
                thisChunkID = GetString(ref flacin, 0, 4);
                chunkSize = GetInt(ref flacin, 4);
            }
            if (thisChunkID == desiredChunkID)
            {
                //System.Console.WriteLine(thisChunkID + " ChunkSize= " + chunkSize); 
                byte[] flac = new byte[chunkSize];
                flac = reader.ReadBytes(chunkSize);

                return flac;
            }

            return null;        
        }
        private unsafe int Get24(ref byte[] input, int index)
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

        private BlockType GetBlockType(byte metaByte)
        {
            int bti = (byte)(metaByte & 0x7F);
            BlockType[] bta = { BlockType.StreamInfo, BlockType.Padding, BlockType.Application, BlockType.SeekTable, BlockType.VorbisComment, BlockType.CueSheet, BlockType.Picture };
            if ((bti >= 0) && (bti < bta.Length)) return bta[bti];
            else return BlockType.Unknown;
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
        public int[] histogram;


        public FlacReader(string fileName)
        {

            _fname = fileName;
            using (BinaryReader reader = new BinaryReader(File.Open(_fname, FileMode.Open)))
            {
                int pos = 0;
                int length = (int)reader.BaseStream.Length;
                byte[] flacin = reader.ReadBytes(4);  // fLaC
                flacin = reader.ReadBytes(4); // Meta data block header
                System.Diagnostics.Debug.WriteLine("Filename=" + _fname);
                byte last_type = flacin[0];
                pos += 1;
                int mbl = GetU24(ref flacin, pos);
                System.Diagnostics.Debug.WriteLine("MetaDataBlockLen=" + mbl);

                BlockType blockType = GetBlockType(last_type);
                System.Diagnostics.Debug.WriteLine("BlockType=" + blockType.ToString());
                bool lastHeader = (last_type & 0x80) != 0;

                do
                {
                    switch (blockType)
                    {
                        case BlockType.StreamInfo:
                          
                            flacin = reader.ReadBytes(mbl);
                            pos = 0;
                            short minimumBlockSize = GetShort(ref flacin, pos); pos += 2;
                            short maximumBlockSize = GetShort(ref flacin, pos); pos += 2;
                            int minimumFrameSize = GetU24(ref flacin, pos); pos += 3;
                            int maximumFrameSize = GetU24(ref flacin, pos); pos += 3;
                            SubStreamInfo ss = GetSubStreamInfo(ref flacin, pos); pos += 8;
                            string md5 = GetHexString(ref flacin, pos, 16); pos += 16;
                            System.Diagnostics.Debug.WriteLine("minimumBlockSize=" + minimumBlockSize);
                            System.Diagnostics.Debug.WriteLine("maximumBlockSize=" + maximumBlockSize);
                            System.Diagnostics.Debug.WriteLine("minimumFrameSize=" + minimumFrameSize);
                            System.Diagnostics.Debug.WriteLine("maximumFrameSize=" + maximumFrameSize);
                            System.Diagnostics.Debug.WriteLine("sampleRate=" + ss.sampleRate);
                            System.Diagnostics.Debug.WriteLine("numChannels=" + ss.numChannels);
                            System.Diagnostics.Debug.WriteLine("bitsPerSample=" + ss.bitsPerSample);
                            System.Diagnostics.Debug.WriteLine("totalSamplesInStream=" + ss.totalSamplesInStream);
                            System.Diagnostics.Debug.WriteLine("md5=" + md5);
                            System.Diagnostics.Debug.WriteLine("FinalPosofStreamInfo=" + pos);
                            break;
                             
                        case BlockType.VorbisComment:
                            flacin = reader.ReadBytes(mbl);
                            pos = 0;
                            int vendor_length = GetInt(ref flacin, pos); pos += 4;
                            string vendor_string = GetString(ref flacin, pos, vendor_length); pos += vendor_length;
                            System.Diagnostics.Debug.WriteLine("vendor_string=" + vendor_string);
                            int user_comment_list_length = GetInt(ref flacin, pos); pos += 4;
                            for (int i = 0; i < user_comment_list_length; i++)
                            {
                                int comment_length = GetInt(ref flacin, pos); pos += 4;
                                string comment = GetString(ref flacin, pos, comment_length); pos += comment_length;
                                System.Diagnostics.Debug.WriteLine("comment=" + comment);
                            }
                            break;
                        case BlockType.Application:
                        case BlockType.CueSheet:
                        case BlockType.Padding:
                        case BlockType.Picture:
                        case BlockType.SeekTable:
                        case BlockType.Unknown:
                            flacin = reader.ReadBytes(mbl);
                            break;

                    }

                    if (!lastHeader)
                    {
                        flacin = reader.ReadBytes(4); pos = 0; // Meta data block header

                        last_type = flacin[0];
                        pos += 1;
                        mbl = GetU24(ref flacin, pos);
                        System.Diagnostics.Debug.WriteLine("MetaDataBlockLen=" + mbl);
                        blockType = GetBlockType(last_type);
                        System.Diagnostics.Debug.WriteLine("BlockType=" + blockType.ToString());
                        lastHeader = (last_type & 0x80) != 0;
                    }
                } while (!lastHeader);
            }
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
