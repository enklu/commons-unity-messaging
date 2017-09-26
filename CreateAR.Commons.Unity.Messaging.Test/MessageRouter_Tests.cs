using System;
using CreateAR.Commons.Unity.Async;
using NUnit.Framework;

namespace CreateAR.Commons.Unity.Messaging
{
    [TestFixture]
    public class MessageRouter_Tests
    {
        public class Message
        {
            public string Foo = Guid.NewGuid().ToString();
        }

        private const int MESSAGE_TYPE_A = 1;
        private const int MESSAGE_TYPE_B = 2;
        
        private MessageRouter _router;

        [SetUp]
        public void Setup()
        {
            _router = new MessageRouter();
        }

        [Test]
        public void Subscribe()
        {
            var called = false;
            var message = new Message();

            _router.Subscribe(
                MESSAGE_TYPE_A,
                (received, unsub) =>
                {
                    called = true;

                    Assert.AreSame(message, received);
                });

            _router.Publish(
                MESSAGE_TYPE_A,
                message);

            Assert.IsTrue(called);
        }

        [Test]
        public void MultiSubscribe()
        {
            var calledA = false;
            var calledB = false;
            var message = new Message();

            _router.Subscribe(
                MESSAGE_TYPE_A,
                (received, unsub) =>
                {
                    calledA = true;

                    Assert.AreSame(message, received);
                });

            _router.Subscribe(
                MESSAGE_TYPE_A,
                (received, unsub) =>
                {
                    calledB = true;

                    Assert.AreSame(message, received);
                });

            _router.Publish(
                MESSAGE_TYPE_A,
                message);

            Assert.IsTrue(calledA);
            Assert.IsTrue(calledB);
        }

        [Test]
        public void SubscribeRelevantEvents()
        {
            var called = false;
            var message = new Message();

            _router.Subscribe(
                MESSAGE_TYPE_A,
                (received, unsub) =>
                {
                    called = true;
                });

            _router.Publish(
                MESSAGE_TYPE_B,
                message);

            Assert.IsFalse(called);
        }

        [Test]
        public void SubscribeMultipleCalls()
        {
            var called = 0;
            var message = new Message();

            _router.Subscribe(
                MESSAGE_TYPE_A,
                (received, unsub) =>
                {
                    called++;
                });

            _router.Publish(
                MESSAGE_TYPE_A,
                message);

            _router.Publish(
                MESSAGE_TYPE_A,
                message);

            Assert.AreEqual(2, called);
        }

        [Test]
        public void SubscribeUnsubscribe()
        {
            var called = 0;
            var message = new Message();

            _router.Subscribe(
                MESSAGE_TYPE_A,
                (received, unsub) =>
                {
                    called++;

                    unsub();
                });

            _router.Publish(
                MESSAGE_TYPE_A,
                message);
            
            _router.Publish(
                MESSAGE_TYPE_A,
                message);

            Assert.AreEqual(1, called);
        }

        [Test]
        public void MultipleSubscribersException()
        {
            var called = false;
            var message = new Message();

            _router.Subscribe(
                MESSAGE_TYPE_A,
                (received, unsub) =>
                {
                    throw new Exception();
                });

            _router.Subscribe(
                MESSAGE_TYPE_A,
                (received, unsub) =>
                {
                    called = true;
                });

            Assert.Throws<Exception>(
                delegate
                {
                    _router.Publish(
                        MESSAGE_TYPE_A,
                        message);
                });

            Assert.IsTrue(called);
        }

        [Test]
        public void MultipleSubscribersMultipleExceptions()
        {
            var message = new Message();

            _router.Subscribe(
                MESSAGE_TYPE_A,
                (received, unsub) =>
                {
                    throw new Exception();
                });

            _router.Subscribe(
                MESSAGE_TYPE_A,
                (received, unsub) =>
                {
                    throw new Exception();
                });

            Assert.Throws<AggregateException>(
                delegate
                {
                    _router.Publish(
                        MESSAGE_TYPE_A,
                        message);
                });
        }

        [Test]
        public void SubscribeSafety()
        {
            var called = false;
            var message = new Message();

            _router.Subscribe(
                MESSAGE_TYPE_A,
                (received, unsub) =>
                {
                    // unsub in the middle of a dispatch
                    unsub();
                });

            _router.Subscribe(
                MESSAGE_TYPE_A,
                (received, unsub) =>
                {
                    called = true;
                });

            _router.Publish(
                MESSAGE_TYPE_A,
                message);

            Assert.IsTrue(called);
        }

