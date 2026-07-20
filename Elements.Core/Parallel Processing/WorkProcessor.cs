using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Threading.Tasks.Dataflow;

namespace Elements.Core
{
    // NOTE: These are technicaly legacy and are not actually respected since the system doesn't really support them
    // We are keeping them around so we can potentially expand the system to support this
    public enum WorkType
    {
        Background,
        HighPriority
    }

    public class WorkProcessor : SynchronizationContext, IDisposable
    {
        public static WorkProcessor Instance { get; private set; }

        //static readonly AsyncLocal<WorkType> workType = new AsyncLocal<WorkType>();

        public bool IsDisposed { get; private set; }

        public int CurentlyProcessingJobs => _activeJobs;
        public int LastProcessedJobs => _lastProcessedJobs;

        volatile int _lastProcessedJobs;
        volatile int _processedJobs;
        volatile int _activeJobs;

        ActionBlock<WorkTask> _processor;

        struct WorkTask
        {
            public readonly Action action;

            public WorkTask(Action action)
            {
                this.action = action;
            }
        }

        public void Update()
        {
            _lastProcessedJobs = Interlocked.Exchange(ref _processedJobs, 0);
        }

        public void Dispose()
        {
            CheckDisposed();

            IsDisposed = true;

            _processor.Complete();
        }

        void CheckDisposed()
        {
            if (IsDisposed)
                throw new Exception("WorkProcessor is disposed");
        }

        public WorkProcessor()
        {
            Instance = this;

            _processor = new ActionBlock<WorkTask>(ProcessTask, new ExecutionDataflowBlockOptions()
            {
                MaxDegreeOfParallelism = -1,
                EnsureOrdered = false,
            });
        }

        void ProcessTask(WorkTask task)
        {
            try
            {
                Interlocked.Increment(ref _activeJobs);

                task.action();

                Interlocked.Increment(ref _processedJobs);
            }
            catch(Exception ex)
            {
                UniLog.Error("Exception in background job:\n" + ex, false);
            }
            finally
            {
                Interlocked.Decrement(ref _activeJobs);
            }
        }

        public void Enqueue(Action work, WorkType workType = WorkType.Background)
        {
            CheckDisposed();

            _processor.Post(new WorkTask(work));
        }

        public override void Post(SendOrPostCallback d, object state)
        {
            Enqueue(() => d(state) /*, workType.Value*/);
        }
    }
}
