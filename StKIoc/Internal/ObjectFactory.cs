using StKIoc.Exceptions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;

namespace StKIoc.Internal
{
    class ObjectFactory
    {
        private IServiceProvider serviceProvider;
        private TypeRelationCollection typeRelations;
        public ObjectFactory(IServiceProvider serviceProvider)
        {
            this.typeRelations = TypeRelationCollection.Instance;
            this.serviceProvider = serviceProvider;
        }
        public object Get(TypeRelation relation)
        {
            if (relation.Instance != null)
            {
                return relation.Instance;
            }
            else if (relation.Factory != null)
            {
                return relation.Factory.Invoke(this.serviceProvider);
            }
            else
            {
                return this.Get(relation.ServiceType, relation.ImplementType);
            }
        }
        private object Get(Type serviceType, Type implementType)
        {
            if (this.typeRelations.ContainsServiceType(serviceType))
            {
                return CreateByConstructor(implementType);
            }
            else if (serviceType.IsConstructedGenericType)
            {
                if (serviceType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                {
                    return CreateIEnumerable(serviceType);
                }
                else
                {
                    return CreateGeneric(serviceType);
                }
            }
            else
            {
                throw new NotImplementedException();
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
            var parms = new List<object>();
            foreach (var constructor in constructors)
            {
                var fit = true;
                parms.Clear();
                foreach (var parm in constructor.GetParameters())
                {
                    var obj = this.serviceProvider.GetService(parm.ParameterType);
                    if (obj == null)
                    {
                        fit = false;
                        break;
                    }
                    else
                    {
                        parms.Add(obj);
                    }
                }
                if (fit)
                {
                    bestConstructor = constructor;
                    break;
                }
            }
            if (bestConstructor == null)
            {
                throw new NoFitConstructorException(implementType);
            }
            return bestConstructor.Invoke(parms.ToArray());
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
                var relations = this.typeRelations.GetGetRelations(newServiceType, parms);
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
