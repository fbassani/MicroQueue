using System;
using System.Collections.Concurrent;
using System.Threading;

namespace MicroQueue {
    public class MainQueue<T> : IQueue<T> {
        private readonly int _maxWorkers;
        private readonly BlockingCollection<T> _innerQueue;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly Worker<T>[] _workers;
        private bool _started;

        public int AddTimeoutInMilliseconds { get; set; }

        public MainQueue(int maxWorkers) : this(maxWorkers, new ConcurrentQueue<T>(), new CancellationTokenSource()) { }

        public MainQueue(int maxWorkers, IProducerConsumerCollection<T> storage, CancellationTokenSource cancellationTokenSource) {
            ThrowIfMaxWorkersLowerThanOne(maxWorkers);
            _maxWorkers = maxWorkers;
            _cancellationTokenSource = cancellationTokenSource;
            _innerQueue = new BlockingCollection<T>(storage);
            _workers = new Worker<T>[maxWorkers];
            AddTimeoutInMilliseconds = 2000;
        }

        private static void ThrowIfMaxWorkersLowerThanOne(int maxWorkers) {
            if (maxWorkers < 1) {
                throw new ArgumentException("maxWorkers should be greater than 0", "maxWorkers");
            }
        }

        public void Start(Func<IEnqueuedObjectProcessor<T>> enqueuedObjectProcessorFactory) {
            for (int i = 0; i < _maxWorkers; i++) {
                var worker = new Worker<T>("Worker_" + i, _cancellationTokenSource.Token, enqueuedObjectProcessorFactory);
                var workerThread = new Thread(worker.DoWork);
                workerThread.Name = "MicroQueue.Worker_" + (i + 1);
                workerThread.Start(_innerQueue);
                _workers[i] = worker;
            }
            _started = true;
        }

        public void Stop() {
            _cancellationTokenSource.Cancel();
            _innerQueue.CompleteAdding();
            _cancellationTokenSource.Dispose();
        }

        public bool EnqueueForProcessing(T obj) {
            if (!_started) {
                throw new InvalidOperationException("The queueing process was not started");
            }
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
