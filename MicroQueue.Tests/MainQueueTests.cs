using System;
using System.Collections.Concurrent;
using System.Threading;
using Moq;
using NUnit.Framework;

namespace MicroQueue.Tests {
    [TestFixture]
    public class MainQueueTests {
        private MainQueue<ObjectToBeProcessed> _mainQueue;
        private const int MaxWorkers = 5;
        private Mock<IProducerConsumerCollection<ObjectToBeProcessed>> _storageMock;
        private CancellationTokenSource _cancellationTokenSource;

        [SetUp]
        public void SetUp() {
            _storageMock = new Mock<IProducerConsumerCollection<ObjectToBeProcessed>>();
            _storageMock.Setup(s => s.TryAdd(It.IsAny<ObjectToBeProcessed>())).Returns(true);
            _cancellationTokenSource = new CancellationTokenSource();
            _mainQueue = new MainQueue<ObjectToBeProcessed>(MaxWorkers, _storageMock.Object, _cancellationTokenSource);
            _mainQueue.AddTimeoutInMilliseconds = 100;
            _mainQueue.Start(() => new FakeProcessor());
        }

        [Test]
        public void EnqueueForProcessing_should_enqueue_object() {
            var objectToBeProcessed = new ObjectToBeProcessed();
            var result = _mainQueue.EnqueueForProcessing(objectToBeProcessed);
            _storageMock.Verify(s => s.TryAdd(objectToBeProcessed));
            Assert.IsTrue(result);
        }

        [Test]
        public void EnqueueForProcessing_should_not_enqueue_object_after_stopped() {
            _mainQueue.Stop();
            var result = _mainQueue.EnqueueForProcessing(new ObjectToBeProcessed());
            Assert.IsFalse(result);
        }

        [Test, ExpectedException(typeof(InvalidOperationException))]
        public void EnqueueForProcessing_should_throw_if_not_started() {
            _mainQueue = new MainQueue<ObjectToBeProcessed>(1);
            _mainQueue.EnqueueForProcessing(new ObjectToBeProcessed());
        }

        [Test, ExpectedException(typeof(ArgumentException))]
        public void Constructor_should_throw_if_max_workers_is_lower_than_one() {
            new MainQueue<ObjectToBeProcessed>(0);
        }
    }
}