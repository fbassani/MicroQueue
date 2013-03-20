using System;
using System.Collections.Concurrent;
using System.Threading;

namespace MicroQueue {
    public class MainQueue<T> : IQueue<T> {
        private readonly int _maxWorkers;
        private readonly BlockingCollection<T> _innerQueue;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly Thread[] _workerThreads;
        private bool _started;

        public int AddTimeoutInMilliseconds { get; set; }

        public MainQueue(int maxWorkers) : this(maxWorkers, new ConcurrentQueue<T>(), new CancellationTokenSource()) { }

        public MainQueue(int maxWorkers, IProducerConsumerCollection<T> storage, CancellationTokenSource cancellationTokenSource) {
            ThrowIfMaxWorkersLowerThanOne(maxWorkers);
            _maxWorkers = maxWorkers;
            _cancellationTokenSource = cancellationTokenSource;
            _innerQueue = new BlockingCollection<T>(storage);
            _workerThreads = new Thread[maxWorkers];
            AddTimeoutInMilliseconds = 2000;
        }

        private static void ThrowIfMaxWorkersLowerThanOne(int maxWorkers) {
            if (maxWorkers < 1) {
                throw new ArgumentException("maxWorkers should be greater than 0", "maxWorkers");
            }
        }

        public void Start(Func<IEnqueuedObjectProcessor<T>> enqueuedObjectProcessorFactory) {
            for (int i = 0; i < _maxWorkers; i++) {
                var workerIndex = (i + 1);
                var worker = new Worker<T>("Worker_" + workerIndex, _cancellationTokenSource.Token, enqueuedObjectProcessorFactory);
                _workerThreads[i] = CreateWorkerThread(worker, workerIndex);
            }
            _started = true;
        }

        private Thread CreateWorkerThread(Worker<T> worker, int workerIndex) {
            var workerThread = new Thread(worker.DoWork);
            workerThread.Name = "MicroQueue.Worker_" + workerIndex;
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
            foreach (var thread in _workerThreads) {
                thread.Abort();
            }
        }

        public bool EnqueueForProcessing(T obj) {
            if (!_started) {
                throw new InvalidOperationException("The queueing process was not started");
            }
            return TryEnqueue(obj);
        }

        private bool TryEnqueue(T obj) {
            try {
                if (_innerQueue.TryAdd(obj, AddTimeoutInMilliseconds, _cancellationTokenSource.Token)) {
                    TraceHelper.TraceMessage(obj + " enqueued");
                    return true;
                }
                TraceHelper.TraceMessage(obj + " was not enqueued");
                return false;
            }
            catch (ObjectDisposedException) {
                TraceHelper.TraceMessage(obj + " was not enqueued due to cancellation");
                return false;
            }
        }
    }
}
