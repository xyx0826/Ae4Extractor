using System;
using System.Collections.Generic;
using System.IO;

namespace Ae4Extractor
{
    /// <summary>
    /// Handles writing extracted files.
    /// </summary>
    internal static class FileUtils
    {
        /// <summary>
        /// Writes file in an archive file according to its manifest.
        /// </summary>
        /// <param name="path">Path to the archive file.</param>
        /// <param name="files">File entries parsed from the raw manifest.</param>
        public static void WriteFiles(string path, IEnumerable<Ae4File> files)
        {
            var createdFolders = new HashSet<string>();

            using (var stream = new FileStream(path, FileMode.Open))
            {
                foreach (var file in files)
                {
                    // Ensure folder exists
                    var dir = Path.GetDirectoryName(file.Name);
                    if (!createdFolders.Contains(dir) && !String.IsNullOrEmpty(dir))
                    {
                        Directory.CreateDirectory(dir);
                        createdFolders.Add(dir);
                    }

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
