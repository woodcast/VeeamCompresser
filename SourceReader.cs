using com.veeam.Compresser.FileMapping;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace com.veeam.Compresser
{
    public sealed class SourceReader : IDisposable
    {
        FileMappingWrapper fileMapping;

        internal SourceReader(string path, int granularity)
        {
            this.Path = path;
            this.Granularity = granularity;

            // можно использовать лэйзи.
            fileMapping = FileMappingWrapper.CreateFromFile(Path);

            offset = 0;
            length = new FileInfo(Path).Length;
            bytesInBlock = Granularity;
        }

        long offset = 0;
        long length;
        int bytesInBlock;

        public Block ReadNext()
        {
            if (length > 0)
            {
                if (length < bytesInBlock)
                    bytesInBlock = (int)length;

                byte[] block = new byte[bytesInBlock];
                using (var accessor = fileMapping.CreateViewAccessor(offset, bytesInBlock))
                {
                    accessor.ReadBytes(0, block, bytesInBlock);
                }

                //WriteMessageToUser("offset: {0}, length: {1}", offset, length);

                offset += bytesInBlock;
                length -= bytesInBlock;

                return new Block { Id = 0, Offset = offset, Data = block };
            }
            else
                throw new Exception("The end");
        }

        public string Path { get; set; }

        public int Granularity { get; set; }

        public void Dispose()
        {
            if (fileMapping != null)
            {
                fileMapping.Dispose();
            }
        }

        internal bool HasNext()
        {
            return length > 0;
        }
    }
}
