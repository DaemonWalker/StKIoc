using System;
using System.Collections.Generic;
using System.Text;

namespace StKIoc.Exceptions
{
    public class NoFitConstructorException : StKIocExceptionBase
    {
        private Type implementType;
        public NoFitConstructorException(Type implementType)
        {
            this.implementType = implementType;
        }
        public override string Message => $"Can't find a fit constructor to create instance for {implementType.Name}";
    }
}
