using System;
using System.Collections.Concurrent;
using System.Threading;

namespace MicroQueue {
    public class DefaultQueue<T> : IQueue<T> {
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly BlockingCollection<T> _innerQueue;
        private readonly int _numWorkers;
        private readonly Thread[] _workerThreads;
        private bool _started;

        public int AddTimeoutInMilliseconds { get; set; }
        public string Name { set; get; }

        public DefaultQueue(int numWorkers) : this(numWorkers, new ConcurrentQueue<T>(), new CancellationTokenSource()) {}

        public DefaultQueue(int numWorkers, IProducerConsumerCollection<T> storage, CancellationTokenSource cancellationTokenSource) {
            ThrowIfNumWorkersLowerThanOne(numWorkers);
            _numWorkers = numWorkers;
            _cancellationTokenSource = cancellationTokenSource;
            _innerQueue = new BlockingCollection<T>(storage);
            _workerThreads = new Thread[numWorkers];
            AddTimeoutInMilliseconds = 2000;
            Name = "DefaultQueue";
        }

        private static void ThrowIfNumWorkersLowerThanOne(int numWorkers) {
            if (numWorkers < 1) {
                throw new ArgumentException("numWorkers should be greater than 0", "numWorkers");
            }
        }

        public void Start(Func<IEnqueuedObjectProcessor<T>> enqueuedObjectProcessorFactory) {
            for (int i = 0; i < _numWorkers; i++) {
                int workerIndex = (i + 1);
                string workerName = GetWorkerName(workerIndex);
                var worker = new Worker<T>(workerName, _cancellationTokenSource.Token, enqueuedObjectProcessorFactory);
                _workerThreads[i] = CreateWorkerThread(worker, workerIndex);
            }
            _started = true;
        }

        private string GetWorkerName(int workerIndex) {
            return String.Format("{0}.Worker_{1}", Name, workerIndex);
        }

        private Thread CreateWorkerThread(Worker<T> worker, int workerIndex) {
            var workerThread = new Thread(worker.DoWork);
            workerThread.Name = GetWorkerName(workerIndex);
            workerThread.Start(_innerQueue);
            return workerThread;
        }

        public void Stop(bool abortThreads = false) {
            _cancellationTokenSource.Cancel();
            _innerQueue.CompleteAdding();
            _cancellationTokenSource.Dispose();
            if (abortThreads) {
                AbortThreads();
            }
        }

        private void AbortThreads() {
            Array.ForEach(_workerThreads, t => t.Abort());
        }

        public bool EnqueueForProcessing(T obj) {
            ThrowIfNotStarted();
            return TryEnqueue(obj);
        }

        private void ThrowIfNotStarted() {
            if (!_started) {
                throw new InvalidOperationException(String.Format("[{0}] The queueing process was not started", Name));
            }
        }

        private bool TryEnqueue(T obj) {
            try {
                if (_innerQueue.TryAdd(obj, AddTimeoutInMilliseconds, _cancellationTokenSource.Token)) {
                    TraceHelper.TraceMessage(String.Format("[{0}] {1} enqueued", Name, obj));
                    return true;
                }
                TraceHelper.TraceMessage(String.Format("[{0}]: {1} was not enqueued", Name, obj));
                return false;
            } catch (ObjectDisposedException) {
                TraceHelper.TraceMessage(String.Format("[{0}]: {1} was not enqueued due to cancellation", Name, obj));
                return false;
            }
        }
    }
}