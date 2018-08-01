using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ae4Extractor
{
    class FileUtils
    {
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
