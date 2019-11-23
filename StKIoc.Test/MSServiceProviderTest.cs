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
    public class MSServiceProviderTest
    {
        protected IServiceProvider CreateServiceProvider(IServiceCollection serviceCollection)
        {
            var factory = new DefaultServiceProviderFactory();
            return factory.CreateServiceProvider(factory.CreateBuilder(serviceCollection));
        }
        [Fact]
        public void NoImplemenetTest()
        {
            var collection = new TestServiceCollection();
            collection.AddSingleton<IFoo, Foo>();
            collection.AddSingleton<IBar, Bar>();
            Assert.Throws<InvalidOperationException>(() =>
            {
               var serviceProvider= CreateServiceProvider(collection);
                serviceProvider.GetService<IFoo>();
            });

        }
        [Fact]
        public void NoConstructor()
        {
            var collection = new TestServiceCollection();
            collection.AddSingleton<FakeService>();
            var serviceProvider = CreateServiceProvider(collection);
            var obj = serviceProvider.GetService<FakeService>();

        }
        [Fact]
        public void ConstructorSwitch()
        {
            var collection = new TestServiceCollection();
            collection.AddSingleton<IFakeService,FakeService>();
            collection.AddSingleton<IBar, Bar>();
            var serviceProvider = CreateServiceProvider(collection);
            var obj = serviceProvider.GetService<IFakeService>();
            Assert.NotNull(obj);
        }
        [Fact]
        public void CircleTest2()
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
