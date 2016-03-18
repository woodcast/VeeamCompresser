using com.veeam.Compresser.FileMapping;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Threading;
using FixedThreadPoolApplication.Util;
using NLog;

namespace com.veeam.Compresser
{
    public class Archivarius
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        readonly SourceReader _reader;
        readonly DestinationWriter _writer;

        Thread[] _threads;
        readonly int _nThreads;

        readonly object _syncRead = new object();
        readonly object _syncWrite = new object();
        private int _granularity;
        private long _compressedBlockLength;

        public Archivarius(SourceReader reader, DestinationWriter writer, int nThreads, int allocationGranularity)
        {

            if (nThreads <= 0)
                throw new ArgumentOutOfRangeException("nThreads",
                    "nThreads must be in range [1; MAX_NUMBER_OF_THREADS]");

            _reader = reader;
            _writer = writer;

            _nThreads = nThreads;
            _granularity = allocationGranularity;
        }


        private void RunTask()
        {

            // ReSharper disable once InconsistentlySynchronizedField
            while (_reader.HasNext())
            {
                Block block;

                lock (_syncRead)
                {
                    if (!_reader.HasNext())
                        return;

                    block = _reader.NextBlock();
                }

                Logger.Info("[{1}] block readed: {0}", block.ToString(), Thread.CurrentThread.Name);
//                WriteMessageToUser("block readed: [{1}] {0}", block.ToString(), Thread.CurrentThread.Name);

                Block blockToWrite = doOperation(block);

                Thread.Sleep(200);

                lock (_syncWrite)
                {
                    _writer.WriteNext(blockToWrite);
                }

                Logger.Info("[{1}] block written: {0}", block.ToString(), Thread.CurrentThread.Name);
//                WriteMessageToUser("block written [{1}]: {0}", block.ToString(), Thread.CurrentThread.Name);

            }
        }

        private Block doOperation(Block block)
        {
            Block blockToWrite;
            using (var compressedBlock = new MemoryStream())
            using (var compressor = new GZipStream(compressedBlock, CompressionMode.Compress))
            {
                compressor.Write(block.Data, 0, block.Data.Length);
                byte[] tempBytes = compressedBlock.ToArray();

                blockToWrite = new Block
                {
                    Id = block.Id,
                    Offset = block.Id * _compressedBlockLength,
                    Data = tempBytes
                };
            }
            return blockToWrite;
        }

        public void Run()
        {
            FileInfo fileInfo = new FileInfo(_reader.Path);
            Block block = _reader.NextBlock();
            Block compressedblock = doOperation(block);          

            long destFileSize = fileInfo.Length/_granularity*compressedblock.Data.Length;
            _compressedBlockLength = compressedblock.Data.Length;
            
            Logger.Info("Source file size is {0}", fileInfo.Length);
            Logger.Info("Destination file size is {0}", destFileSize);
            Logger.Info("Number of blocks is {0} with granularity={1}", fileInfo.Length / _granularity, _granularity);
            Logger.Info("Size of compressed block is {0}", compressedblock.Data.Length);

            _writer.OpenMappingFile(destFileSize, _compressedBlockLength);
            _writer.WriteNext(compressedblock);


            _threads = new Thread[_nThreads];
            for (int i = 0; i < _nThreads; i++)
            {
                Thread t = new Thread(RunTask) {Name = "thread_" + i + 1};
                Logger.Info("starting thread {0}", t.Name);
                _threads[i] = t;
                t.Start();
            }

        }



        public static void Main(string[] args)
        {            
            string path = args[1]; // source file;

            if (!File.Exists(path))
            {
                WriteMessageToUser("Can't find source file [{0}]", path);
            }

            Win32.SystemInfo info;
            Win32.GetSystemInfo(out info);

            WriteMessageToUser("NumberOfProcessors: {0}", info.NumberOfProcessors);
            WriteMessageToUser("AllocationGranularity: {0}", info.AllocationGranularity);
            WriteMessageToUser("PageSize: {0}", info.PageSize);
            WriteMessageToUser("ProcessorArchitecture: {0}", info.ProcessorArchitecture);

            SourceReader reader = new SourceReader(path, (int)info.AllocationGranularity * 2);
            DestinationWriter writer = new DestinationWriter(path, (int)info.AllocationGranularity);
            Archivarius archivarius = new Archivarius(reader, writer, (int)info.NumberOfProcessors, (int)info.AllocationGranularity);
            archivarius.Run();

            Console.ReadLine();
        }

        private static void WriteMessageToUser(string message, params object[] para)
        {
            Console.WriteLine(message, para);
        }
    }
}
