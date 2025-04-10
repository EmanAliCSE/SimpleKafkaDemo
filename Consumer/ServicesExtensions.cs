using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Data;
using Domain.Interfaces;
using Infrastructure.Services;
using KafkaWebApiDemo.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure
{
    public static  class ServicesExtensions
    {
        public static IServiceCollection AddConsumerServices(
           this IServiceCollection services,
           IConfiguration configuration)
        {
           services.AddHostedService<KafkaConsumerService>();

            return services;
        }
    }
}
