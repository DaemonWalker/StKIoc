using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StKIoc.Internal
{
    /// <summary>
    /// 对象容器
    /// </summary>
    class ObjectContainer : IDisposable
    {
        /// <summary>
        /// 用于保存对象
        /// </summary>
        private ConcurrentDictionary<string, object> objects;
        /// <summary>
        /// 记录singleton(root中) scoped对象(所有)的key
        /// </summary>
        private HashSet<string> recordObjects;
        /// <summary>
        /// 记录对象生成顺序
        /// ConcurrentDictionary在foreach或者是ToList()的顺序是不是插入的顺序
        /// </summary>
        private List<string> objectIndex;

        /// <summary>
        /// 用于查找对照关系
        /// </summary>
        private readonly TypeRelationCollection typeRelations;
        /// <summary>
        /// 用于生成对象
        /// </summary>
        private ObjectFactory objectFactory;

        /// <summary>
        /// 根容器，用于保存Singleton对象
        /// </summary>
        private static ObjectContainer root;
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <param name="isRoot"></param>
        public ObjectContainer(IServiceProvider serviceProvider, bool isRoot = false)
        {
            this.typeRelations = TypeRelationCollection.Instance;
            this.objectIndex = new List<string>();
            this.objectFactory = new ObjectFactory(serviceProvider);
            this.objects = new ConcurrentDictionary<string, object>();
            this.recordObjects = new HashSet<string>();
            if (isRoot)
            {
                ObjectContainer.root = this;
            }
        }
        /// <summary>
        /// 获取service type对象
        /// </summary>
        /// <param name="serviceType"></param>
        /// <returns></returns>
        public object Get(Type serviceType)
        {
            //如果存在对照关系则按照关系返回对象
            if (this.typeRelations.ContainsServiceType(serviceType))
            {
                return Get(this.typeRelations.GetRelation(serviceType));
            }
            //如果没找到则看是否存在泛型
            else if (serviceType.IsGenericType && serviceType.IsConstructedGenericType)
            {
                //如果是IEnumerable<T>则返回所有对照关系对象
                if (serviceType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                {
                    return Get(new TypeRelation(serviceType, serviceType, ServiceLifetime.Transient));
                }
                //如果请求一个IFoo<Bar>对象，但是只有IFoo<> -> Foo<>的对照关系，则生成一个Foo<Bar>对象
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
            //没有则抛出异常
            else
            {
                return null;
            }
        }
        /// <summary>
        /// 根据对照关系获取对象
        /// </summary>
        /// <param name="relation"></param>
        /// <returns></returns>
        public object Get(TypeRelation relation)
        {
            var key = relation.ID;
            //如果是Singleton对象则直接在根容器中查找
            if (relation.Lifetime == ServiceLifetime.Singleton)
            {
                return root.GetRecordObject(key, relation);
            }
            //Scoped容器在当前容器中查找
            else if (relation.Lifetime == ServiceLifetime.Scoped)
            {
                return GetRecordObject(key, relation);
            }
            //Transient对象直接生成
            else
            {
                //随便生成一个Key避免重复
                key = key + Guid.NewGuid().ToString().GetHashCode();
                var obj = this.objectFactory.Get(relation);
                if (this.objects.TryAdd(key, obj))
                {
                    this.objectIndex.Add(key);
                }
                return obj;
            }
        }
        /// <summary>
        /// 在容器中根据对照关系获取对象
        /// </summary>
        /// <param name="key"></param>
        /// <param name="relation"></param>
        /// <returns></returns>
        private object GetRecordObject(string key, TypeRelation relation)
        {
            //如果存在则直接返回
            if (recordObjects.Contains(key))
            {
                return objects[key];
            }
            //不存在则生成一个
            else
            {
                var obj = this.objectFactory.Get(relation);
                recordObjects.Add(key);
                if (objects.TryAdd(key, obj))
                {
                    this.objectIndex.Add(key);
                    return obj;
                }
                //如果添加失败说明其他线程已经添加了对象(因为key唯一)
                //直接返回容器中的对象
                else
                {
                    return objects[key];
                }
            }
        }
        #region IDisposable
        /// <summary>
        /// 记录对象是否已经Dispose
        /// </summary>
        private bool disposed = false;
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
        #endregion
    }
}
