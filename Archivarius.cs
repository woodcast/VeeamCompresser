using com.veeam.Compresser.FileMapping;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

namespace com.veeam.Compresser
{
    public class Archivarius
    {
        SourceReader reader;
        DestinationWriter writer;

        Queue<Block> preparedBlocks = new Queue<Block>();
        Queue<Block> compressedBlocks = new Queue<Block>();

        public Archivarius(SourceReader reader, DestinationWriter writer)
        {
            this.reader = reader;
            this.writer = writer;
        }

        /// <summary>
        /// Паттерн шаблонный метод (задает алгоритм).
        /// </summary>
        public void Run()
        {
            while (reader.HasNext())
            {
                Block block = reader.ReadNext();
                preparedBlocks.Enqueue(block);
            }

            while (preparedBlocks.Count > 0)
            {
                Block block = preparedBlocks.Dequeue();
                using (var compressedBlock = new MemoryStream())
                using (var compressor = new GZipStream(compressedBlock, CompressionMode.Compress))
                {
                    compressor.Write(block.Data, 0, block.Data.Length);
                    compressedBlocks.Enqueue(
                        new Block
                        {
                            Id = block.Id,
                            Offset = block.Offset,                            
                            Data = compressedBlock.ToArray()
                        });
                }
            }

            while (compressedBlocks.Count > 0)
            {
                Block block = compressedBlocks.Dequeue();
                writer.WriteNext(block);
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
            DestinationWriter writer = new DestinationWriter(path);
            Archivarius archivarius = new Archivarius(reader, writer);
            archivarius.Run();
        }

        private static void WriteMessageToUser(string message, params object[] para)
        {
            Console.WriteLine(message, para);
        }
    }
}
