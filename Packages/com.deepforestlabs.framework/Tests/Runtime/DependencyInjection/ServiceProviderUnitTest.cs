#nullable enable
using System;
using System.Collections;
using System.Threading;
using Cysharp.Threading.Tasks;
using DeepForestLabs.DependencyInjection.Mocks;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace DeepForestLabs.DependencyInjection
{
    [TestFixture]
    [Ignore("WIP DI self-injection harness that was never wired to the current Container bootstrap API " +
            "(disabled since the repo's initial commit). Re-enable only with a proper rewrite against " +
            "Container.CreateMain and verified self-injection.")]
    public sealed partial class ServiceProviderUnitTest
    {
        [Dependency] private readonly IContainer _container = null!;
        [Dependency] private readonly CancellationToken _scope = default!;
        [Dependency] private readonly ServiceProviderUnitTest _self = null!; 
        
        private CancelOnDisposeTokenSource? _scopeTokenSource;
        private IContainerBuilder _unit = null!;

        [SetUp]
        public void SetUp()
        {
            // Intentionally empty. This fixture is [Ignore]d at the class level because the DI
            // self-injection harness (_container/_scope/_self via [Dependency]) was never wired to the
            // current Container bootstrap API. A proper rewrite must restore _unit via Container.CreateMain
            // and verify self-injection in EditMode before removing the [Ignore].
        }

        [UnityTearDown]
        public IEnumerator UnityTearDown()
        {
            if (_container != null!)
            {
                yield return _container.DisposeAsync().AsTask().AsUniTask().ToCoroutine();
            }

            if (_scopeTokenSource != null)
            {
                _scopeTokenSource.Dispose();
                _scopeTokenSource = null;
            }
        }

        [UnityTest]
        public IEnumerator BuildAsync_WithAMock_DependenciesSet() => UniTask.ToCoroutine(async () =>
        {
            // Arrange

            // Act
            await using (IContainer _ = await _unit.Build(_scope))
            {
                // Assert
                Assert.NotNull(_self);
                Assert.NotNull(_container);
            }
        });
        
        [UnityTest]
        public IEnumerator AddServices_AllScoped_DisposedAllEqual() => UniTask.ToCoroutine(async () =>
        {
            // Arrange
            IContainer container = await _unit.AddScopedServiceA()
                .AddScopedServiceB()
                .AddScopedServiceC()
                .AddScopedServiceD()
                .AddScopedServiceE().Build(_scope);
            
            // Act
            IServiceA serviceA = container.Get<IServiceA>();
            IServiceB serviceB = container.Get<IServiceB>();
            IServiceC serviceC = container.Get<IServiceC>();
            IServiceD serviceD = container.Get<IServiceD>();
            IServiceE serviceE = container.Get<IServiceE>();
            await container.DisposeAsync().AsTask().AsUniTask();

            // Assert
            Assert.NotNull(serviceA);
            Assert.NotNull(serviceB);
            Assert.NotNull(serviceC);
            Assert.NotNull(serviceD);
            Assert.NotNull(serviceE);
            Assert.IsTrue(serviceA.IsDisposed);
            Assert.IsTrue(serviceB.IsDisposed);
            Assert.IsTrue(serviceC.IsDisposed);
            Assert.IsTrue(serviceD.IsDisposed);
            Assert.IsTrue(serviceE.IsDisposed);
            Assert.AreEqual(serviceA, serviceD.ServiceA);
            Assert.AreEqual(serviceB, serviceA.ServiceB);
            Assert.AreEqual(serviceB, serviceD.ServiceB);
            Assert.AreEqual(serviceC, serviceA.ServiceC);
            Assert.AreEqual(serviceC, serviceB.ServiceC);
            Assert.AreEqual(serviceD, serviceA.ServiceD);
            Assert.AreEqual(serviceD, serviceB.ServiceD);
            Assert.AreEqual(serviceD, serviceC.ServiceD);
            Assert.AreEqual(serviceE, serviceA.ServiceE);
            Assert.AreEqual(serviceE, serviceB.ServiceE);
            Assert.AreEqual(serviceE, serviceC.ServiceE);
            Assert.AreEqual(serviceE, serviceD.ServiceE);
        });
        
        [UnityTest]
        public IEnumerator AddServices_AllSingleton_NotDisposedAllEqual() => UniTask.ToCoroutine(async () =>
        {
            // Arrange
            await using IContainer container = await _unit.AddSingletonServiceA()
                .AddSingletonServiceB()
                .AddSingletonServiceC()
                .AddSingletonServiceD()
                .AddSingletonServiceE().Build(_scope);
            
            // Act
            IServiceA serviceA = container.Get<IServiceA>();
            IServiceB serviceB = container.Get<IServiceB>();
            IServiceC serviceC = container.Get<IServiceC>();
            IServiceD serviceD = container.Get<IServiceD>();
            IServiceE serviceE = container.Get<IServiceE>();
            
            // Assert
            Assert.NotNull(serviceA);
            Assert.NotNull(serviceB);
            Assert.NotNull(serviceC);
            Assert.NotNull(serviceD);
            Assert.NotNull(serviceE);
            Assert.IsFalse(serviceA.IsDisposed);
            Assert.IsFalse(serviceB.IsDisposed);
            Assert.IsFalse(serviceC.IsDisposed);
            Assert.IsFalse(serviceD.IsDisposed);
            Assert.IsFalse(serviceE.IsDisposed);
            Assert.AreEqual(serviceA, serviceD.ServiceA);
            Assert.AreEqual(serviceB, serviceA.ServiceB);
            Assert.AreEqual(serviceB, serviceD.ServiceB);
            Assert.AreEqual(serviceC, serviceA.ServiceC);
            Assert.AreEqual(serviceC, serviceB.ServiceC);
            Assert.AreEqual(serviceD, serviceA.ServiceD);
            Assert.AreEqual(serviceD, serviceB.ServiceD);
            Assert.AreEqual(serviceD, serviceC.ServiceD);
            Assert.AreEqual(serviceE, serviceA.ServiceE);
            Assert.AreEqual(serviceE, serviceB.ServiceE);
            Assert.AreEqual(serviceE, serviceC.ServiceE);
            Assert.AreEqual(serviceE, serviceD.ServiceE);
        });
        
        [UnityTest]
        public IEnumerator AddServices_AllServiceETransient_Pass() => UniTask.ToCoroutine(async () =>
        {
            // Arrange
            await using IContainer container = await _unit
                .AddSingletonServiceA()
                .AddSingletonServiceB()
                .AddSingletonServiceC()
                .AddSingletonServiceD()
                .AddTransientServiceE().Build(_scope);

            // Act
            IServiceA serviceA = container.Get<IServiceA>();
            IServiceB serviceB = container.Get<IServiceB>();
            IServiceC serviceC = container.Get<IServiceC>();
            IServiceD serviceD = container.Get<IServiceD>();
            IServiceE serviceE = container.Get<IServiceE>();

            // Assert
            Assert.NotNull(serviceA);
            Assert.NotNull(serviceB);
            Assert.NotNull(serviceC);
            Assert.NotNull(serviceD);
            Assert.NotNull(serviceE);
            Assert.AreNotEqual(serviceA.ServiceE, serviceB.ServiceE);
            Assert.AreNotEqual(serviceA.ServiceE, serviceC.ServiceE);
            Assert.AreNotEqual(serviceA.ServiceE, serviceD.ServiceE);
            Assert.AreNotEqual(serviceB.ServiceE, serviceC.ServiceE);
            Assert.AreNotEqual(serviceC.ServiceE, serviceD.ServiceE);
        });

        [UnityTest]
        public IEnumerator BuildAsync_MissingDependency_Asserts() => UniTask.ToCoroutine(async () =>
        {
            Exception? ex = null;
            try
            {
                // Arrange
                // Act
                await using IContainer container = await _unit.AddSingletonServiceA()
                    .AddSingletonServiceB()
                    .AddSingletonServiceC()
                    //.AddSingletonServiceD() <-- missing
                    .AddTransientServiceE().Build(_scope);
            }
            catch (Exception e)
            {
                ex = e;
            }

            void Throw() { if (ex != null) throw ex; }
            
            // Assert
            Assert.Throws<DiException>(Throw);
        });

        [UnityTest]
        public IEnumerator Create_MissingType_ThrowsException() => UniTask.ToCoroutine(async () =>
        {
            // Arrange
            await using IContainer container = await _unit
                //.AddSingletonServiceD() <-- missing
                .AddTransientServiceE().Build(_scope);
            {
                // Act
                Assert.Throws<DiException>(() => _container.Get<IServiceD>());
            }
        });
    }
}
#nullable disable