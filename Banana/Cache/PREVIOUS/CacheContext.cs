//using System;

//namespace Banana.Cache
//{
//#if NETSTANDARD2_0

//    using Microsoft.Extensions.Caching.Memory;

//    public class CacheContext : ICacheContext
//    {
//        private IMemoryCache _objCache;

//        public CacheContext(IMemoryCache objCache)
//        {
//            _objCache = objCache;
//        }

//        public override T Get<T>(string key)
//        {
//            return _objCache.Get<T>(key);
//        }

//        public override bool Set<T>(string key, T t, DateTime expire)
//        {
//            var obj = Get<T>(key);
//            if (obj != null)
//            {
//                Remove(key);
//            }

//            _objCache.Set(key, t, new MemoryCacheEntryOptions().SetAbsoluteExpiration(expire));   //绝对过期时间

//            return true;
//        }

//        public override bool Remove(string key)
//        {
//            _objCache.Remove(key);
//            return true;
//        }
//    }

//#else

//    using System;
//    using System.Linq;
//    using System.Collections;
//    using System.Collections.Generic;
//    using System.Runtime.Caching;

//    public static class CacheContext
//    {
//        private static readonly ObjectCache cache = MemoryCache.Default;
//        private static List<string> keyList = new List<string>();

//        /// <summary>
//        /// 清除所有缓存
//        /// </summary>
//        /// <returns></returns>
//        public static int Clear()
//        {
//            return Remove(string.Empty, 666);
//        }

//        /// <summary>
//        /// 清除缓存
//        /// </summary>
//        /// <param name="key"></param>
//        /// <param name="type">0-匹配开始 1-匹配结束 2-全字匹配 3-包含</param>
//        public static int Remove(string key, int type = 0)
//        {
//            List<string> keys = null;
//            switch (type)
//            {
//                case 0:
//                    keys = keyList.Where(x => x.StartsWith(key)).ToList();
//                    break;
//                case 1:
//                    keys = keyList.Where(x => x.EndsWith(key)).ToList();
//                    break;
//                case 2:
//                    keys = keyList.Where(x => x.Equals(key)).ToList();
//                    break;
//                case 3:
//                    keys = keyList.Where(x => x.Contains(key)).ToList();
//                    break;
//                case 666:
//                    keys = keyList.ToList();
//                    break;
//                default:
//                    break;
//            }

//            if (keys != null)
//            {
//                foreach (var item in keys)
//                {
//                    RemoveByKey(item);
//                }
//                return keys.Count;
//            }
//            return 0;
//        }

//        public static object Get(string key)
//        {
//            return cache.Get(key);
//        }

//        public static void Set(string key, object value, DateTimeOffset expireTime)
//        {
//            var policy = new CacheItemPolicy();
//            policy.RemovedCallback = new CacheEntryRemovedCallback((args) =>
//            {
//                keyList.RemoveAll(x => x.Equals(args.CacheItem.Key));
//            });

//            cache.Set(key, value, expireTime);

//            if (!keyList.Contains(key))
//            {
//                keyList.Add(key);
//            }
//        }

//        public static void Set(string key, object value)
//        {
//            Set(key, value, DateTime.Now.AddHours(1));
//        }

//        /// <summary>
//        /// 按键移除缓存（全字匹配）
//        /// </summary>
//        /// <param name="key"></param>
//        public static void RemoveByKey(string key)
//        {
//            Remove(key, 2);
//        }

//        public static T Get<T>(string key, Func<T> action)
//        {
//            return Get<T>(key, action, DateTime.Now.AddHours(1));
//        }
//        public static T Get<T>(string key, Func<T> action, DateTimeOffset expireTime)
//        {
//            var obj = Get(key);
//            if (obj == null || (obj is IList && ((IList)obj).Count == 0))
//            {
//                obj = action();
//                if (obj != null)
//                {
//                    if (obj is IList)
//                    {
//                        if (((IList)obj).Count > 0)
//                        {
//                            Set(key, obj, expireTime);
//                        }
//                    }
//                    else
//                    {
//                        Set(key, obj, expireTime);
//                    }
//                }
//            }
//            return (T)obj;
//        }

//    }

//#endif
//}
