using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ae4Extractor
{
    class ByteUtils
    {
        private static readonly int _maxSearchedBytes = 8192000;
        private static readonly int _searchSteppingBytes = 512;

        private static readonly byte _zlibCmf = 0x78;
        private static readonly byte _zlibFlg = 0xda;


        public static byte[] GetCompressedMf(string file)
        {
            var compressedBytes = new List<byte>(_maxSearchedBytes / 2);
            var hasFound = false;

            using (var stream = new FileStream(file, FileMode.Open, FileAccess.Read))
            {
                for (int currentOffset = _searchSteppingBytes; 
                    currentOffset <= _maxSearchedBytes; 
                    currentOffset += _searchSteppingBytes)
                {
                    stream.Seek(-currentOffset, SeekOrigin.End);
                    var bytesToSearch = new byte[_searchSteppingBytes];
                    stream.Read(bytesToSearch, 0, _searchSteppingBytes);

                    var searchResult = CheckForZlibHeader(bytesToSearch);
                    if (searchResult == -1)
                    {
                        // header not found, assume it is part of zlibbed data
                        compressedBytes.InsertRange(0, bytesToSearch);
                    }
                    else
                    {
                        // header found, write data behind header position
                        hasFound = true;
                        compressedBytes.InsertRange(0, bytesToSearch.Skip(searchResult));
                        break;
                    }
                }
                
                // DeflateStream does not want 2-byte zlib header
                if (hasFound) return compressedBytes.Skip(2).ToArray();
                else return null;
            }
        }

        public static byte[] InflateData(byte[] deflatedData)
        {
            using (DeflateStream stream =
                new DeflateStream(new MemoryStream(deflatedData), CompressionMode.Decompress))
            {
                using (MemoryStream outStream = new MemoryStream())
                {
                    stream.CopyTo(outStream);
                    return outStream.ToArray();
                }
            }
        }

        public static int CheckForZlibHeader(byte[] data)
        {
            for (int i = 0; i < data.Length - 2; i ++)
            {
                if (data[i].Equals(_zlibCmf) && data[i + 1].Equals(_zlibFlg))
                    return i;
            }
            return -1;
        }

        public static void WriteDebug(string log)
        {
            if (Debugger.IsAttached) Console.WriteLine(log);
        }
    }
}
