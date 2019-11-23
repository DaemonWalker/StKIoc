using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace StKIoc.Internal
{
    /// <summary>
    /// 对象生成工厂
    /// </summary>
    class ObjectFactory
    {
        /// <summary>
        /// 用于Factory生成对象
        /// </summary>
        private IServiceProvider serviceProvider;
        /// <summary>
        /// 对应关系集合
        /// </summary>
        private TypeRelationCollection typeRelations;
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="serviceProvider"></param>
        public ObjectFactory(IServiceProvider serviceProvider)
        {
            this.typeRelations = TypeRelationCollection.Instance;
            this.serviceProvider = serviceProvider;
        }
        /// <summary>
        /// 根据对应关系获取实体
        /// </summary>
        /// <param name="relation"></param>
        /// <returns></returns>
        public object Get(TypeRelation relation)
        {
            //如果注册时提供了对象则直接适用对象
            if (relation.Instance != null)
            {
                return relation.Instance;
            }
            //如果注册时提供了Func<ISeriveProvider,object>用于生成对象则调用Func
            else if (relation.Factory != null)
            {
                return relation.Factory.Invoke(this.serviceProvider);

            }
            //如果在对应关系中存在serviceType则根据构造函数生成对象
            else if (this.typeRelations.ContainsServiceType(relation.ServiceType))
            {
                return CreateByConstructor(relation.ImplementType);
            }
            //检查泛型情况
            else if (relation.ServiceType.IsConstructedGenericType)
            {
                if (relation.ServiceType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                {
                    return CreateIEnumerable(relation.ServiceType);
                }
                else
                {
                    return CreateGeneric(relation.ServiceType);
                }
            }
            //暂时没想到这种情况，先扔个异常
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 使用构造函数实例化对象
        /// </summary>
        /// <param name="implementType"></param>
        /// <returns></returns>
        private object CreateByConstructor(Type implementType)
        {
            ConstructorInfo bestConstructor = null;

            //使用能找到最多参数的构造函数进行实例化
            var constructors = implementType.GetConstructors().Where(p => p.IsPublic).OrderByDescending(p => p.GetParameters().Length).ToArray();
            object[] parms = null;
            foreach (var constructor in constructors)
            {
                var fit = true;
                foreach (var parm in constructor.GetParameters())
                {
                    if (this.serviceProvider.GetService<ObjectMap>().CanCreateInstance(parm.ParameterType) == false)
                    {
                        fit = false;
                        break;
                    }
                }
                if (fit)
                {
                    bestConstructor = constructor;
                    parms = bestConstructor.GetParameters().Select(p => this.serviceProvider.GetService(p.ParameterType)).ToArray();
                    break;
                }
            }
            if (bestConstructor == null)
            {
               Utils.ThrowNoFitConstructorException(implementType);
            }
            return bestConstructor.Invoke(parms);
        }

        /// <summary>
        /// 创建IEnumerable对象
        /// </summary>
        /// <param name="serviceType"></param>
        /// <returns></returns>
        private IEnumerable CreateIEnumerable(Type serviceType)
        {
            var outType = serviceType.GetGenericTypeDefinition();
            //IEnumerable只可能有一个泛型参数
            var inType = serviceType.GenericTypeArguments.First();
            var newType = typeof(List<>).MakeGenericType(inType);
            var list = Activator.CreateInstance(newType) as IList;
            Type newServiceType = null;
            Type[] parms = null;
            //如果能直接找到inType
            if (this.typeRelations.ContainsServiceType(inType))
            {
                newServiceType = inType;
            }
            //如果通过组合泛型能找到inType
            else if (inType.IsGenericType && this.typeRelations.ContainsServiceType(inType.GetGenericTypeDefinition()))
            {
                newServiceType = inType.GetGenericTypeDefinition();
                parms = inType.GetGenericArguments();
            }
            //如果在ioc内能找到对应inType
            if (newServiceType != null)
            {
                var relations = this.typeRelations.GetRelations(newServiceType, parms);
                foreach (var relation in relations)
                {
                    list.Add(this.serviceProvider.GetServiceByRelation(relation));
                }
            }
            return list;
        }
        /// <summary>
        /// 构建泛型对象
        /// </summary>
        /// <param name="serviceType"></param>
        /// <returns></returns>
        private object CreateGeneric(Type serviceType)
        {
            if (this.typeRelations.CanMakeGeneric(serviceType))
            {
                var relation = this.typeRelations.MakeGeneric(serviceType);
                return this.CreateByConstructor(relation.ImplementType);
            }
            else
            {
                Debug.WriteLine($"Make generictype failed {serviceType.FullName}");
                return null;
            }
        }

    }
}
