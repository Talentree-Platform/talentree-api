using Microsoft.Extensions.Caching.Distributed;
using StackExchange.Redis;
using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Talentree.Service.Contracts;

namespace Talentree.Service.Services
{
    public class CacheService : ICacheService
    {
        private readonly IDistributedCache _distributedCache;
        private readonly IConnectionMultiplexer _redis;
        private readonly JsonSerializerOptions _jsonOptions;

        public CacheService(IDistributedCache distributedCache, IConnectionMultiplexer redis)
        {
            _distributedCache = distributedCache;
            _redis = redis;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                ReferenceHandler = ReferenceHandler.IgnoreCycles
            };
        }

        public async Task CacheResponseAsync(string cacheKey, object response, TimeSpan timeToLive)
        {
            if (response == null) return;

            var serializedResponse = JsonSerializer.Serialize(response, _jsonOptions);

            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = timeToLive
            };

            await _distributedCache.SetStringAsync(cacheKey, serializedResponse, options);
        }

        public async Task<T?> GetCachedResponseAsync<T>(string cacheKey)
        {
            var cachedResponse = await _distributedCache.GetStringAsync(cacheKey);

            if (string.IsNullOrEmpty(cachedResponse))
            {
                return default;
            }

            return JsonSerializer.Deserialize<T>(cachedResponse, _jsonOptions);
        }

        public async Task RemoveCacheByPatternAsync(string pattern)
        {
            var endPoints = _redis.GetEndPoints();
            foreach (var endpoint in endPoints)
            {
                var server = _redis.GetServer(endpoint);
                var keys = server.Keys(pattern: pattern).ToArray();
                if (keys.Length > 0)
                {
                    var db = _redis.GetDatabase();
                    await db.KeyDeleteAsync(keys);
                }
            }
        }
    }
}
