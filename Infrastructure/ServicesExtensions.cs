using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Data;
using Domain.Interfaces;
using Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure
{
    public static  class ServicesExtensions
    {
        //public static IServiceCollection AddInfrastructure(
        //   this IServiceCollection services,
        //   IConfiguration configuration)
        //{
        //    services.AddDbContext<AppDbContext>(options =>
        //    options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        //    services.AddScoped<IOutboxService, OutboxService>();
        //    services.AddHostedService<OutboxBackgroundService>();

        //    return services;
        //}
    }
}
