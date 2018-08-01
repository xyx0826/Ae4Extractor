﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Ae4Extractor
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Ae4Extractor - DS Fishlabs Abyss Engine 4 " +
                "Asset Extractor - " +
                Assembly.GetExecutingAssembly().GetName().Version);

            if (args.Length < 1 || !File.Exists(args[0]))
            {
                Console.WriteLine("Usage: Ae4Extractor.exe [file-to-be-extracted]");
                Console.WriteLine("ERROR: File not specified or does not exist. " +
                    "Press any key to exit.");
                Console.Read();
                return;
            }

            Console.WriteLine("Searching for manifest...");
            var compressedManifest = ByteUtils.GetCompressedMf(args[0]);
            Console.WriteLine("Compressed manifest found. " +
                $"Size: {compressedManifest.Length} bytes");
            var decompressedManifest = ByteUtils.InflateData(compressedManifest);
            Console.WriteLine("Decompressed manifest size: " +
                $"{decompressedManifest.Length} bytes");
            Console.WriteLine("Parsing manifest...");
            var fileList = ManifestUtils.ParseManifest(decompressedManifest);
            Console.WriteLine($"Manifest contains {fileList.Count} files. Writing files...");
            FileUtils.WriteFiles(args[0], fileList);
            Console.WriteLine("Extraction complete. Press any key to exit.");
            Console.Read();
        }
    }
}
