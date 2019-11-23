using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace StKIoc.Test
{
    class TestServiceCollection : List<ServiceDescriptor>, IServiceCollection
    {
        public TestServiceCollection AddScoped(Type tSource, Type tTarget)
        {
            this.Add(new ServiceDescriptor(tSource, tTarget, ServiceLifetime.Scoped));
            return this;
        }
    }
    interface IFoo { }
    interface IBar { }
    class Foo : IFoo
    {
        public Foo(IBar bar) { }
        public Foo() { }
    }
    class Bar : IBar
    {
        public Bar(IFoo foo) { }
        public Bar() { }
    }
    class FakeService : IFakeService
    {
        private IFoo foo;
        private IBar bar;
        public FakeService(IFoo foo) => this.foo = foo;
        public FakeService(IBar bar) => this.bar = bar;
        public FakeService() { }
        public string GetMsg => $"{foo?.GetType()} {bar?.GetType()}";
    }
    interface IFakeService
    {
        string GetMsg { get; }
    }
}
