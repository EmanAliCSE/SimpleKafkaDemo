using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Application
{
    public static class ApplicationServiceExtensions
    {
        public static IServiceCollection AddApplicationServiceExtensions(
          this IServiceCollection services)
        {
            services.AddMediatR(Assembly.GetAssembly(typeof(IApplication)));

            services.AddAutoMapper(typeof(Mappings.MappingProfile));
           // services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

            return services;
        }
    }
}
