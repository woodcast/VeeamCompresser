using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WC.Util.Concurrent.ExecutorService;

namespace WC.Util.Concurrent.Executors
{
    public static class Executors
    {
        /// <summary>
        /// Creates a thread pool that reuses a fixed number of threads operating off 
        /// a shared unbounded queue. 
        /// </summary>
        /// <remarks>
        /// At any point, at most nThreads threads will be 
        /// active processing tasks. If additional tasks are submitted when all threads 
        /// are active, they will wait in the queue until a thread is available. 
        /// If any thread terminates due to a failure during execution prior to shutdown, 
        /// a new one will take its place if needed to execute subsequent tasks. 
        /// The threads in the pool will exist until it is explicitly shutdown.
        /// </remarks>
        /// <param name="nThreads">the number of threads in the pool</param>
        /// <returns>the newly created thread pool</returns>
        public static IExecutorService newFixedThreadPool(int nThreads)
        {
            IPriorityTaskQueueStrategy strategy =
                new OneNormalPerThreeHighMaybeLowStrategy();
            return new FixedThreadPoolService(strategy, nThreads);
        }
    }
}
