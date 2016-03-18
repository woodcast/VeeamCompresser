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

        public override string ToString()
        {
            return new StringBuilder(string.Format("[ID={0} Offset={1} Data[0]={2}", Id, Offset, Data[0])).ToString();
        }
    }
}
