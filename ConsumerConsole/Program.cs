

using Domain.Data;
using KafkaWebApiDemo.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

class Program

{

    static async Task Main(string[] args)

    {

        var host = Host.CreateDefaultBuilder()

            .ConfigureServices((context, services) =>

            {

                services.AddDbContext<AppDbContext>(options =>

                        options.UseSqlServer("Server=.;Database=TicketBooking;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"));

                services.AddHostedService<KafkaConsumerService>();

                services.AddLogging(config => config.AddConsole());

            })

            .Build();

        using (var scope = host.Services.CreateScope())

        {

            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            dbContext.Database.Migrate();

        }

        await host.RunAsync();

    }

}

