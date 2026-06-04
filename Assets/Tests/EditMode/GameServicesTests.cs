using NUnit.Framework;
using BogatyriMoba.Core;

namespace BogatyriMoba.Tests
{
    [TestFixture]
    public class GameServicesTests
    {
        [SetUp]
        public void SetUp() => GameServices.Clear();

        [TearDown]
        public void TearDown() => GameServices.Clear();

        [Test]
        public void Register_And_Resolve_ReturnsInstance()
        {
            var service = new TestService();
            GameServices.Register<ITestService>(service);
            Assert.AreEqual(service, GameServices.Get<ITestService>());
        }

        [Test]
        public void Resolve_NotRegistered_ReturnsNull()
        {
            Assert.IsNull(GameServices.Get<ITestService>());
        }

        [Test]
        public void Unregister_RemovesService()
        {
            var service = new TestService();
            GameServices.Register<ITestService>(service);
            GameServices.Unregister<ITestService>();
            Assert.IsNull(GameServices.Get<ITestService>());
        }

        [Test]
        public void Register_OverwritesExisting()
        {
            var first = new TestService();
            var second = new TestService();
            GameServices.Register<ITestService>(first);
            GameServices.Register<ITestService>(second);
            Assert.AreEqual(second, GameServices.Get<ITestService>());
        }

        [Test]
        public void Clear_RemovesAll()
        {
            GameServices.Register<ITestService>(new TestService());
            GameServices.Register<IAnotherService>(new AnotherService());
            GameServices.Clear();
            Assert.IsNull(GameServices.Get<ITestService>());
            Assert.IsNull(GameServices.Get<IAnotherService>());
        }

        private interface ITestService { }
        private class TestService : ITestService { }
        private interface IAnotherService { }
        private class AnotherService : IAnotherService { }
    }
}
