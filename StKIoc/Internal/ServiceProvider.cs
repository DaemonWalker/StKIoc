using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace StKIoc.Internal
{
    /// <summary>
    /// Service对象提供器
    /// </summary>
    internal class ServiceProvider : IServiceProvider, IServiceScopeFactory, ISupportRequiredService, IDisposable
    {
        /// <summary>
        /// 对象容器
        /// </summary>
        private ObjectContainer objectContainer;
        #region IDisposable
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
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="isRoot">根对象为true</param>
        public ServiceProvider(bool isRoot = false)
        {
            this.objectContainer = new ObjectContainer(this, isRoot);
        }

        /// <summary>
        /// 直接获取ImplementSerivce对象
        /// </summary>
        /// <param name="typeRelation"></param>
        /// <returns></returns>
        public object GetServiceByRelation(TypeRelation typeRelation)
        {
            return this.objectContainer.Get(typeRelation);
        }

        #region IServiceScopeFactory
        /// <summary>
        /// 创建作用域对象
        /// </summary>
        /// <returns></returns>
        public IServiceScope CreateScope()
        {
            return new ServiceScope();
        }
        #endregion

        #region ISupportRequiredService
        /// <summary>
        /// 同GetService
        /// </summary>
        /// <param name="serviceType"></param>
        /// <returns></returns>
        public object GetRequiredService(Type serviceType)
        {
            return this.objectContainer.Get(serviceType);
        }
        #endregion

        #region IServiceProvider
        /// <summary>
        /// 获取Type对应的对象
        /// </summary>
        /// <param name="serviceType"></param>
        /// <returns></returns>
        public object GetService(Type serviceType)
        {
            return this.objectContainer.Get(serviceType);
        }
        #endregion

    }
}
