using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Specification.Fakes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace StKIoc.Test
{
    public class CopyTest
    {
        protected IServiceProvider CreateServiceProvider(IServiceCollection serviceCollection)
        {
            var factory = new ServiceProviderFactory();
            return factory.CreateServiceProvider(factory.CreateBuilder(serviceCollection));
        }
        [Fact]
        public void ResolvesMixedOpenClosedGenericsAsEnumerable()
        {
            // Arrange
            var serviceCollection = new TestServiceCollection();
            var instance = new FakeOpenGenericService<PocoClass>(null);

            serviceCollection.AddTransient<PocoClass, PocoClass>();
            serviceCollection.AddSingleton(typeof(IFakeOpenGenericService<PocoClass>), typeof(FakeService));
            serviceCollection.AddSingleton(typeof(IFakeOpenGenericService<>), typeof(FakeOpenGenericService<>));
            serviceCollection.AddSingleton<IFakeOpenGenericService<PocoClass>>(instance);

            var serviceProvider = CreateServiceProvider(serviceCollection);

            var enumerable = serviceProvider.GetService<IEnumerable<IFakeOpenGenericService<PocoClass>>>().ToArray();

            // Assert
            Assert.Equal(3, enumerable.Length);
            Assert.NotNull(enumerable[0]);
            Assert.NotNull(enumerable[1]);
            Assert.NotNull(enumerable[2]);

            Assert.Equal(instance, enumerable[2]);
            Assert.IsType<FakeService>(enumerable[0]);
        }
    }
    class TestServiceCollection : List<ServiceDescriptor>, IServiceCollection
    {
        public TestServiceCollection AddScoped(Type tSource, Type tTarget)
        {
            this.Add(new ServiceDescriptor(tSource, tTarget, ServiceLifetime.Scoped));
            return this;
        }
    }
    public class FakeOpenGenericService<TVal> : IFakeOpenGenericService<TVal>
    {
        static int idx = 0;
        public int Idx { get; }
        public FakeOpenGenericService(TVal value)
        {
            Value = value;
            this.Idx = idx++;
        }

        public TVal Value { get; }
        public override string ToString()
        {
            return Idx.ToString();
        }
    }
}
