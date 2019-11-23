using Microsoft.Extensions.DependencyInjection;
using StKIoc.Internal;
using StKIoc.Internal.CircleDetect;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StKIoc
{
    /// <summary>
    /// 依赖关系容器
    /// </summary>
    public class TypeRelationCollection
    {
        /// <summary>
        /// 当前容器中类型，用于快速检索
        /// </summary>
        private HashSet<int> serviceTypes;

        /// <summary>
        /// 保存依赖类型
        /// </summary>
        private List<TypeRelation> typeRelations;

        /// <summary>
        /// 单例对象
        /// </summary>
        public static TypeRelationCollection Instance { get; internal set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="serviceDescriptors"></param>
        internal TypeRelationCollection(IServiceCollection serviceDescriptors)
        {
            TypeRelationCollection.Instance = this;
            typeRelations = new List<TypeRelation>();
            serviceTypes = new HashSet<int>();
            Init(serviceDescriptors);
        }

        /// <summary>
        /// 容器初始化
        /// </summary>
        /// <param name="serviceDescriptors"></param>
        private void Init(IServiceCollection serviceDescriptors)
        {
            foreach (var item in serviceDescriptors)
            {
                if (item.ImplementationInstance != null)
                {
                    typeRelations.Add(new TypeRelation(item.ServiceType, item.ImplementationInstance.GetType(), item.Lifetime, instance: item.ImplementationInstance));
                }
                else if (item.ImplementationFactory != null)
                {
                    typeRelations.Add(new TypeRelation(item.ServiceType,
                        item.ImplementationType ?? (item.ImplementationFactory.Target.GetType() == typeof(object) ? item.ServiceType : item.ImplementationFactory.Target.GetType()),
                        item.Lifetime, factory: item.ImplementationFactory));
                }
                else
                {
                    typeRelations.Add(new TypeRelation(item.ServiceType, item.ImplementationType, item.Lifetime));
                }
            }
        }

        /// <summary>
        /// hashset添加同样对象会报错，所以。。。
        /// </summary>
        /// <param name="serviceType"></param>
        private void TryAddServiceType(Type serviceType)
        {
            var code = serviceType.GetHashCode();
            if (this.serviceTypes.Contains(code) == false)
            {
                this.serviceTypes.Add(code);
            }
        }

        /// <summary>
        /// 根据serviceType获取依赖关系
        /// </summary>
        /// <param name="serviceType"></param>
        /// <returns></returns>
        internal TypeRelation GetRelation(Type serviceType)
        {
            var result = this.typeRelations.First(p => p.ServiceType == serviceType);
            return result;
        }

        /// <summary>
        /// 容器中是否包含serviceType的关系，包括泛型和IEnumerable情况
        /// </summary>
        /// <param name="serviceType"></param>
        /// <returns></returns>
        internal bool ContainsServiceTypeWithCheckGeneric(Type serviceType)
        {
            if (this.ContainsServiceType(serviceType))
            {
                return true;
            }
            if (serviceType.IsConstructedGenericType)
            {
                var outType = serviceType.GetGenericTypeDefinition();
                var inType = serviceType.GenericTypeArguments;
                if (outType == typeof(IEnumerable<>))
                {
                    return true;
                }
                else
                {
                    if (this.ContainsServiceType(outType))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            else
            {
                return false;
            }
        }
        /// <summary>
        /// 容器中是否包含serviceType的关系,不包括泛型和IEnumerable情况
        /// </summary>
        /// <param name="serviceType"></param>
        /// <returns></returns>
        internal bool ContainsServiceType(Type serviceType)
        {
            return this.serviceTypes.Contains(serviceType.GetHashCode());
        }

        internal List<TypeRelation> GetRelations(Type serviceType, Type[] genericParms, bool isDetect = false)
        {
            var relations = this.typeRelations.Where(_ =>
            {
                if (_.BuildFlag == false)
                {
                    return false;
                }
                if (_.ServiceType == serviceType)
                {
                    return true;
                }
                if (serviceType.IsConstructedGenericType)
                {
                    var outType = serviceType.GetGenericTypeDefinition();
                    if (_.ServiceType == outType)
                    {
                        return true;
                    }
                }
                return false;
            }).Select(p =>
            {
                if (p.ImplementType.IsGenericType &&
                p.ImplementType.ContainsGenericParameters)
                {
                    Type newType = null;
                    Type newServiceType = null;
                    if (genericParms == null)
                    {
                        newType = p.ImplementType.MakeGenericType(serviceType.GetGenericArguments());
                        newServiceType = serviceType;
                    }
                    else
                    {
                        newType = p.ImplementType.MakeGenericType(genericParms);
                        newServiceType = serviceType.MakeGenericType(genericParms);
                    }
                    var relation = new TypeRelation(newServiceType, newType, p.Lifetime);
                    if (isDetect == false)
                    {
                        this.TryAddServiceType(newServiceType);
                        this.typeRelations.Add(relation);
                    }
                    return relation;
                }
                else
                {
                    return p;
                }
            }).ToList();
            relations.Reverse();
            return relations;

        }
        /// <summary>
        /// 当前serviceType是否能通过组合泛型创建对象
        /// </summary>
        /// <param name="serviceType"></param>
        /// <returns></returns>
        internal bool CanMakeGeneric(Type serviceType)
        {
            var outType = serviceType.GetGenericTypeDefinition();
            if (this.ContainsServiceType(outType))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 根据serviceType创建新的依赖关系
        /// </summary>
        /// <param name="serviceType"></param>
        /// <param name="isDedect"></param>
        /// <returns></returns>
        internal TypeRelation MakeGeneric(Type serviceType, bool isDedect = false)
        {
            var outType = serviceType.GetGenericTypeDefinition();
            var relation = this.typeRelations.First(p => p.ServiceType == outType);
            var result = new TypeRelation(serviceType,
                relation.ImplementType.MakeGenericType(serviceType.GenericTypeArguments),
                relation.Lifetime);
            if (isDedect == false)
            {
                this.typeRelations.Insert(0, result);
                this.TryAddServiceType(serviceType);
            }
            return result;
        }

        /// <summary>
        /// 构造容器
        /// </summary>
        /// <returns></returns>
        public IServiceProvider Build()
        {
            var serviceProvider = new ServiceProvider(true);
            this.typeRelations.Add(new TypeRelation(typeof(IServiceProvider), typeof(ServiceProvider), ServiceLifetime.Singleton, instance: serviceProvider));
            this.typeRelations.Add(new TypeRelation(typeof(IServiceScope), typeof(ServiceScope), ServiceLifetime.Scoped));
            this.typeRelations.Add(new TypeRelation(typeof(IServiceScopeFactory), typeof(ServiceProvider), ServiceLifetime.Singleton, instance: serviceProvider));
            this.typeRelations.Add(new TypeRelation(typeof(ObjectMap), typeof(ObjectMap), ServiceLifetime.Singleton, instance: new ObjectMap()));


            typeRelations.ForEach(p => p.BuildFlag = true);
            typeRelations.Reverse();

            foreach (var relation in this.typeRelations)
            {
                var code = relation.ServiceType.GetHashCode();
                if (this.serviceTypes.Contains(code) == false)
                {
                    this.serviceTypes.Add(code);
                }
            }

            CheckCircleImplement();
            return serviceProvider;
        }

        /// <summary>
        /// 检查环形依赖
        /// </summary>
        private void CheckCircleImplement()
        {
            foreach (var relation in this.typeRelations)
            {
                var detector = new CircleDetector(relation);
                detector.Detect();
            }
        }
    }
}
