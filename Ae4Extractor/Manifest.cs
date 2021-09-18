using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace Ae4Extractor
{
    /// <summary>
    /// Extracts and reads the manifest from a Tin file.
    /// </summary>
    internal static class Manifest
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

        /// <summary>
        /// Parses an archive manifest into a list of file attributes.
        /// </summary>
        /// <param name="manifest">Byte array of raw manifest.</param>
        /// <returns>List of parsed file attributes.</returns>
        public static List<TinFile> ParseManifest(byte[] manifest)
        {
            var sb = new StringBuilder(64);
            var buf = new byte[8];
            var val = new ulong[3];

            var files = new List<TinFile>();

            using (var stream = new MemoryStream(manifest))
            {
                while (stream.Position < stream.Length)
                {
                    // Read path
                    var c = (char) stream.ReadByte();
                    while (c > 0)
                    {
                        sb.Append(c);
                        c = (char) stream.ReadByte();
                    }

                    var path = sb.ToString();
                    sb.Clear();

                    // Length of all TinFile fields
                    stream.Position += 8;

                    // File offset, raw length, compressed length
                    for (var i = 0; i < val.Length; i++)
                    {
                        stream.Read(buf, 0, buf.Length);
                        val[i] = BitConverter.ToUInt64(buf, 0);
                    }

                    // Compression type
                    stream.Read(buf, 0, 2);
                    var readAccessType = (TinReadAccessType) (buf[0] | buf[1] << 8);
                    files.Add(new TinFile(
                        path, val[0], val[1], val[2], readAccessType));
                }
                return files;
            }
        }
    }
}
