using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AwesomeShop.Services.Orders.Infrastructure.CacheStorage
{
    public class RedisService : ICacheService
    {
        private readonly IDistributedCache _cache;
        public RedisService(IDistributedCache cache)
        {
            _cache = cache;
        }
        public async Task<T> GetAsync<T>(string key)
        {
            var objectString = await _cache.GetStringAsync(key);

            if (string.IsNullOrWhiteSpace(objectString))
            {
                Console.WriteLine($"**** Orders cache key {key} not found *****");
                return default(T);
            }

            return JsonConvert.DeserializeObject<T>(objectString);
        }

        public async Task SetAsync<T>(string key, T data)
        {
            var memoryCacheEntryOption = new DistributedCacheEntryOptions
            {
                // Tempo obrigatório para expirar
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(3600),
                // Tempo que passou sem ser expirado
                SlidingExpiration = TimeSpan.FromSeconds(1200),
            };

            var objectString = JsonConvert.SerializeObject(data);

            Console.WriteLine($"**** Orders cache set key {key} *****");

            await _cache.SetStringAsync(key, objectString, memoryCacheEntryOption);

        }
    }
}
