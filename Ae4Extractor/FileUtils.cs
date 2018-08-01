using System;
using System.Collections.Generic;
using System.IO;

namespace Ae4Extractor
{
    class FileUtils
    {
        /// <summary>
        /// Writes file in an archive file according to its manifest.
        /// </summary>
        /// <param name="path">Path to the archive file.</param>
        /// <param name="files">File entries parsed from the raw manifest.</param>
        public static void WriteFiles(string path, List<Ae4File> files)
        {
            using (var stream = new FileStream(path, FileMode.Open))
            {
                foreach (var file in files)
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(file.Name));
                    stream.Position = file.Offset;
                    using (var newFile = new FileStream(file.Name, FileMode.Create))
                    {
                        var fileBytes = new byte[file.Size];
                        stream.Read(fileBytes, 0, file.Size);
                        newFile.Write(fileBytes, 0, file.Size);
                    }
                    Console.WriteLine($"Written {file.Name}: {file.Size} bytes");
                }
            }
        }
    }
}
