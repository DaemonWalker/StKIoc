using System;
using System.Collections.Generic;
using System.Text;

namespace StKIoc
{
    class Utils
    {
        /// <summary>
        /// 抛出环形依赖异常
        /// </summary>
        /// <param name="type1"></param>
        /// <param name="type2"></param>
        public static void ThrowCircularDependencyException(Type type1,Type type2)=>
            throw new InvalidOperationException($"A circular dependency was detected for the service of type {type1.GetTypeName()} and type {type2.GetTypeName()}");

        /// <summary>
        /// 抛出没有找到适合构造函数异常
        /// </summary>
        /// <param name="type"></param>
        public static void ThrowNoFitConstructorException(Type type) =>
            throw new InvalidOperationException($"Can't find a fit constructor to create instance for {type.GetTypeName()}");

    }
}
