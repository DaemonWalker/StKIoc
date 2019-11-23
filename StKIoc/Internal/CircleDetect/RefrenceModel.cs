using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StKIoc.Internal.CircleDetect
{
    /// <summary>
    /// 调用链模型
    /// Dictionary的 Key为需要创建实例的类型 Value为创建Key实例所需要的类型
    /// 如果在Value中出现了Key的类型，则视为出现了循环引用
    /// </summary>
    class RefrenceModel
    {
        /// <summary>
        /// 记录对象创建所需要的类型字典
        /// </summary>
        Dictionary<Type, List<Type>> refMap = new Dictionary<Type, List<Type>>();

        /// <summary>
        /// 添加记录
        /// </summary>
        /// <param name="type"></param>
        /// <param name="refTypes"></param>
        public void Add(Type type, List<Type> refTypes)
        {
            if (refMap.ContainsKey(type))
            {
                refMap[type] = refMap[type].Union(refTypes).ToList();
            }
            else
            {
                refMap.Add(type, refTypes);
            }
        }

        /// <summary>
        /// 外部监查
        /// </summary>
        /// <param name="constructType">构建类型（用于输出异常信息）</param>
        /// <param name="refType">参数类型</param>
        public void Check(Type constructType,Type refType)
        {
            //如果当前引用类在构造链中存在则出现环形引用 抛出异常
            if (this.refMap.Keys.Contains(refType))
            {
                Utils.ThrowCircularDependencyException(constructType, refType);
            }
        }

        /// <summary>
        /// 当一个调用链附加到当前调用链上
        /// </summary>
        /// <param name="refModel"></param>
        public void Attach(RefrenceModel refModel)
        {
            foreach (var kv in refModel.refMap)
            {
                this.Add(kv.Key, kv.Value);
            }
        }
    }
}
