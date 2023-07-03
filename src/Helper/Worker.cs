using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HuaweiHMSInstaller.Helper
{
    // Define a class that does some work
    public class Worker<T>
    {
        // Define a delegate for the event handler
        public delegate void WorkCompletedEventHandler(object sender, T result);

        // Define an event that is raised when the work is completed
        public event WorkCompletedEventHandler WorkCompleted;

        // Define a method that does some work asynchronously
        public async Task DoWorkAsync(Func<T> workFunction)
        {
            // Do some work using the workFunction
            T result = await Task.Run(workFunction);

            // Raise the event
            OnWorkCompleted(result);
        }        
        public async Task DoWorkAsync(Func<Task<T>> workFunction)
        {
            // Do some work using the workFunction
            T result = await workFunction?.Invoke();

            // Raise the event
            OnWorkCompleted(result);
        }

        // Define a method that invokes the event handler
        protected virtual void OnWorkCompleted(T result)
        {
            // Check if there are any subscribers
            if (WorkCompleted != null)
            {
                // Invoke the event handler
                WorkCompleted(this, result);
            }
        }
    }


}
