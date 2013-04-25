using System;

namespace MicroQueue {
    public interface IQueue<T> {
        string Name { get; set; }
        void Start(Func<IEnqueuedObjectProcessor<T>> enqueuedObjectProcessorFactory);
        void Stop(bool abortThreads = false);
        bool EnqueueForProcessing(T obj);
    }
}