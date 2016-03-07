using FixedThreadPoolApplication.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using WC.Util.Concurrent.Executors;

namespace WC.Util.Concurrent.ExecutorService
{
    public class FixedThreadPoolService : IExecutorService
    {
        /// <summary>
        /// if stop is true - prevent adding tasks to task queue.
        /// </summary>
        private volatile bool stop = false;

        /// <summary>
        /// maximum number of threads in pool.
        /// </summary>
        private readonly int maxThreads;

        /// <summary>
        /// task queue.
        /// </summary>
        private readonly PriorityTaskQueue taskQueue;

        /// <summary>
        /// holds mutex == handlers[0] and semaphore == handlers[1]
        /// </summary>
        private readonly WaitHandle[] handlers;

        /// <summary>
        /// Извлечение из очереди возможно только если там что-нибудь есть.
        /// В этом помогает семафор.
        /// </summary>
        private readonly Semaphore semaphore;

        /// <summary>
        /// только один поток может добавлять или извлекать из <see cref="taskQueue"/>
        /// </summary>
        private readonly Mutex mutex;

        /// <summary>
        /// Возможно, что он и не нужен, но я пока не знаю как еще можно
        /// дождаться завершения работы всех потоков.
        /// </summary>
        private readonly Thread[] pool;


        /// <summary>
        /// Creates FixedThreadPoolService
        /// Throws ArgumentOutOfRangeException if nThreads <code><= 0</code>
        /// Очередь может держать не более int.MaxValue задач.
        /// </summary>
        /// <param name="nThreads">Number of threads should be in range [1; MAX_NUMBER_OF_THREADS]</param>
        /// <param name="poolManager">Manages FixedThreadPool</param>
        public FixedThreadPoolService(
            IPriorityTaskQueueStrategy strategy, 
            int nThreads)
        {
            if (nThreads <= 0)
                throw new ArgumentOutOfRangeException("nThreads", 
                    "nThreads must be in range [1; MAX_NUMBER_OF_THREADS]");

            this.maxThreads = nThreads;
            
            this.handlers = new WaitHandle[2];
            this.mutex = new Mutex(false);
            this.handlers[0] = this.mutex;
            this.semaphore = new Semaphore(0, int.MaxValue);
            this.handlers[1] = this.semaphore;
            
            this.taskQueue = new PriorityTaskQueue(strategy);
            this.pool = new Thread[nThreads];
            
            for (int i = 0; i < nThreads; i++)
            {
                Thread t = new Thread(this.RunTask);
                t.Name = "thread" + i;
                Log.Info("starting thread {0}", t.Name);
                this.pool[i] = t;
                t.Start();                    
            }
        }

        /// <summary>
        /// Ставит task в очередь task queue на выполнение.
        /// Throws ArgumentNullException.
        /// </summary>
        /// <param name="task">Task to execute</param>
        /// <param name="priority">Task prioriry</param>
        /// <returns></returns>
        public bool Execute(WCTask task, Priority priority)
        {
            if (task == null)
                throw new ArgumentNullException("task");
            
            if (stop == true) return false;            

            this.mutex.WaitOne();
            int prevCount = this.semaphore.Release(1);
            this.taskQueue.Enqueue(task, priority);
            Log.Info("task {0} puted into queue", task.ToString());
            this.mutex.ReleaseMutex();

            return true;
        }

        /// <summary>
        /// Prevent adding tasks to task queue.
        /// Wait while threads finishes their works.
        /// </summary>
        public void Stop()
        {
            Log.Info("queue is stoping...");
            this.stop = true;
          
            foreach (var t in this.pool)
            {
                if (t.IsAlive) t.Join();
            }
            Log.Info("queue stoped");
        }

        /// <summary>
        /// Точка входа для всех threads из pool
        /// </summary>
        /// <param name="obj"></param>
        private void RunTask(object obj)
        {
            try
            {
                while (!stop)
                {
                    bool isTimeOut = !WaitHandle.WaitAll(this.handlers, TimeSpan.FromSeconds(2));
                    if (isTimeOut) 
                        continue;
                    WCTask task = this.taskQueue.Dequeue();
                    Log.Info("thread {0} taked a task {1} from queue",
                        Thread.CurrentThread.Name, task.ToString());                        
                    this.mutex.ReleaseMutex();
                    // мы не уверены в том, что пользователь
                    // нашего пула создал корректный код задачи (task)
                    try
                    {
                        task.Execute();
                        Thread.Sleep(2000 * this.maxThreads); // типа работает.
                    }
                    catch (Exception ex) 
                    {
                        Log.Error(ex);
                    }
                }
                Log.Info("thread {0} went out from loop",
                        Thread.CurrentThread.Name); 
            }
            catch (ApplicationException ex)
            {
                // это может быть не только mutex но и код
                // стратегии, который инжектится в priorityqueue.

                /* Начиная с версии 2.0 платформы .NET Framework, в
                 * следующем потоке, получившем мьютекс, выдается исключение 
                 * AbandonedMutexException.
                 * Недопустим этого! 
                 */
                Log.Error(ex);
            }
            catch (Exception ex)
            {
                // это выбросил либо наш 
                this.mutex.ReleaseMutex();
                Log.Error(ex);
            }
        }

    }
}
