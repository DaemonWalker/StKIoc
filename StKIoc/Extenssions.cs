using StKIoc.Internal;
using System;
using System.Collections.Generic;
using System.Text;

namespace StKIoc
{
    public static class Extenssions
    {
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
    }
}
