using System.Threading;

namespace MicroQueue.Tests {
    public class ObjectToBeProcessed {
        public bool Processed { get; set; }
        public ManualResetEvent WaitHandle { get; private set; }

        public ObjectToBeProcessed() {
            WaitHandle = new ManualResetEvent(false);
        }
    }
}