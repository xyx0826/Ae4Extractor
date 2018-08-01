using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace Ae4Extractor
{
    class ByteUtils
    {
        private static readonly int _maxSearchedBytes = 8192000;
        private static readonly int _searchSteppingBytes = 512;

        private static readonly byte _zlibCmf = 0x78;
        private static readonly byte _zlibFlg = 0xda;

        /// <summary>
        /// Retrives file manifest from the end of the given archive file.
        /// </summary>
        /// <param name="file">Archive file to be analysed.</param>
        /// <returns>Byte array of decompressed file manifest.</returns>
        public static byte[] GetDecompressedMf(string file)
        {
            byte[] compressedMf = new byte[0];
            byte[] decompressedMf;
            do
            {
                Console.WriteLine("Finding valid zlib header. " +
                    $"Last position: -{compressedMf.Length}");

                compressedMf = GetCompressedMf(file, compressedMf.Length);
                decompressedMf = InflateData(compressedMf);
            }
            while (decompressedMf == null);
            return decompressedMf;
        }

        /// <summary>
        /// Searches the file for a compressed file manifest.
        /// </summary>
        /// <param name="file">The file to be searched.</param>
        /// <param name="searchFrom">The position that zlib headers before it shall be omitted.</param>
        /// <returns>Byte array of compressed file manifest.</returns>
        static byte[] GetCompressedMf(string file, int searchFrom = 0)
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
                    if (currentOffset < searchFrom + _searchSteppingBytes || searchResult == -1)
                    {
                        // header not found, assume it is part of zlibbed data
                        // or it can be intentionally omitted by searchFrom
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

        /// <summary>
        /// Attempts to inflate (decompress) given byte data.
        /// </summary>
        /// <param name="deflatedData">Byte array of data compressed in Deflate mode.</param>
        /// <returns>Byte array of decompressed data.</returns>
        static byte[] InflateData(byte[] deflatedData)
        {
            using (DeflateStream stream =
                new DeflateStream(new MemoryStream(deflatedData), CompressionMode.Decompress))
            {
                using (MemoryStream outStream = new MemoryStream())
                {
                    try
                    {
                        stream.CopyTo(outStream);
                        return outStream.ToArray();
                    }
                    catch { return null; }
                }
            }
        }

        /// <summary>
        /// Checks the given byte array for zlib header presence.
        /// </summary>
        /// <param name="data">Data to be checked.</param>
        /// <returns>Position of the found zlib header, or -1 if not found.</returns>
        static int CheckForZlibHeader(byte[] data)
        {
            for (int i = 0; i < data.Length - 1; i ++)
            {
                if (data[i].Equals(_zlibCmf) && data[i + 1].Equals(_zlibFlg))
                    return i;
            }
            return -1;
        }
    }
}
