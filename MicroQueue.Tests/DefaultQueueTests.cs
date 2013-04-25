using System;
using System.Collections.Concurrent;
using System.Threading;
using Moq;
using NUnit.Framework;

namespace MicroQueue.Tests {
    [TestFixture]
    public class DefaultQueueTests {
        private DefaultQueue<ObjectToBeProcessed> _defaultQueue;
        private const int MaxWorkers = 5;
        private Mock<IProducerConsumerCollection<ObjectToBeProcessed>> _storageMock;
        private CancellationTokenSource _cancellationTokenSource;

        [SetUp]
        public void SetUp() {
            _storageMock = new Mock<IProducerConsumerCollection<ObjectToBeProcessed>>();
            _storageMock.Setup(s => s.TryAdd(It.IsAny<ObjectToBeProcessed>())).Returns(true);
            _cancellationTokenSource = new CancellationTokenSource();
            _defaultQueue = new DefaultQueue<ObjectToBeProcessed>(MaxWorkers, _storageMock.Object, _cancellationTokenSource);
            _defaultQueue.AddTimeoutInMilliseconds = 100;
            _defaultQueue.Start(() => new FakeProcessor());
        }

        [Test]
        public void EnqueueForProcessing_should_enqueue_object() {
            var objectToBeProcessed = new ObjectToBeProcessed();
            var result = _defaultQueue.EnqueueForProcessing(objectToBeProcessed);
            _storageMock.Verify(s => s.TryAdd(objectToBeProcessed));
            Assert.IsTrue(result);
        }

        [Test]
        public void EnqueueForProcessing_should_not_enqueue_object_after_stopped() {
            _defaultQueue.Stop();
            var result = _defaultQueue.EnqueueForProcessing(new ObjectToBeProcessed());
            Assert.IsFalse(result);
        }

        [Test, ExpectedException(typeof(InvalidOperationException))]
        public void EnqueueForProcessing_should_throw_if_not_started() {
            _defaultQueue = new DefaultQueue<ObjectToBeProcessed>(1);
            _defaultQueue.EnqueueForProcessing(new ObjectToBeProcessed());
        }

        [Test, ExpectedException(typeof(ArgumentException))]
        public void Constructor_should_throw_if_max_workers_is_lower_than_one() {
            new DefaultQueue<ObjectToBeProcessed>(0);
        }
    }
}