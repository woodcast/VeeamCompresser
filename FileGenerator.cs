using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace com.veeam.Compresser
{
    class FileGenerator
    {
        private static void Main(string[] args)
        {
            string path = args[1]; // source file;

            FileStream fs = File.OpenWrite(path);

            using (var writer = new BinaryWriter(fs))
            {
                byte[] arr = new byte[16] 
                {
                    0xF, 0xA, 0xF, 0xA, 0xF, 0xA, 0xF, 0xA,
                    0xF, 0xA, 0xF, 0xA, 0xF, 0xA, 0xF, 0xD,
                };

                for (int i = 0; i < 1024 * 1024 * 5 + 16; i += 16)
                {
                    writer.Write(arr);
                }
            }
        }
    }

}
