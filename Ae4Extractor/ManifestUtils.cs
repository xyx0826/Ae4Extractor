using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ae4Extractor
{
    class ManifestUtils
    {
        public static List<Ae4File> ParseManifest(byte[] manifest)
        {
            var files = new List<Ae4File>();
            using (var stream = new MemoryStream(manifest))
            {
                while (stream.Position < stream.Length)
                {
                    byte currentByte = 0xff;

                    // read file name bytes until terminator (0x00)
                    var nameBytes = new List<byte>();
                    while (currentByte != 0x00)
                    {
                        currentByte = Convert.ToByte(stream.ReadByte());
                        nameBytes.Add(currentByte);
                    }

                    nameBytes.RemoveAt(nameBytes.Count - 1);

                    stream.Position += 8;

                    // read file offset bytes (8 bytes)
                    var offsetBytes = new byte[8];
                    for (int i = 0; i < 8; i++)
                    {
                        currentByte = Convert.ToByte(stream.ReadByte());
                        offsetBytes[i] = currentByte;
                    }

                    // read file size bytes (larger/unknown) (8 bytes)
                    var sizeUnknownBytes = new byte[8];
                    for (int i = 0; i < 8; i++)
                    {
                        currentByte = Convert.ToByte(stream.ReadByte());
                        sizeUnknownBytes[i] = currentByte;
                    }

                    // read file size bytes (smaller/valid) (8 bytes)
                    var sizeBytes = new byte[8];
                    for (int i = 0; i < 8; i++)
                    {
                        currentByte = Convert.ToByte(stream.ReadByte());
                        sizeBytes[i] = currentByte;
                    }

                    stream.Position += 2;
                    files.Add(new Ae4File(Encoding.UTF8.GetString(nameBytes.ToArray()),
                        BitConverter.ToInt32(offsetBytes, 0),
                        BitConverter.ToInt32(sizeUnknownBytes, 0),
                        BitConverter.ToInt32(sizeBytes, 0)));
                }
                return files;
            }
        }
    }
}
