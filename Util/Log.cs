using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FixedThreadPoolApplication.Util
{
    internal static class Log
    {
        internal static void Info(string s, params object[] arg)
        {
            Console.WriteLine(s, arg);
        }

        internal static void Error(Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
    }
}
