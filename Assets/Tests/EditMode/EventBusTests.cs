using NUnit.Framework;
using BogatyriMoba.Core;

namespace BogatyriMoba.Tests
{
    [TestFixture]
    public class EventBusTests
    {
        private struct TestEvent : IGameEvent { public int Value; }
        private struct OtherEvent : IGameEvent { public string Text; }

        [SetUp]
        public void SetUp() => EventBus.Clear();

        [TearDown]
        public void TearDown() => EventBus.Clear();

        [Test]
        public void Publish_CallsSubscriber()
        {
            int received = -1;
            EventBus.Subscribe<TestEvent>(e => received = e.Value);
            EventBus.Publish(new TestEvent { Value = 5 });
            Assert.AreEqual(5, received);
        }

        [Test]
        public void Unsubscribe_StopsDelivery()
        {
            int received = 0;
            void Handler(TestEvent e) => received = e.Value;

            EventBus.Subscribe<TestEvent>(Handler);
            EventBus.Unsubscribe<TestEvent>(Handler);
            EventBus.Publish(new TestEvent { Value = 99 });

            Assert.AreEqual(0, received);
        }

        [Test]
        public void SubscribeOnce_FiresExactlyOnce()
        {
            int callCount = 0;
            EventBus.SubscribeOnce<TestEvent>(_ => callCount++);

            EventBus.Publish(new TestEvent());
            EventBus.Publish(new TestEvent());

            Assert.AreEqual(1, callCount);
        }

        [Test]
        public void Publish_DoesNotCrossEventTypes()
        {
            bool testFired = false;
            bool otherFired = false;
            EventBus.Subscribe<TestEvent>(_ => testFired = true);
            EventBus.Subscribe<OtherEvent>(_ => otherFired = true);

            EventBus.Publish(new TestEvent());

            Assert.IsTrue(testFired);
            Assert.IsFalse(otherFired);
        }

        [Test]
        public void Publish_MultipleSubscribers_AllReceive()
        {
            int total = 0;
            EventBus.Subscribe<TestEvent>(e => total += e.Value);
            EventBus.Subscribe<TestEvent>(e => total += e.Value);

            EventBus.Publish(new TestEvent { Value = 3 });

            Assert.AreEqual(6, total);
        }

        [Test]
        public void Publish_NoSubscribers_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => EventBus.Publish(new TestEvent { Value = 1 }));
        }

        [Test]
        public void ThrowingSubscriber_DoesNotPreventOtherSubscribers()
        {
            bool secondCalled = false;
            EventBus.Subscribe<TestEvent>(_ => throw new System.Exception("intentional"));
            EventBus.Subscribe<TestEvent>(_ => secondCalled = true);

            Assert.DoesNotThrow(() => EventBus.Publish(new TestEvent()));
            Assert.IsTrue(secondCalled, "Second subscriber should run even if first throws");
        }

        [Test]
        public void Clear_RemovesAllSubscribers()
        {
            int callCount = 0;
            EventBus.Subscribe<TestEvent>(_ => callCount++);
            EventBus.Clear();
            EventBus.Publish(new TestEvent());
            Assert.AreEqual(0, callCount);
        }
    }
}
