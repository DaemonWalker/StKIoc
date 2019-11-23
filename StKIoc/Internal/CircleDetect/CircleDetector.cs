using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace StKIoc.Internal.CircleDetect
{
    /// <summary>
    /// 循环引用检测
    /// </summary>
    class CircleDetector
    {
        /// <summary>
        /// 用于外部调用初始化
        /// </summary>
        /// <param name="typeRelation"></param>
        public CircleDetector(TypeRelation typeRelation)
        {
            this.typeRelation = typeRelation;
            this.refrenceModel = new RefrenceModel();
            typeRelations = TypeRelationCollection.Instance;
        }

        /// <summary>
        /// 用于内部递归使用
        /// </summary>
        /// <param name="typeRelation"></param>
        /// <param name="circleDetector"></param>
        private CircleDetector(TypeRelation typeRelation, CircleDetector circleDetector)
        {
            this.typeRelation = typeRelation;
            typeRelations = TypeRelationCollection.Instance;
            this.refrenceModel = new RefrenceModel();
            this.refrenceModel.Attach(circleDetector.refrenceModel);
        }

        /// <summary>
        /// 当前检测依赖关系
        /// </summary>
        private TypeRelation typeRelation;
        private RefrenceModel refrenceModel;
        private TypeRelationCollection typeRelations;
        public void Detect()
        {
            this.CheckRelation();
        }

        /// <summary>
        /// 检测依赖关系
        /// </summary>
        /// <returns></returns>
        private bool CheckRelation()
        {
            //如果当前依赖关系存在实例对象或是工厂方法则不检测
            if (this.typeRelation.CanSelfBuild)
            {
                return true;
            }
            //如果需要通过构造函数创建，则进行检测
            else
            {
                //如果是调用方输入的依赖关系，并且当前接口的默认依赖关系为当前关系，则把当前关系加入循环检测
                if (this.typeRelation.BuildFlag == false ||
                    this.typeRelations.GetRelation(this.typeRelation.ServiceType)?.ImplementType == this.typeRelation.ImplementType)
                {
                    this.refrenceModel.Add(this.typeRelation.ServiceType, new List<Type>());
                }
                return this.CheckImplementType(this.typeRelation.ImplementType);
            }
        }

        /// <summary>
        /// 检测serviceType合法性
        /// </summary>
        /// <param name="serviceType"></param>
        /// <param name="circleDetector"></param>
        /// <returns></returns>
        private bool CheckServiceType(Type serviceType, out CircleDetector circleDetector)
        {
            if (this.typeRelations.ContainsServiceType(serviceType))
            {
                var relation = this.typeRelations.GetRelation(serviceType);
                circleDetector = new CircleDetector(relation, this);
                return circleDetector.CheckRelation();
            }
            else if (serviceType.IsConstructedGenericType)
            {
                var outType = serviceType.GetGenericTypeDefinition();
                var inType = serviceType.GenericTypeArguments;
                if (outType == typeof(IEnumerable<>))
                {
                    var relations = this.typeRelations.GetRelations(serviceType, null, true);
                    var list = new List<CircleDetector>();
                    foreach (var relation in relations)
                    {
                        var detector = new CircleDetector(relation, this);
                        if (detector.CheckRelation() == false)
                        {
                            circleDetector = null;
                            return false;
                        }
                        list.Add(detector);
                    }
                    var tempDetector = new CircleDetector(this.typeRelation, this);
                    list.ForEach(p => tempDetector.refrenceModel.Attach(p.refrenceModel));
                    circleDetector = tempDetector;
                    return true;
                }
                else
                {
                    if (this.typeRelations.CanMakeGeneric(serviceType))
                    {
                        var newRelation = this.typeRelations.MakeGeneric(serviceType, true);
                        circleDetector = new CircleDetector(newRelation, this);
                        return circleDetector.CheckRelation();
                    }
                    else
                    {
                        circleDetector = null;
                        return false;
                    }
                }
            }
            else
            {
                circleDetector = null;
                return false;
            }
        }
        /// <summary>
        /// 检测ImplementType合法性
        /// </summary>
        /// <param name="implementType"></param>
        /// <returns></returns>
        private bool CheckImplementType(Type implementType)
        {
            var constructors = implementType.GetConstructors(BindingFlags.Public | BindingFlags.Instance).OrderByDescending(p => p.GetParameters().Length);
            var detectors = new List<CircleDetector>();
            foreach (var item in constructors)
            {
                var list = new List<CircleDetector>();
                var checkResult = true;
                if (item.GetParameters().Any(p => this.typeRelations.ContainsServiceTypeWithCheckGeneric(p.ParameterType)) == false)
                {
                    continue;
                }
                foreach (var parm in item.GetParameters())
                {
                    this.refrenceModel.Check(this.typeRelation.ServiceType, parm.ParameterType);
                    if (this.CheckServiceType(parm.ParameterType, out var detector) == false)
                    {
                        checkResult = false;
                        break;
                    }
                    else
                    {
                        list.Add(detector);
                    }
                }
                if (checkResult)
                {
                    list.ForEach(p =>
                    {
                        this.refrenceModel.Attach(p.refrenceModel);
                    });
                    return true;
                }
            }
            return false;
        }
    }
}
