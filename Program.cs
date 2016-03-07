using com.veeam.Compresser.FileMapping;
using com.veeam.Compresser.Tests;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;


namespace com.veeam.Compresser
{
    class Program
    {
        static void Main(string[] args)
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

            using(var fileMapping = FileMappingWrapper.CreateFromFile(path))
            using(var compressedFile = File.Create(new FileInfo(path).FullName + ".gz"))
            using(var compressor = new GZipStream(compressedFile, CompressionMode.Compress))
            {
                long offset = 0;
                long length = new FileInfo(path).Length;
                int bytesInBlock = (int)info.AllocationGranularity;

                while (length > 0)
                {
                    if (length < bytesInBlock)
                        bytesInBlock = (int)length;

                    using (var accessor = fileMapping.CreateViewAccessor(offset, bytesInBlock))
                    {

                        byte[] block = new byte[bytesInBlock];
                        accessor.ReadBytes(0, block, bytesInBlock);
                        compressor.Write(block, 0, bytesInBlock);
                    }

                    WriteMessageToUser("offset: {0}, length: {1}", offset, length);

                    offset += bytesInBlock;
                    length -= bytesInBlock;
                }
            }

            WriteMessageToUser("{0} completed", args[0]);
            Console.ReadLine();
        }

        private static void WriteMessageToUser(string message, params object[] para)
        {
            Console.WriteLine(message, para);
        }
    }
}
