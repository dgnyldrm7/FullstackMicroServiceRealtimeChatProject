using App.Core.Interface;
using Microsoft.Extensions.Caching.Memory;

namespace App.Infrastructure.Cache
{
    public class MemoryCacheService : IMemoryCacheService
    {
        private readonly IMemoryCache _memoryCache;

        public MemoryCacheService(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

        public bool Exists(string key)
        {
            return _memoryCache.TryGetValue(key, out var value);
        }

        public T? Get<T>(string key)
        {
            if (_memoryCache.TryGetValue(key, out var value) && value is T typedValue)
            {
                return typedValue;
            }

            return default;
        }

        public void Remove(string key)
        {
            _memoryCache.Remove(key);
        }

        public void Set<T>(string key, T value, TimeSpan expiration)
        {
            var cacheOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration
            };

            _memoryCache.Set(key, value, cacheOptions);
        }
    }
}
