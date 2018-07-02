using System;

namespace Banana.Cache
{
    #if NETSTANDARD2_0

    using Microsoft.Extensions.Caching.Memory;

    public class CacheContext : ICacheContext
    {
        private IMemoryCache _objCache;

        public CacheContext(IMemoryCache objCache)
        {
            _objCache = objCache;
        }

        public override T Get<T>(string key)
        {
            return _objCache.Get<T>(key);
        }

        public override bool Set<T>(string key, T t, DateTime expire)
        {
            var obj = Get<T>(key);
            if (obj != null)
            {
                Remove(key);
            }

            _objCache.Set(key, t, new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(expire));   //绝对过期时间

            return true;
        }

        public override bool Remove(string key)
        {
            _objCache.Remove(key);
            return true;
        }
    }

    #endif
}
