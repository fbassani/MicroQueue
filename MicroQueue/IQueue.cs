using System;

namespace MicroQueue {
    public interface IQueue<T> {
        void Start(Func<IEnqueuedObjectProcessor<T>> enqueuedObjectProcessorFactory);
        void Stop();
        bool EnqueueForProcessing(T obj);
    }
}