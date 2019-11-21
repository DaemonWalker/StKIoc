using System;
using System.Collections.Generic;
using System.Text;

namespace StKIoc.Exceptions
{
    public class StKIocExceptionBase : Exception
    {
        public override string ToString() => this.Message;
    }
}
