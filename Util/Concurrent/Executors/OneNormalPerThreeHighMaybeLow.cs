using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WC.Util.Concurrent.ExecutorService;

namespace WC.Util.Concurrent.Executors
{
    public class OneNormalPerThreeHighMaybeLowStrategy : IPriorityTaskQueueStrategy
    {
        private int highCounter;

        public OneNormalPerThreeHighMaybeLowStrategy()
        {
            this.highCounter = 3;
        }

        public WCTask GetNextTask(PriorityTaskQueue queue)
        {
            WCTask result;
            if (this.highCounter > 0 && queue.GetQueue(Priority.HIGH).Count > 0)
            {
                result = queue.GetQueue(Priority.HIGH).Dequeue();
                highCounter--;
            }
            else
            {
                if (queue.GetQueue(Priority.NORMAL).Count > 0)
                    result = queue.GetQueue(Priority.NORMAL).Dequeue();
                else
                    result = queue.GetQueue(Priority.LOW).Dequeue();

                highCounter = 3;
            }

            return result;
        }
    }
}
