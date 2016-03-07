using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WC.Util.Concurrent.ExecutorService;

namespace FixedThreadPoolApplication
{
    public class SimpleTask : WCTask
    {
        private string name;

        public SimpleTask(string name)
        {
            this.name = name;
        }

        public override void Execute()
        {
            System.Console.WriteLine("task {0} executed", this.name);
        }

        public override string ToString()
        {
            return this.name;
        }
    }
}
