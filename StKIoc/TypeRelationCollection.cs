using Microsoft.Extensions.DependencyInjection;
using StKIoc.Exceptions;
using StKIoc.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StKIoc
{
    public class TypeRelationCollection
    {
        private HashSet<int> serviceTypes;
        private List<TypeRelation> typeRelations;
        public static TypeRelationCollection Instance { get; internal set; }
        public TypeRelationCollection(IServiceCollection serviceDescriptors)
        {
            typeRelations = new List<TypeRelation>();
            serviceTypes = new HashSet<int>();
            Init(serviceDescriptors);
        }
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
        private void TryAddServiceType(Type serviceType)
        {
            var code = serviceType.GetHashCode();
            if (this.serviceTypes.Contains(code) == false)
            {
                this.serviceTypes.Add(code);
            }
        }
        internal TypeRelation GetRelation(Type serviceType)
        {
            var result = this.typeRelations.FirstOrDefault(p => p.ServiceType == serviceType);
            if (result == default(TypeRelation))
            {
                throw new ImplementTypeNotFoundException(serviceType);
            }
            else
            {
                return result;
            }
        }
        internal bool ContainsServiceType(TypeRelation relation)
        {
            return this.ContainsServiceType(relation.ServiceType);
        }
        internal bool ContainsServiceType(Type serviceType)
        {
            return this.serviceTypes.Contains(serviceType.GetHashCode());
        }

        internal List<TypeRelation> GetGetRelations(Type serviceType, Type[] genericParms)
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
                    this.TryAddServiceType(newServiceType);
                    var relation = new TypeRelation(newServiceType, newType, p.Lifetime);
                    this.typeRelations.Add(relation);
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
        internal TypeRelation MakeGeneric(Type serviceType)
        {
            var outType = serviceType.GetGenericTypeDefinition();
            var relation = this.typeRelations.First(p => p.ServiceType == outType);
            var result = new TypeRelation(serviceType,
                relation.ImplementType.MakeGenericType(serviceType.GenericTypeArguments),
                relation.Lifetime);
            this.typeRelations.Insert(0, result);
            this.TryAddServiceType(serviceType);
            return result;
        }

        public IServiceProvider Build()
        {
            var serviceProvider = new ServiceProvider(true);
            this.typeRelations.Add(new TypeRelation(typeof(IServiceProvider), typeof(ServiceProvider), ServiceLifetime.Singleton, instance: serviceProvider));
            this.typeRelations.Add(new TypeRelation(typeof(IServiceScope), typeof(ServiceScope), ServiceLifetime.Scoped));
            this.typeRelations.Add(new TypeRelation(typeof(IServiceScopeFactory), typeof(ServiceProvider), ServiceLifetime.Singleton, instance: serviceProvider));

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

            return serviceProvider;
        }
    }
}
