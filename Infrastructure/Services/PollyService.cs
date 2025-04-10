using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infrastructure.Configurations;
using Infrastructure.Interfaces;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Retry;

namespace Infrastructure.Services
{
    public class PollyService : IPollyService
    {
        private readonly AsyncRetryPolicy _retryPolicy;

        public PollyService(IOptions<PollySettings> settings)
        {
            var retryCount = settings.Value.RetryCount;
            var delay = TimeSpan.FromSeconds(settings.Value.RetryDelaySeconds);

            _retryPolicy = Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(retryCount,
                    retryAttempt => delay,
                    (exception, timeSpan, retryCount, context) =>
                    {
                        Console.WriteLine($"Retry {retryCount} after {timeSpan.TotalSeconds}s due to {exception.Message}");
                    });
        }

        public async Task RetryAsync(Func<Task> action, string context = null)
        {
            await _retryPolicy.ExecuteAsync(async (_) =>
            {
                await action();
            }, new Context(context ?? "Default"));
        }

        public async Task<T> RetryAsync<T>(Func<Task<T>> action, string context = null)
        {
            return await _retryPolicy.ExecuteAsync(async (ctx) =>
            {
                return await action();
            }, new Context(context ?? "Default"));
        }
    }
}
