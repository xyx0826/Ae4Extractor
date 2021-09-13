using System;
using System.IO;
using System.IO.Compression;

namespace Ae4Extractor
{
    /// <summary>
    /// Searches and extracts the manifest from a data container.
    /// </summary>
    internal static class ByteUtils
    {
        /// <summary>
        /// The offset of the manifest offset field, from the end of file.
        /// </summary>
        private const long OffsetFromEnd = -18;

        /// <summary>
        /// Retrives file manifest from the end of the given archive file.
        /// </summary>
        /// <param name="file">Archive file to be analysed.</param>
        /// <returns>Byte array of decompressed file manifest.</returns>
        public static byte[] GetDecompressedMf(string file)
        {
            var compressedMf = GetCompressedMf(file);
            return InflateData(compressedMf);
        }

        /// <summary>
        /// Searches the file for a compressed file manifest.
        /// </summary>
        /// <param name="file">The file to be searched.</param>
        /// <returns>Byte array of compressed file manifest.</returns>
        private static byte[] GetCompressedMf(string file)
        {
            var temp = new byte[8];
            using (var stream = new FileStream(file, FileMode.Open, FileAccess.Read))
            {
                // Read manifest offset from tail of file
                stream.Seek(OffsetFromEnd, SeekOrigin.End);
                stream.Read(temp, 0, temp.Length);
                // The next 8 bytes is the compressed manifest length, but we don't care
                // Skip 2-byte zlib header
                var mfOffset = BitConverter.ToInt64(temp, 0) + 2;

                // Compute read length and read the blob
                var mfLength = stream.Length - mfOffset - OffsetFromEnd;
                var mf = new byte[mfLength];
                stream.Seek(mfOffset, SeekOrigin.Begin);
                stream.Read(mf, 0, mf.Length);
                return mf;
            }
        }

        /// <summary>
        /// Attempts to inflate (decompress) given byte data.
        /// </summary>
        /// <param name="deflatedData">Byte array of data compressed in Deflate mode.</param>
        /// <returns>Byte array of decompressed data.</returns>
        private static byte[] InflateData(byte[] deflatedData)
        {
            using (var stream =
                new DeflateStream(new MemoryStream(deflatedData), CompressionMode.Decompress))
            {
                using (var outStream = new MemoryStream())
                {
                    try
                    {
                        stream.CopyTo(outStream);
                        return outStream.ToArray();
                    }
                    catch
                    {
                        return null;
                    }
                }
            }
        }
    }
}