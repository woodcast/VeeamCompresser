using com.veeam.Compresser.FileMapping;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace com.veeam.Compresser
{
    class ExampleAccessorReadByte
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

            using (var fileMapping = FileMappingWrapper.CreateFromFile(path))
            {
                long offset = 0;
                long length = new FileInfo(path).Length;
                long bytesInBlock = info.AllocationGranularity;

                while (length > 0)
                {
                    if (length < bytesInBlock)
                        bytesInBlock = length;

                    using (var accessor = fileMapping.CreateViewAccessor(offset, bytesInBlock))
                    {
                        int size = Marshal.SizeOf(typeof(byte));

                        for (long i = 0; i < bytesInBlock; i += size)
                        {
                            byte value = accessor.ReadByte(i);
                            if (value == 0xF)
                                value -= 0x1;
                            else if (value == 0xA)
                                value += 0x1;

                            //accessor.WriteByte(i, value);
                        }
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
