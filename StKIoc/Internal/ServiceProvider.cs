using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace StKIoc.Internal
{
    internal class ServiceProvider : IServiceProvider, IServiceScopeFactory, ISupportRequiredService, IDisposable
    {
        private ObjectContainer objectContainer;
        private bool disposed = false;
        private void Dispose(bool dispoing)
        {
            if (disposed == false)
            {
                this.disposed = true;
                if (dispoing)
                {
                    this.objectContainer.Dispose();
                }
            }
        }
        public ServiceProvider(bool isRoot = false)
        {
            this.objectContainer = new ObjectContainer(this, isRoot);
        }
        public object GetServiceByRelation(TypeRelation typeRelation)
        {
            return this.objectContainer.Get(typeRelation);
        }
        public IServiceScope CreateScope()
        {
            return new ServiceScope();
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        public object GetRequiredService(Type serviceType)
        {
            return this.objectContainer.Get(serviceType);
        }

        public object GetService(Type serviceType)
        {
            return this.objectContainer.Get(serviceType);
        }
    }
}
