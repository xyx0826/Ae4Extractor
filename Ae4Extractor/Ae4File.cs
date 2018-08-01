using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ae4Extractor
{
    class Ae4File
    {
        public Ae4File(string name, int offset, int size, int sizeUnknown)
        {
            Name = name;
            Offset = offset;
            Size = size;
            SizeUnknown = sizeUnknown;
        }

        public string Name { get; set; }
        public int Offset { get; set; }
        public int Size { get; set; }
        public int SizeUnknown { get; set; }
    }
}
