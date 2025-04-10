using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Interfaces
{
    public interface IPollyService
    {
        Task RetryAsync(Func<Task> action, string context = null);
        Task<T> RetryAsync<T>(Func<Task<T>> action, string context = null);
    }
}
