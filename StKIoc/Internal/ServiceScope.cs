using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace StKIoc.Internal
{
    internal class ServiceScope : IServiceScope, IDisposable
    {
        public ServiceScope()
        {
            //给当前域创建一个ServiceProvider
            this.ServiceProvider = new ServiceProvider();
        }
        #region IServiceScope
        public IServiceProvider ServiceProvider { get; }
        #endregion
        #region IDisposable
        private bool disposed = false;


        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed == false)
            {
                disposed = true;
                if (disposing)
                {
                    (this.ServiceProvider as IDisposable)?.Dispose();
                }
            }
        }
        ~ServiceScope()
        {
            this.Dispose(false);
        }
        #endregion



    }
}
