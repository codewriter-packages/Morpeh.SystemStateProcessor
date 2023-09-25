using System;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Scellecs.Morpeh.Tests
{
    public class SystemStateProcessorTests
    {
        private World _world;

        private Stash<TestComponent> _testComponentStash;
        private Stash<KeepAliveComponent> _keepAliveComponentStash;
        private Stash<TestStateComponent> _testStateComponentStash;

        [SetUp]
        public void SetUp()
        {
            _world = World.Create();

            _testComponentStash = _world.GetStash<TestComponent>();
            _keepAliveComponentStash = _world.GetStash<KeepAliveComponent>();
            _testStateComponentStash = _world.GetStash<TestStateComponent>();
        }

        [TearDown]
        public void TearDown()
        {
            _world.Dispose();
        }

        [Test]
        public void DisallowMultipleDisposableStateProcessors()
        {
            _world.Filter
                .With<TestComponent>()
                .ToSystemStateProcessor(NoInit<TestStateComponent>, NoCleanup);

            var ex = Assert.Catch<Exception>(() =>
            {
                _world.Filter
                    .With<TestComponent>()
                    .ToSystemStateProcessor(NoInit<TestStateComponent>, NoCleanup);
            });

            Assert.AreEqual(
                "Only one instance of DisposableSystemStateProcessor<TestStateComponent> can be created per world",
                ex.Message);
        }

        [Test]
        public void DisposableStateProcessorCleanupEnvOnDispose()
        {
            var p1 = _world.Filter
                .With<TestComponent>()
                .ToSystemStateProcessor(NoInit<TestStateComponent>, NoCleanup);

            p1.Dispose();

            var p2 = _world.Filter
                .With<TestComponent>()
                .ToSystemStateProcessor(NoInit<TestStateComponent>, NoCleanup);

            p2.Dispose();

            Assert.Pass();
        }

        [Test]
        public void DisallowDisposableStateComponent()
        {
            var ex = Assert.Catch<Exception>(() =>
            {
                _world.Filter
                    .With<TestComponent>()
                    .ToSystemStateProcessor(NoInit<DisposableTestStateComponent>, NoCleanup);
            });

            Assert.AreEqual("DisposableTestStateComponent cannot be IDisposable", ex.Message);
        }

        [Test]
        public void InitOnAdd()
        {
            var initNum = 0;

            TestStateComponent Init(Entity entity)
            {
                initNum++;
                return new TestStateComponent();
            }

            var processor = _world.Filter
                .With<TestComponent>()
                .ToSystemStateProcessor(Init);

            _testComponentStash.Add(_world.CreateEntity());
            _world.Commit();
            processor.Process();

            Assert.AreEqual(1, initNum);
        }

        [Test]
        public void DelayedCleanupOnRemove()
        {
            var cleanNum = 0;

            void Cleanup(ref TestStateComponent state) => cleanNum++;

            var processor = _world.Filter
                .With<TestComponent>()
                .ToSystemStateProcessor(NoInit<TestStateComponent>, Cleanup);

            var testEntity = _world.CreateEntity();
            _testComponentStash.Add(testEntity);
            _keepAliveComponentStash.Add(testEntity);

            _world.Commit();
            processor.Process();

            _testComponentStash.Remove(testEntity);

            _world.Commit();

            Assert.AreEqual(0, cleanNum);

            processor.Process();

            Assert.AreEqual(1, cleanNum);
        }

        [Test]
        public void ImmediateCleanupOnDispose()
        {
            var cleanNum = 0;

            void Cleanup(ref TestStateComponent state) => cleanNum++;

            var processor = _world.Filter
                .With<TestComponent>()
                .ToSystemStateProcessor(NoInit<TestStateComponent>, Cleanup);

            var testEntity = _world.CreateEntity();
            _testComponentStash.Add(testEntity);
            _keepAliveComponentStash.Add(testEntity);

            _world.Commit();
            processor.Process();

            Assert.AreEqual(0, cleanNum);

            testEntity.Dispose();

            Assert.AreEqual(1, cleanNum);
        }

        [Test]
        public void CleanupStateOnProcessorDispose()
        {
            var cleanupsNum = 0;

            var processor = _world.Filter
                .With<TestComponent>()
                .ToSystemStateProcessor(NoInit<TestStateComponent>, Cleanup);

            var testEntity = _world.CreateEntity();
            _testComponentStash.Add(testEntity);

            _world.Commit();
            processor.Process();
            processor.Dispose();

            Assert.IsFalse(_testStateComponentStash.Has(testEntity));
            Assert.AreEqual(1, cleanupsNum);

            void Cleanup(ref TestStateComponent state)
            {
                cleanupsNum++;
            }
        }

        private static T NoInit<T>(Entity entity) where T : struct
        {
            return default;
        }

        private static void NoCleanup<T>(ref T state)
        {
        }

        [Serializable]
        public struct TestComponent : IComponent
        {
        }

        [Serializable]
        public struct TestStateComponent : ISystemStateComponent
        {
        }

        [Serializable]
        public struct DisposableTestStateComponent : ISystemStateComponent, IDisposable
        {
            public void Dispose()
            {
            }
        }

        [Serializable]
        public struct KeepAliveComponent : IComponent
        {
        }
    }
}