using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;

namespace Elements.Core
{
    public static class JobExtensions
    {
        public static Job AsJob(this Task task) => Job.FromAsyncTask(task);
        public static Job<T> AsJob<T>(this Task<T> task) => Job.FromAsyncTask<T>(task);
    }

    public class Job : INotifyCompletion
    {
        public static readonly Job FinishedJob;

        static Job()
        {
            FinishedJob = new Job();
            FinishedJob.Finish();
        }

        protected object lockobj = new object();

        public bool IsCompleted { get; private set; }

        event Action _onDone;

        public event Action OnDone
        {
            add
            {
                // Do a quick lock-less check first
                if (IsCompleted)
                    value();
                else
                    lock (lockobj)
                    {
                        // Do a second check, in case it was finished
                        if (IsCompleted)
                            value();
                        else
                            _onDone += value;
                    }
            }

            remove
            {
                lock (lockobj)
                    _onDone -= value;
            }
        }

        public void Finish()
        {
            lock (lockobj)
            {
                if (IsCompleted)
                {
                    Elements.Core.UniLog.Error("Calling Task.Finish() multiple times!");
                    throw new Exception("The task has already finished, cannot call Finish twice");
                }

                IsCompleted = true;
            }

            // Call all the registered events outside of the lock (IsDone prevents any further ones from being registered)
            _onDone?.Invoke();

            // Clear them, no longer needed
            _onDone = null;
        }

        public bool Wait(int millisecondsTimeout = -1)
        {
            // First check IsDone without lock, in case it's already done
            if (IsCompleted)
                return true;

            // It's still running, register wait event
            var waitEvent = new ManualResetEvent(false);

            // event registering takes care of locking and calling immediatelly in case it finishes
            OnDone += () => waitEvent.Set();

            // Wait until finished
            bool result = waitEvent.WaitOne(millisecondsTimeout);
            waitEvent.Close();

            return result;
        }

        public static Job FromAsyncTask(Task task)
        {
            var job = new Job();
            task.ContinueWith(t => job.Finish());
            return job;
        }

        public static Job<T> FromAsyncTask<T>(Task<T> task)
        {
            var job = new Job<T>();
            task.ContinueWith(t => job.SetResultAndFinish(t.Result));
            return job;
        }

        public Job GetAwaiter() => this;

        public void OnCompleted(Action continuation) => OnDone += continuation;

        public void GetResult() { }
    }

    public class Job<T> : Job
    {
        T _result;

        public event Action<T> OnResultDone
        {
            add { OnDone += () => value(_result); }
            remove { throw new NotSupportedException(); }
        }

        public T Result
        {
            get
            {
                // Wait synchronously if not done yet
                if (!IsCompleted)
                    Wait();

                return _result;
            }
        }

        // TODO!!! Protect from being called multiple times?
        public void SetResultAndFinish(T result)
        {
            _result = result;
            Finish();
        }

        public new Job<T> GetAwaiter() => this;

        public new T GetResult() => _result;
    }
}
