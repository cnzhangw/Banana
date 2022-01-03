#if NETCOREAPP || NETSTANDARD2_0
using Banana;
using Banana.Repository.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Reflection;
using static Banana.Repository.RepositoryExtension;

namespace Banana.Repository
{
    public static class DependencyInjection
    {
        /// <summary>
        /// 注册仓储
        /// </summary>
        /// <param name="services"></param>
        /// <param name="setup"></param>
        /// <returns></returns>
        public static IServiceCollection AddKogelRepository(this IServiceCollection services, Action<RepositoryOptionsBuilder> setup)
        {
            services.AddTransient((x) =>
            {
                var options = new RepositoryOptionsBuilder();
                setup.Invoke(options);
                return options;
            });
            services.AddTransient(typeof(IRepository<>), typeof(BaseRepositoryExtension<>));
            return services;
        }
    }
}
#endif