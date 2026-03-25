using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cosmatics.Application.Common
{
    public interface ICacheService
    {
        Task<T?> GetDataAsync<T>(string key);
        Task SetDataAsync<T>(string key, T value, TimeSpan? expiration = null);
        Task RemoveDataAsync(string key);
    }
}
