using System;
using System.Collections.Generic;
using System.Text;

namespace StKIoc.Exceptions
{
    public class ImplementTypeNotFoundException : StKIocExceptionBase
    {
        private Type serviceType;
        public ImplementTypeNotFoundException(Type serviceType)
        {
            this.serviceType = serviceType;
        }
        public override string Message => $"There's no implement type for {this.serviceType}";
    }
}
