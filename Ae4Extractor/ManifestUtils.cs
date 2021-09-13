using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Ae4Extractor
{
    internal static class ManifestUtils
    {
        /// <summary>
        /// Parses an archive manifest into a list of file attributes.
        /// </summary>
        /// <param name="manifest">Byte array of raw manifest.</param>
        /// <returns>List of parsed file attributes.</returns>
        public static List<TinFileInfo> ParseManifest(byte[] manifest)
        {
            var sb = new StringBuilder(64);
            var buf = new byte[8];
            var val = new ulong[3];

            var files = new List<TinFileInfo>();

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

                    // Length of all TinFileInfo fields
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
                    files.Add(new TinFileInfo(
                        path, val[0], val[1], val[2], readAccessType));
                }
                return files;
            }
        }
    }
}
