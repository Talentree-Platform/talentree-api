using System;
using System.Threading.Tasks;

namespace Talentree.Service.Contracts
{
    public interface ICacheService
    {
        Task CacheResponseAsync(string cacheKey, object response, TimeSpan timeToLive);
        Task<T?> GetCachedResponseAsync<T>(string cacheKey);
        Task RemoveCacheByPatternAsync(string pattern);
    }
}
