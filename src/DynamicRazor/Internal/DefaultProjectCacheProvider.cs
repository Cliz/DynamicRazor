using DynamicRazor.Interface;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace DynamicRazor.Internal
{
    internal class DefaultProjectCacheProvider : IDynamicRazorProjectCacheProvider
    {
        private IDictionary<string, IMemoryCache> _cache = new ConcurrentDictionary<string, IMemoryCache>();

        public IMemoryCache GetCache(string id)
        {
            if (string.IsNullOrEmpty(id)) throw new ArgumentException(nameof(id));

            if (!_cache.TryGetValue(id, out var cache))
            {
                cache = new MemoryCache(new MemoryCacheOptions());
                _cache[id] = cache;
            }

            return cache;
        }
    }
}
