using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace StKIoc.Internal
{
    /// <summary>
    /// 对象依赖关系
    /// </summary>
    class TypeRelation
    {
        /// <summary>
        /// 源类型
        /// </summary>
        public Type ServiceType { get; }

        /// <summary>
        /// 目标类型
        /// </summary>
        public Type ImplementType { get; }

        /// <summary>
        /// 生命周期
        /// </summary>
        public ServiceLifetime Lifetime { get; }

        /// <summary>
        /// 实例对象
        /// </summary>
        public object Instance { get; }

        /// <summary>
        /// 工厂方法
        /// </summary>
        public Func<IServiceProvider, object> Factory { get; }

        /// <summary>
        /// 用户输入还是后面框架生成的
        /// 主要是区分在获取IEnumerable<>对象时添加的关系
        /// </summary>
        public bool BuildFlag { get; set; }

        /// <summary>
        /// 用于Single和Scoped类型对象获取
        /// </summary>
        public string ID { get; }

        /// <summary>
        /// 当前关系能否直接生成
        /// </summary>
        public bool CanSelfBuild => this.Instance != null || this.Factory != null;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="service"></param>
        /// <param name="implement"></param>
        /// <param name="lifetime"></param>
        /// <param name="instance"></param>
        /// <param name="factory"></param>
        public TypeRelation(Type service, Type implement, ServiceLifetime lifetime, object instance = null, Func<IServiceProvider, object> factory = null)
        {
            this.ServiceType = service;
            this.ImplementType = implement;
            this.Instance = instance;
            this.Factory = factory;
            this.Lifetime = lifetime;
            this.ID = Guid.NewGuid().ToString("N");
        }
    }
}
