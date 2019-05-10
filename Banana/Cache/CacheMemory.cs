#if NETSTANDARD2_0
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Net;

namespace Banana.Cache
{
    public class CacheMemory : ICache
    {
        //public MemoryCacheHelper(/*MemoryCacheOptions options*/)//这里可以做成依赖注入，但没打算做成通用类库，所以直接把选项直接封在帮助类里边
        //{
        //    //this._cache = new MemoryCache(options);
        //    //this._cache = new MemoryCache(new MemoryCacheOptions());
        //}
        //public MemoryCacheHelper(MemoryCacheOptions options)//这里可以做成依赖注入，但没打算做成通用类库，所以直接把选项直接封在帮助类里边
        //{
        //    this._cache = new MemoryCache(options);
        //}

        public static IMemoryCache _cache = new MemoryCache(new MemoryCacheOptions());

        /// <summary>
        /// 是否存在此缓存
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool Exists(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));
            object v = null;
            return _cache.TryGetValue<object>(key, out v);
        }
        /// <summary>
        /// 取得缓存数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public T GetCache<T>(string key) where T : class
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));


            _cache.TryGetValue<T>(key, out T v);


            return v;
        }
        /// <summary>
        /// 设置缓存
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void SetCache(string key, object value)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));
            if (value == null)
                throw new ArgumentNullException(nameof(value));


            object v = null;
            if (_cache.TryGetValue(key, out v))
                _cache.Remove(key);
            _cache.Set<object>(key, value);
        }
        /// <summary>
        /// 设置缓存,绝对过期
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="expirationMinute">间隔分钟</param>
        /// CommonManager.CacheObj.Save<RedisCacheHelper>("test", "RedisCache works!", 30);
        public void SetCache(string key, object value, double expirationMinute)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            object v = null;
            if (_cache.TryGetValue(key, out v))
                _cache.Remove(key);
            DateTime now = DateTime.Now;
            TimeSpan ts = now.AddMinutes(expirationMinute) - DateTime.Now;
            _cache.Set<object>(key, value, ts);
        }
        /// <summary>
        /// 设置缓存,绝对过期
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="expirationTime">DateTimeOffset 结束时间</param>
        /// CommonManager.CacheObj.Save<RedisCacheHelper>("test", "RedisCache works!", DateTimeOffset.Now.AddSeconds(30));
        public void SetCache(string key, object value, DateTimeOffset expirationTime)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            object v = null;
            if (_cache.TryGetValue(key, out v))
                _cache.Remove(key);

            _cache.Set<object>(key, value, expirationTime);
        }
        /// <summary>
        /// 设置缓存,相对过期时间
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="t"></param>
        /// CommonManager.CacheObj.SaveSlidingCache<MemoryCacheHelper>("test", "MemoryCache works!",TimeSpan.FromSeconds(30));
        public void SetSlidingCache(string key, object value, TimeSpan t)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            object v = null;
            if (_cache.TryGetValue(key, out v))
                _cache.Remove(key);

            _cache.Set(key, value, new MemoryCacheEntryOptions()
            {
                SlidingExpiration = t
            });
        }
        /// <summary>
        /// 移除缓存
        /// </summary>
        /// <param name="key"></param>
        public void RemoveCache(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));

            _cache.Remove(key);
        }

        /// <summary>
        /// 释放
        /// </summary>
        public void Dispose()
        {
            if (_cache != null)
            {
                _cache.Dispose();
            }

            GC.SuppressFinalize(this);
        }

        public void KeyMigrate(string key, EndPoint endPoint, int database, int timeountseconds)
        {
            throw new NotImplementedException();
        }

        public void GetMssages()
        {
            throw new NotImplementedException();
        }

        public void Publish(string msg)
        {
            throw new NotImplementedException();
        }

    }
}
#endif