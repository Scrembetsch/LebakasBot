using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenericUtil.Extensions
{
    public static class ServiceProviderExtension
    {
        public static T GetService<T>(this IServiceProvider services)
        {
            return (T)services.GetService(typeof(T));
        }
    }
}
