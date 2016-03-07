using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WC.Util.Concurrent.Executors;

namespace WC.Util.Concurrent.ExecutorService
{
    public class PriorityTaskQueue
    {
        IPriorityTaskQueueStrategy strategy;

        Dictionary<Priority, Queue<WCTask>> tasks;

        public PriorityTaskQueue(IPriorityTaskQueueStrategy strategy)
        {
            this.strategy = strategy;

            this.tasks = new Dictionary<Priority, Queue<WCTask>>();
            this.tasks.Add(Priority.HIGH, new Queue<WCTask>(new Queue<WCTask>()));
            this.tasks.Add(Priority.NORMAL, new Queue<WCTask>(new Queue<WCTask>()));
            this.tasks.Add(Priority.LOW, new Queue<WCTask>(new Queue<WCTask>()));
        }

        public void Enqueue(WCTask task, Priority priority)
        {
            this.tasks[priority].Enqueue(task);
        }

        public WCTask Dequeue()
        {
            return this.strategy.GetNextTask(this);
        }

        public WCTask Dequeue(Priority priority)
        {
            return this.tasks[priority].Dequeue();
        }

        public int Count 
        { 
            get 
            {
                return this.tasks[Priority.HIGH].Count +
                    this.tasks[Priority.NORMAL].Count +
                    this.tasks[Priority.LOW].Count;
            } 
        }

        public Queue<WCTask> GetQueue(Priority priority)
        {
            return this.tasks[priority];
        }
    }
}
