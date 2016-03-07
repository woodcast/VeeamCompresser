using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WC.Util.Concurrent.ExecutorService;

namespace WC.Util.Concurrent.Executors
{
    public interface IPriorityTaskQueueStrategy
    {
        WCTask GetNextTask(PriorityTaskQueue queue);
    }
}
