using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace com.veeam.Compresser
{
    public class Block
    {
        public int Id { get; set; }
        public long Offset { get; set; }
        public byte[] Data { get; set; }
    }
}
