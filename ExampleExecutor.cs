using FixedThreadPoolApplication.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WC.Util.Concurrent.Executors;
using WC.Util.Concurrent.ExecutorService;

namespace FixedThreadPoolApplication
{
    class Program
    {
        static void Main(string[] args)
        {
            int nThreads = 3;
            int nTasks = 20;
            
            Console.WriteLine(
                "Starting Fixed Thread Pool of {0} threads and {1} tasks", 
                nThreads, nTasks);
            
            IExecutorService executor = Executors.newFixedThreadPool(nThreads);

            new Thread((o) =>
            {

                for (int i = 0; i < nTasks / 2; i++)
                {
                    Log.Info("loop1 is trying to put task{0} into queue", i);
                    executor.Execute(new SimpleTask("task" + i), Priority.NORMAL);
                    Thread.Sleep(TimeSpan.FromSeconds(5)); // не быстрее
                }
            }).Start();

            new Thread((o) =>
            {

                for (int i = nTasks / 2; i < nTasks; i++)
                {
                    Log.Info("loop2 is trying to put task{0} into queue", i);
                    executor.Execute(new SimpleTask("task" + i), Priority.NORMAL);
                    Thread.Sleep(TimeSpan.FromSeconds(5)); // не быстрее
                }
            }).Start();

            Thread.Sleep(10000);
            executor.Stop();

            Console.Write("enter any key...");
            Console.ReadLine();
        }
    }
}
