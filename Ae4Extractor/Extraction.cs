﻿using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using ZstdNet;

namespace Ae4Extractor
{
    /// <summary>
    /// Handles writing extracted files.
    /// </summary>
    internal static class Extraction
    {
        /// <summary>
        /// Writes file in an archive file according to its manifest.
        /// </summary>
        /// <param name="path">Path to the archive file.</param>
        /// <param name="files">File entries parsed from the raw manifest.</param>
        public static void WriteFiles(string path, IEnumerable<TinFile> files)
        {
            var createdFolders = new HashSet<string>();

            using (var stream = new FileStream(path, FileMode.Open))
            {
                foreach (var file in files)
                {
                    // Ensure folder exists
                    var dir = Path.GetDirectoryName(file.Path);
                    if (!createdFolders.Contains(dir) && !String.IsNullOrEmpty(dir))
                    {
                        Directory.CreateDirectory(dir);
                        createdFolders.Add(dir);
                    }

                    // Read raw (or compressed) data from Tin
                    stream.Position = (long) file.Offset;
                    var data = new byte[file.CompressedSize];
                    stream.Read(data, 0, data.Length);

                    // Switch on compression type
                    using (var newFile = new FileStream(file.Path, FileMode.Create))
                    {
                        switch (file.ReadAccessType)
                        {
                            case TinReadAccessType.RawReadAccess:
                                // Read the raw bytes
                                newFile.Write(data, 0, data.Length);
                                break;
                            case TinReadAccessType.ZLibReadAccess:
                                if (data.Length < 2)
                                {
                                    throw new InvalidDataException("Compressed data is too short for ZLibReadAccess.");
                                }

                                // Skip 2-byte zlib header when decompressing
                                using (var dataStream = new MemoryStream(data, 2, data.Length - 2))
                                using (var zlib = new DeflateStream(dataStream, CompressionMode.Decompress))
                                {
                                    zlib.CopyTo(newFile);
                                }
                                break;
                            case TinReadAccessType.ZStdReadAccess:
                                // Wrap stream in a DecompressStream
                                using (var zstd = new Decompressor())
                                {
                                    var raw = zstd.Unwrap(data, (int) file.RawSize);
                                    newFile.Write(raw, 0, raw.Length);
                                }
                                break;
                                    default:
                                throw new ArgumentOutOfRangeException(
                                    $"Specified compression {file.ReadAccessType} is not yet implemented.");
                        }
                    }
                    Console.WriteLine($"Written {file.Path}: {file.RawSize} bytes, {file.ReadAccessType}");
                }
            }
        }
    }
}
