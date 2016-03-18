using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using com.veeam.Compresser.FileMapping;
using NLog;

namespace com.veeam.Compresser
{
    public sealed class DestinationWriter : IDisposable
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

//        readonly FileStream _compressedFile;
        private FileMappingWrapper _file;
        private FileMappingViewAccessor _accessor;

        private long _bytesInBlock = 0;

        internal DestinationWriter(string path, int granularity)
        {
            Path = new FileInfo(path).FullName + ".gz";
            Granularity = granularity;
//            _compressedFile = File.Create(new FileInfo(path).FullName + ".gz");
        }

        public bool OpenMappingFile(long length, long bytesInBlock)
        {
            Length = length;
            _bytesInBlock = bytesInBlock;

            using (var file = File.Open(Path, FileMode.Create, FileAccess.ReadWrite))
            {
                file.SetLength(Length);
//                file.Flush();

                FileMappingAccess rights = new FileMappingAccess();
                FileMappingSecurity security = new FileMappingSecurity();

                _file = FileMappingWrapper.CreateFromFile(
                    file,
                    Guid.NewGuid().ToString(),
                    Length,
                    rights,
                    security,
                    HandleInheritability.Inheritable,
                    true // TODO: уточнить
                    );
            }

            // _accessor never can be null
            _accessor = _file.CreateViewAccessor(0, Length < Granularity ? Length : Granularity);
            Debug.Assert(_accessor != null, "_accessor != null");

            return true;
        }

        public long Granularity { get; set; }

        public string Path { get; set; }

        public long Length { get; set; }

        public void WriteNext(Block block)
        {
            if (block.Offset < _accessor.PointerOffset || block.Offset > _accessor.PointerOffset + Granularity)
            {
                _accessor.Dispose();
                _accessor = _file.CreateViewAccessor(block.Offset, Granularity);
            }

            Logger.Info("write block {0}", block.ToString());

            long offset = block.Offset < Granularity ? block.Offset : block.Offset % Granularity;
            _accessor.WriteBytes(offset, block.Data, block.Data.Length);

            //            _compressedFile.Write(block.Data, 0, block.Data.Length);               
            //WriteMessageToUser("offset: {0}, length: {1}", offset, length);
        }

        public void Dispose()
        {

            // усечь файл
            _file.

//            if (_compressedFile != null)
//                _compressedFile.Dispose();
            if (_file != null)
                _file.Dispose();

            if (_accessor != null)
                _accessor.Dispose();
        }
    }
}
