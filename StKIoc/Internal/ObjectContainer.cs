using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StKIoc.Internal
{
    class ObjectContainer : IDisposable
    {
        private const string SINGTONSTRING = "SINGLETON";
        private ConcurrentDictionary<string, object> objects;
        private readonly TypeRelationCollection typeRelations;
        private HashSet<string> recordObjects;
        private ObjectFactory objectFactory;
        private bool isRoot;
        private IServiceProvider serviceProvider;
        private bool disposed = false;
        private static ObjectContainer root;
        private List<string> objectIndex;
        public ObjectContainer(IServiceProvider serviceProvider, bool isRoot = false)
        {
            this.typeRelations = TypeRelationCollection.Instance;
            this.serviceProvider = serviceProvider;
            this.objectIndex = new List<string>();
            this.objectFactory = new ObjectFactory(serviceProvider);
            this.objects = new ConcurrentDictionary<string, object>();
            this.recordObjects = new HashSet<string>();
            this.isRoot = isRoot;
            if (isRoot)
            {
                ObjectContainer.root = this;
            }
        }
        public object Get(Type serviceType)
        {
            if (this.typeRelations.ContainsServiceType(serviceType))
            {
                return Get(this.typeRelations.GetRelation(serviceType));
            }
            else if (serviceType.IsGenericType && serviceType.IsConstructedGenericType)
            {
                if (serviceType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                {
                    return Get(new TypeRelation(serviceType, serviceType, ServiceLifetime.Transient));
                }
                else
                {
                    if (this.typeRelations.CanMakeGeneric(serviceType))
                    {
                        return Get(this.typeRelations.MakeGeneric(serviceType));
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            else
            {
                return null;
            }
        }
        public object Get(TypeRelation relation)
        {
            var key = relation.ID;
            if (relation.Lifetime == ServiceLifetime.Singleton)
            {
                return root.GetRecordObject(key, relation);
            }
            else if (relation.Lifetime == ServiceLifetime.Scoped)
            {
                return GetRecordObject(key, relation);
            }
            else
            {
                key = key + Guid.NewGuid().ToString().GetHashCode();
                var obj = this.objectFactory.Get(relation);
                if (this.objects.TryAdd(key, obj))
                {
                    this.objectIndex.Add(key);
                }
                return obj;
            }
        }
        private object GetRecordObject(string key, TypeRelation relation)
        {
            if (recordObjects.Contains(key))
            {
                return objects[key];
            }
            else
            {
                var obj = this.objectFactory.Get(relation);
                recordObjects.Add(key);
                if (objects.TryAdd(key, obj))
                {
                    this.objectIndex.Add(key);
                    return obj;
                }
                else
                {
                    return objects[key];
                }
            }
        }

        protected virtual void Dispose(bool dispoing)
        {
            if (this.disposed == false)
            {
                this.disposed = true;
                if (dispoing)
                {
                    for (int i = objectIndex.Count - 1; i >= 0; i--)
                    {
                        var value = this.objects[this.objectIndex[i]];
                        if (value is IDisposable disposable &&
                            value is IServiceProvider == false)
                        {
                            disposable.Dispose();
                        }
                    }
                }
            }

        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
