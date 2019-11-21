﻿using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace StKIoc.Internal
{
    internal class ServiceScope : IServiceScope, IDisposable
    {
        /// <summary>
        /// for dispose
        /// </summary>
        private bool disposed = false;

        /// <summary>
        /// from IServiceScope
        /// </summary>
        public IServiceProvider ServiceProvider { get; }

        public ServiceScope()
        {
            this.ServiceProvider = new ServiceProvider();
        }

        /// <summary>
        /// from IDisposable
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// call by IDisposable
        /// </summary>
        /// <param name="disposing"></param>
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
    }
}