using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace com.veeam.Compresser
{
    public sealed class DestinationWriter : IDisposable
    {
        FileStream compressedFile;

        internal DestinationWriter(string path)
        {
            this.Path = path;
            compressedFile = File.Create(new FileInfo(path).FullName + ".gz");
        }

        public string Path { get; set; }

        public void WriteNext(Block block)
        {
            compressedFile.Write(block.Data, 0, block.Data.Length);
        }

        public void Dispose()
        {
            if (compressedFile != null)
                compressedFile.Dispose();
        }
    }
}
