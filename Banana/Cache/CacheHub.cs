using System;
using System.Collections.Generic;
using System.Text;

namespace Banana.Cache
{
    public sealed class CacheHub
    {
        internal CacheHub() { }
        public static ICache cache;

        /// <summary>
        /// 判断缓存是否存在
        /// </summary>
        /// <typeparam name="CacheType">缓存类型</typeparam>
        /// <param name="key">键名</param>
        /// <returns></returns>
        public bool Exists<CacheType>(string key) where CacheType : ICache, new()
        {
            if (cache != null && typeof(CacheType).Equals(cache.GetType()))
                return cache.Exists(key);
            else
            {
                cache = new CacheType();
                return cache.Exists(key);
            }
        }

        /// <summary>
        /// 获取缓存
        /// </summary>
        /// <typeparam name="T">转换的类</typeparam>
        /// <typeparam name="CacheType">缓存类型</typeparam>
        /// <param name="key">键名</param>
        /// <returns>转换为T类型的值</returns>
        public T Get<T, CacheType>(string key)
            where T : class
            where CacheType : ICache, new()
        {
            if (cache != null && typeof(CacheType).Equals(cache.GetType()))
                return cache.GetCache<T>(key);
            else
            {
                cache = new CacheType();
                return cache.GetCache<T>(key);
            }
        }

        /// <summary>
        /// 保存缓存
        /// </summary>
        /// <typeparam name="CacheType">缓存类型</typeparam>
        /// <param name="key">键名</param>
        /// <param name="value">值</param>
        public void Set<CacheType>(string key, object value) where CacheType : ICache, new()
        {
            if (cache != null && typeof(CacheType).Equals(cache.GetType()))
                cache.SetCache(key, value);
            else
            {
                cache = new CacheType();
                cache.SetCache(key, value);
            }
        }

        /// <summary>
        /// 保存缓存并设置绝对过期时间
        /// </summary>
        /// <typeparam name="CacheType">缓存类型</typeparam>
        /// <param name="key">键名</param>
        /// <param name="value">值</param>
        /// <param name="expiressAbsoulte">绝对过期时间</param>
        public void Set<CacheType>(string key, object value, DateTimeOffset expiressAbsoulte) where CacheType : ICache, new()
        {
            if (cache != null && typeof(CacheType).Equals(cache.GetType()))
                cache.SetCache(key, value, expiressAbsoulte);
            else
            {
                cache = new CacheType();
                cache.SetCache(key, value, expiressAbsoulte);
            }
        }


#if NETSTANDARD2_0
        /// <summary>
        /// 保存滑动缓存
        /// </summary>
        /// <typeparam name="CacheType">只能用memorycache，redis暂不实现</typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="t">间隔时间</param>
        public void SaveSlidingCache<CacheType>(string key, object value, TimeSpan t) where CacheType : CacheMemory, new()
        {
            if (cache != null && typeof(CacheType).Equals(cache.GetType()))
                cache.SetSlidingCache(key, value, t);
            else
            {
                cache = new CacheType();
                cache.SetSlidingCache(key, value, t);
            }
        }
#endif

        /// <summary>
        /// 删除一个缓存
        /// </summary>
        /// <typeparam name="CacheType">缓存类型</typeparam>
        /// <param name="key">要删除的key</param>
        public void Remove<CacheType>(string key) where CacheType : ICache, new()
        {
            if (cache != null && typeof(CacheType).Equals(cache.GetType()))
                cache.RemoveCache(key);
            else
            {
                cache = new CacheType();
                cache.RemoveCache(key);
            }
        }

        /// <summary>
        /// 释放
        /// </summary>
        /// <typeparam name="CacheType"></typeparam>
        public void Dispose<CacheType>() where CacheType : ICache, new()
        {
            if (cache != null && typeof(CacheType).Equals(cache.GetType()))
                cache.Dispose();
            else
            {
                cache = new CacheType();
                cache.Dispose();
            }
        }

        //public void GetMessage<CacheType>() where CacheType : CacheRedis, new()
        //{
        //    if (cache != null && typeof(CacheType).Equals(cache.GetType()))
        //        cache.GetMssages();
        //    else
        //    {
        //        cache = new CacheType();
        //        cache.GetMssages();
        //    }
        //}
        //public void Publish<CacheType>(string msg) where CacheType : CacheRedis, new()
        //{
        //    if (cache != null && typeof(CacheType).Equals(cache.GetType()))
        //        cache.Publish(msg);
        //    else
        //    {
        //        cache = new CacheType();
        //        cache.Publish(msg);
        //    }
        //}

        #region cachememory only

        public bool Exists(string key)
        {
#if NETSTANDARD2_0
            return Exists<CacheMemory>(key);
#else
            throw new NotImplementedException(".net framework not implement yet.");
#endif
        }
        public T Get<T>(string key) where T : class
        {
#if NETSTANDARD2_0
            return Get<T, CacheMemory>(key);
#else
            throw new NotImplementedException(".net framework not implement yet.");
#endif
        }
        public string Get(string key)
        {
            return Get<string>(key);
        }
        public void Set(string key, object value)
        {
#if NETSTANDARD2_0
            Set<CacheMemory>(key, value);
#else
            throw new NotImplementedException(".net framework not implement yet.");
#endif
        }
        public void Set(string key, object value, DateTimeOffset expiressAbsoulte)
        {
#if NETSTANDARD2_0
            Set<CacheMemory>(key, value, expiressAbsoulte);
#else
            throw new NotImplementedException(".net framework not implement yet.");
#endif
        }
        public void Remove(string key)
        {
#if NETSTANDARD2_0
            Remove<CacheMemory>(key);
#else
            throw new NotImplementedException(".net framework not implement yet.");
#endif
        }
        public void Dispose()
        {
#if NETSTANDARD2_0
            Dispose<CacheMemory>();
#else
            throw new NotImplementedException(".net framework not implement yet.");
#endif
        }

        #endregion

    }
}
