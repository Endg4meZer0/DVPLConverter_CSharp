using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using K4os.Compression.LZ4;
using Force.Crc32;

namespace DVPLConverter
{
    class DVPL
    {
        /// <summary>
        /// Class for data from DVPL footer
        /// </summary>

        public class DVPLFooterData
        {
            public uint oSize { get; set; }
            public uint cSize { get; set; }
            public uint crc32 { get; set; }
            public uint type { get; set; }
        }

        /// <summary>
        /// Reads DVPL footer and returns its data.
        /// </summary>
        /// <param name="buffer">Array of bytes (usually from file), where program will search data.</param>
        /// <returns>DVPL footer data, using class for it.</returns>
        public DVPLFooterData readDVPLFooter(byte[] buffer)
        {
            //easy guide to edit arrays in csharp lol
            byte[] footerBuffer = buffer.Reverse().Take(20).Reverse().ToArray();

            byte[] DVPLTypeBytes = footerBuffer.Reverse().Take(4).Reverse().ToArray();
            string DVPLTypeCheck = Encoding.UTF8.GetString(DVPLTypeBytes);
            if (DVPLTypeCheck != "DVPL") throw new Exception("Invalid DVPL Footer");

            DVPLFooterData dataThatWereRead = new DVPLFooterData();
            
            dataThatWereRead.oSize = BitConverter.ToUInt32(footerBuffer, 0);
            dataThatWereRead.cSize = BitConverter.ToUInt32(footerBuffer, 4);
            dataThatWereRead.crc32 = BitConverter.ToUInt32(footerBuffer, 8);
            dataThatWereRead.type = BitConverter.ToUInt32(footerBuffer, 12);

            return dataThatWereRead;
        }

        /// <summary>
        /// Creates DVPL footer from given data.
        /// </summary>
        /// <param name="oS">File's original size</param>
        /// <param name="cS">File's compressed size (without footer)</param>
        /// <param name="crc">CRC32, computed from original file</param>
        /// <param name="typ">Type of used compression (".tex" files use 0, other - 2)</param>
        /// <returns>Array of bytes which is ready for concat with main array.</returns>
        public byte[] toDVPLFooter(uint oS, uint cS, uint crc, uint typ)
        {
            List<byte> result = new List<byte>();
            result.AddRange(BitConverter.GetBytes(oS));
            result.AddRange(BitConverter.GetBytes(cS));
            result.AddRange(BitConverter.GetBytes(crc));
            result.AddRange(BitConverter.GetBytes(typ));
            result.AddRange(Encoding.UTF8.GetBytes("DVPL"));

            return result.ToArray();
        }

        /// <summary>
        /// Decompresses given array of bytes. It's better to use it with try...catch, because there are a lot of things that can cause exceptions.
        /// </summary>
        /// <param name="buffer">Array of bytes to decompress (usually from file)</param>
        /// <returns>Decompressed array of bytes</returns>
        public byte[] decompressDVPL(byte[] buffer)
        {
            DVPLFooterData footerData = readDVPLFooter(buffer);
            byte[] targetBlock = buffer.Reverse().Skip(20).Reverse().ToArray();

            if (targetBlock.Length != footerData.cSize) throw new Exception("DVPL Size Mismatch");
            if (Crc32Algorithm.Compute(targetBlock) != footerData.crc32) throw new Exception("DVPL CRC32 Mismatch");

            if (footerData.type == 0)
            {
                if (!(footerData.oSize == footerData.cSize && footerData.type == 0))
                {
                    throw new Exception("DVPL Compression Type 0 Size Mismatch");
                }
                else
                {
                    return targetBlock;
                }
            }
            else if (footerData.type == 1 || footerData.type == 2)
            {
                byte[] deDVPLBlock = new byte[footerData.oSize];
                int i = LZ4Codec.Decode(targetBlock, deDVPLBlock);

                if (i == -1) throw new Exception("DVPL Decoded Size Mismatch");

                return deDVPLBlock;
            }
            else throw new Exception("Unknown Format");
        }

        /// <summary>
        /// Compresses given array of bytes. For .tex files it only adds DVPL-like footer, other files (or buffers) are compressed with L03_HC level.
        /// Use second variant of method, if you want to compress files, because, as I said, .tex files require different actions from others.
        /// </summary>
        /// <param name="buffer">Array of bytes to compress</param>
        /// <returns></returns>
        public byte[] compressDVPL(byte[] buffer)
        {
            byte[] compressedBlock = new byte[buffer.Length];
            int type = 2;
            int numberOfBytes = LZ4Codec.Encode(buffer, compressedBlock, LZ4Level.L03_HC);
            compressedBlock = compressedBlock.Reverse().Skip(compressedBlock.Length - numberOfBytes).Reverse().ToArray();

            byte[] footerForCompressed = toDVPLFooter((uint)buffer.Length, (uint)compressedBlock.Length, Crc32Algorithm.Compute(compressedBlock), (uint)type);

            byte[] readyBlock = compressedBlock.Concat(footerForCompressed).ToArray();

            return readyBlock;
        }

        /// <summary>
        /// Compresses given array of bytes. For .tex files it only adds DVPL-like footer, other files (or buffers) are compressed with L03_HC level.
        /// This variant is conceived to be used for files, because it also needs to read extension of file to compress properly.
        /// </summary>
        /// <param name="buffer">Array of bytes to compress</param>
        /// <param name="ext">Extension of file. Needed for proper compression of .tex files.</param>
        /// <returns></returns>
        public byte[] compressDVPL(byte[] buffer, string ext)
        {
            byte[] compressedBlock = new byte[buffer.Length];
            int type = 2;
            if (ext == ".tex")
            {
                LZ4Codec.Encode(buffer, compressedBlock, LZ4Level.L00_FAST);
                type = 0;
            } else
            {
                int numberOfBytes = LZ4Codec.Encode(buffer, compressedBlock, LZ4Level.L03_HC);
                compressedBlock = compressedBlock.Reverse().Skip(compressedBlock.Length - numberOfBytes).Reverse().ToArray();
            }

            byte[] footerForCompressed = toDVPLFooter((uint)buffer.Length, (uint)compressedBlock.Length, Crc32Algorithm.Compute(compressedBlock), (uint)type);

            byte[] readyBlock = compressedBlock.Concat(footerForCompressed).ToArray();

            return readyBlock;
        }
    }
}
