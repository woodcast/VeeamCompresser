using com.veeam.Compresser.FileMapping;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NLog;

namespace com.veeam.Compresser
{
    public sealed class SourceReader : IDisposable
    {
        readonly FileMappingWrapper _fileMapping;

        long _offset = 0;
        long _length;
        int _bytesInBlock;
        private int _currentId = -1; // starts from zero

        internal SourceReader(string path, int granularity)
        {
            this.Path = path;
            this.Granularity = granularity;

            // можно использовать лэйзи.
            _fileMapping = FileMappingWrapper.CreateFromFile(Path);

            _offset = 0;
            _length = new FileInfo(Path).Length;
            _bytesInBlock = Granularity;
        }


        public Block NextBlock()
        {
            if (_length > 0)
            {
                if (_length < _bytesInBlock)
                    _bytesInBlock = (int)_length;

                byte[] block = new byte[_bytesInBlock];

                using (var accessor = _fileMapping.CreateViewAccessor(_offset, _bytesInBlock))
                {
                    accessor.ReadBytes(0, block, _bytesInBlock);
                }

                //WriteMessageToUser("offset: {0}, length: {1}", offset, length);

                _currentId += 1;
                _offset += _bytesInBlock;
                _length -= _bytesInBlock;

                return new Block { Id = _currentId, Offset = _offset, Data = block };
            }
            else
                throw new Exception("The end");
        }

        public string Path { get; set; }

        public int Granularity { get; set; }

        public void Dispose()
        {
            if (_fileMapping != null)
            {
                _fileMapping.Dispose();
            }
        }

        internal bool HasNext()
        {
            return _length > 0;
        }
    }
}
