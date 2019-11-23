using Microsoft.Extensions.DependencyInjection;
using StKIoc;
using System;
using System.Collections.Generic;
using System.Text;

namespace StKIoc
{
    public class ServiceProviderFactory : IServiceProviderFactory<TypeRelationCollection>
    {
        /// <summary>
        /// 根据依赖关系生成容器
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public TypeRelationCollection CreateBuilder(IServiceCollection services)
        {
            var collection = new TypeRelationCollection(services);
            return collection;
        }

        /// <summary>
        /// 生成根ServiceProvider
        /// </summary>
        /// <param name="containerBuilder"></param>
        /// <returns></returns>
        public IServiceProvider CreateServiceProvider(TypeRelationCollection containerBuilder)
        {
            return containerBuilder.Build();
        }
    }
}