        [Test]
        public void SubscribeCycle()
        {
            var called = 0;
            var message = new Message();

            _router.Subscribe(
                MESSAGE_TYPE_A,
                (received, unsub) =>
                {
                    called++;

                    // this publish should be discarded
                    _router.Publish(
                        MESSAGE_TYPE_A,
                        message);
                });
            
            _router.Publish(
                MESSAGE_TYPE_A,
                message);

            Assert.AreEqual(1, called);
        }

        [Test]
        public void SubscribeWhileDispatching()
        {
            var called = 0;
            var message = new Message();

            _router.Subscribe(
                MESSAGE_TYPE_A,
                (received, unsub) =>
                {
                    // subscribe in the middle of a dispatch
                    _router.Subscribe(
                        MESSAGE_TYPE_A,
                        (_, __) =>
                        {
                            called++;
                        });
                });

            _router.Publish(
                MESSAGE_TYPE_A,
                message);

            Assert.AreEqual(0, called);

            _router.Publish(
                MESSAGE_TYPE_A,
                message);

            Assert.AreEqual(1, called);
        }

        [Test]
        public void SubscribeOnce()
        {
            var called = 0;
            var message = new Message();

            _router.SubscribeOnce(
                MESSAGE_TYPE_A,
                (received, unsub) =>
                {
                    called++;

                    Assert.AreSame(message, received);
                });

            _router.Publish(
                MESSAGE_TYPE_A,
                message);

            _router.Publish(
                MESSAGE_TYPE_A,
                message);

            Assert.AreEqual(1, called);
        }

        [Test]
        public void UnsubscribeMultiple()
        {
            var called = false;
            var message = new Message();

            _router.Subscribe(
                MESSAGE_TYPE_A,
                (received, unsub) =>
                {
                    called = true;

                    unsub();

                    // shouldn't cause issues
                    unsub();
                });

            _router.Publish(
                MESSAGE_TYPE_A,
                message);

            Assert.IsTrue(called);
        }

        [Test]
        public void SubscribeAll()
        {
            var called = 0;
            var message = new Message();

            _router.SubscribeAll(
                (received, unsub) =>
                {
                    called++;
                });

            _router.Publish(
                MESSAGE_TYPE_A,
                message);

            _router.Publish(
                MESSAGE_TYPE_B,
                message);

            Assert.AreEqual(2, called);
        }

        [Test]
        public void SubscribeUnsubscribeAll()
        {
            var called = 0;
            var message = new Message();

            _router.SubscribeAll(
                (received, unsub) =>
                {
                    called++;

                    unsub();
                });

            _router.Publish(
                MESSAGE_TYPE_A,
                message);

            _router.Publish(
                MESSAGE_TYPE_B,
                message);

            Assert.AreEqual(1, called);
        }

        [Test]
        public void Consume()
        {
            var called = 0;
            var message = new Message();

            _router.Subscribe(
                MESSAGE_TYPE_A,
                (received, unsub) =>
                {
                    called++;

                    _router.Consume(received);
                });

            _router.Subscribe(
                MESSAGE_TYPE_A,
                (received, unsub) => called++);

            _router.Publish(
                MESSAGE_TYPE_A,
                message);

            Assert.AreEqual(1, called);
        }

        [Test]
        public void SubscribeReturnUnsub()
        {
            var called = false;
            var message = new Message();

            _router.Subscribe(
                MESSAGE_TYPE_A,
                received =>
                {
                    called = true;

                    Assert.AreSame(message, received);
                });

            _router.Publish(
                MESSAGE_TYPE_A,
                message);

            Assert.IsTrue(called);
        }

        [Test]
        public void SubscribeUnsubscribeReturnUnsub()
        {
            var called = false;
            var message = new Message();

            var unsub = _router.Subscribe(
                MESSAGE_TYPE_A,
                received =>
                {
                    called = true;

                    Assert.AreSame(message, received);
                });

            unsub();

            _router.Publish(
                MESSAGE_TYPE_A,
                message);

            Assert.IsFalse(called);
        }

        [Test]
        public void SubscribeUnsubscribeAllReturnedUnsub()
        {
            var called = 0;
            var message = new Message();

            var unsub = _router.SubscribeAll(
                received =>
                {
                    called++;
                });

            _router.Publish(
                MESSAGE_TYPE_A,
                message);

            unsub();

            _router.Publish(
                MESSAGE_TYPE_B,
                message);

            Assert.AreEqual(1, called);
        }
    }
}