using System;
using System.IO;
using System.Reflection;

namespace Ae4Extractor
{
    internal class Program
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
                Console.ReadKey();
                return;
            }

            var mf = Manifest.GetDecompressedMf(args[0]);
            Console.WriteLine($"Parsing {mf.Length} bytes of manifest...");
            var fileList = Manifest.ParseManifest(mf);
            Console.WriteLine($"Writing {fileList.Count} files...");
            Extraction.WriteFiles(args[0], fileList);
            Console.WriteLine("Extraction complete. Press any key to exit.");
            Console.ReadKey();
        }
    }
}
