using Cosmatics.Application.Common;
using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Cosmatics.Infrastructure.Background_Services
{
    internal class DistributedCache : ICacheService
    {
        private readonly IDistributedCache _distributedCache;
        public DistributedCache(IDistributedCache cache)
        {
            _distributedCache = cache;
        }
        public async Task<T?> GetDataAsync<T>(string key)
        {
            
            var value= await _distributedCache.GetStringAsync(key);
            if (!string.IsNullOrEmpty(value))
            {
                Console.WriteLine(" Redis Cache Visited");
                return  JsonSerializer.Deserialize<T>(value);
            }
            return default;
        }
        public async Task SetDataAsync<T>(string key, T value, TimeSpan? expiration = null)
        {
            var option = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration ?? TimeSpan.FromMinutes(30)
            };
            var jsonData = JsonSerializer.Serialize(value);
            await _distributedCache.SetStringAsync(key, jsonData, option);

        }

        public async Task RemoveDataAsync(string key)
        {
            await _distributedCache.RemoveAsync(key);
        }

 
    }
}
