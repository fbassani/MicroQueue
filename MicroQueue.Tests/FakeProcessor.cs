namespace MicroQueue.Tests {
    public class FakeProcessor : IEnqueuedObjectProcessor<ObjectToBeProcessed> {
        public virtual void Process(ObjectToBeProcessed obj) {
            obj.Processed = true;
        }
    }
}