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
    public class SelfTest
    {
        protected IServiceProvider CreateServiceProvider(IServiceCollection serviceCollection)
        {
            var factory = new ServiceProviderFactory();
            return factory.CreateServiceProvider(factory.CreateBuilder(serviceCollection));
        }
        [Fact]
        public void CircleDependencyTest()
        {
            var collection = new TestServiceCollection();
            collection.AddSingleton<IFoo, Foo>();
            collection.AddSingleton<IBar, Bar>();
            Assert.Throws<InvalidOperationException>(() => CreateServiceProvider(collection));

        }
        [Fact]
        public void NoImplementTest()
        {
            var collection = new TestServiceCollection();
            var provider = CreateServiceProvider(collection);
            Assert.Null(provider.GetService<IFoo>());
        }
        [Fact]
        public void CircleDependencyTest2()
        {
            var collection = new TestServiceCollection();
            collection.AddSingleton<IFoo>(new Foo());
            collection.AddSingleton<IBar, Bar>();
            var serviceProvider = CreateServiceProvider(collection);
            var obj = serviceProvider.GetService<IBar>();
            Assert.NotNull(obj);
        }

    }

}
