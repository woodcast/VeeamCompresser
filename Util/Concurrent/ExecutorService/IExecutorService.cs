using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WC.Util.Concurrent.ExecutorService
{
    public interface IExecutorService
    {
        /// <summary>
        /// Executes the given task at some time in the future
        /// </summary>
        /// <param name="task">the runnable task</param>
        /// <param name="priority">the task priority</param>
        /// <returns>true if this task has been accepted for execution</returns>
        bool Execute(WCTask task, Priority priority);
        void Stop();
    }
}
