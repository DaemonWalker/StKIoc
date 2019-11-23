using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StKIoc.WebApiTest
{
    public interface IFoo { }
    public interface IBar { }
    class Foo : IFoo
    {
        public Foo(IBar bar) { }
        public Foo() { }
    }
    class Bar : IBar
    {
        public Bar(IFoo foo) { }
    }
}
