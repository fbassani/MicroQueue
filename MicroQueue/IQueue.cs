using System;

namespace MicroQueue {
    public interface IQueue<T> {
        void Start(Func<IEnqueuedObjectProcessor<T>> enqueuedObjectProcessorFactory);
        void Stop(bool abortThreads = false);
        bool EnqueueForProcessing(T obj);
    }
}