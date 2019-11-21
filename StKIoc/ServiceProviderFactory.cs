using Microsoft.Extensions.DependencyInjection;
using StKIoc;
using System;
using System.Collections.Generic;
using System.Text;

namespace StKIoc
{
    public class ServiceProviderFactory : IServiceProviderFactory<TypeRelationCollection>
    {
        public TypeRelationCollection CreateBuilder(IServiceCollection services)
        {
            var collection = new TypeRelationCollection(services);
            TypeRelationCollection.Instance = collection;
            return collection;
        }

        public IServiceProvider CreateServiceProvider(TypeRelationCollection containerBuilder)
        {
            return containerBuilder.Build();
        }
    }
}
