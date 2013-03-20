using System.Collections.Concurrent;
using System.Threading;
using NUnit.Framework;

namespace MicroQueue.Tests {
    [TestFixture]
    public class WorkerTests {
        private Worker<ObjectToBeProcessed> _worker;
        private BlockingCollection<ObjectToBeProcessed> _collection;
        private ObjectToBeProcessed _objectToBeProcessed;
        private CancellationTokenSource _cancellationTokenSource;

        [SetUp]
        public void SetUp() {
            _collection = new BlockingCollection<ObjectToBeProcessed>();
            _objectToBeProcessed = new ObjectToBeProcessed();
            _collection.Add(_objectToBeProcessed);
            _cancellationTokenSource = new CancellationTokenSource();
            _worker = new Worker<ObjectToBeProcessed>("worker", _cancellationTokenSource.Token, () => new FakeProcessor());
            _cancellationTokenSource.CancelAfter(500);
        }
        
        [Test]
        public void DoWork_should_take_the_object_from_the_collection() {
            _worker.DoWork(_collection);
            Assert.AreEqual(0, _collection.Count);
        }

        [Test]
        public void DoWork_should_process_the_object() {
            _worker.DoWork(_collection);
            Assert.IsTrue(_objectToBeProcessed.Processed);
        }

        [Test]
        public void DoWork_should_not_take_the_object_from_the_collection_if_cancelled() {
            _cancellationTokenSource.Cancel();
            _worker.DoWork(_collection);
            Assert.AreEqual(1, _collection.Count);
        }
    }

}