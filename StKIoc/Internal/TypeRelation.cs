using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace StKIoc.Internal
{
    class TypeRelation
    {
        public Type ServiceType { get; }
        public Type ImplementType { get; }
        public object Instance { get; }
        public Func<IServiceProvider, object> Factory { get; }
        public ServiceLifetime Lifetime { get; }
        public bool BuildFlag { get; set; }
        public string ID { get; }
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
