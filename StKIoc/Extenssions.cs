using StKIoc.Internal;
using System;
using System.Collections.Generic;
using System.Text;

namespace StKIoc
{
    public static class Extenssions
    {
        /// <summary>
        /// 根据依赖关系生成对象
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <param name="typeRelation"></param>
        /// <returns></returns>
        internal static object GetServiceByRelation(this IServiceProvider serviceProvider, TypeRelation typeRelation)
        {
            if (serviceProvider is ServiceProvider sp)
            {
                return sp.GetServiceByRelation(typeRelation);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 懒得写typeof 和 as 了...
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="serviceProvider"></param>
        /// <returns></returns>
        internal static T GetService<T>(this IServiceProvider serviceProvider)
        {
            var obj = serviceProvider.GetService(typeof(T));
            if (obj == null)
            {
                return default(T);
            }
            else
            {
                return (T)obj;
            }
        }

        /// <summary>
        /// 获取类型名称，用于异常信息输出
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        internal static string GetTypeName(this Type t) => t.FullName;
    }
}
