using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace StKIoc.Internal
{
    /// <summary>
    /// 对象映射关系辅助类
    /// </summary>
    class ObjectMap
    {
        /// <summary>
        /// 全局的对象关系容器
        /// </summary>
        private TypeRelationCollection typeRelations;

        /// <summary>
        /// 构造函数
        /// </summary>
        public ObjectMap()
        {
            this.typeRelations = TypeRelationCollection.Instance;
        }

        /// <summary>
        /// 当前依赖关系能否创建实例
        /// </summary>
        /// <param name="relation"></param>
        /// <returns></returns>
        public bool CanCreateInstance(TypeRelation relation)
        {
            if (relation.CanSelfBuild)
            {
                return true;
            }
            return CanCallConstructor(relation.ImplementType);
        }

        /// <summary>
        /// 当前serviceType能否创建实例
        /// </summary>
        /// <param name="serviceType"></param>
        /// <returns></returns>
        public bool CanCreateInstance(Type serviceType)
        {
            if (typeRelations.ContainsServiceType(serviceType))
            {
                return CanCreateInstance(this.typeRelations.GetRelation(serviceType));
            }
            else if (serviceType.IsConstructedGenericType)
            {
                var outType = serviceType.GetGenericTypeDefinition();
                var inType = serviceType.GetGenericArguments();
                if (outType == typeof(IEnumerable<>))
                {
                    return true;
                }
                else
                {
                    if (this.typeRelations.CanMakeGeneric(serviceType))
                    {
                        var newRelation = this.typeRelations.MakeGeneric(serviceType);
                        return CanCallConstructor(newRelation.ImplementType);
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
        /// 当前构造函数能否创建实例
        /// </summary>
        /// <param name="serviceType"></param>
        /// <returns></returns>
        public bool CanCallConstructor(Type implementType)
        {
            var constructors = implementType.GetConstructors(BindingFlags.Public | BindingFlags.Instance).OrderByDescending(p => p.GetParameters().Length);
            foreach (var item in constructors)
            {
                var canCall = true;
                foreach (var parm in item.GetParameters())
                {
                    if (CanCreateInstance(parm.ParameterType) == false)
                    {
                        canCall = false;
                        break;
                    }
                }
                if (canCall)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
