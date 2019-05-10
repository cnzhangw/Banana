//using Banana;
//using Microsoft.Extensions.Caching.Redis;
//using Newtonsoft.Json;
//using StackExchange.Redis;
//using System;
//using System.Net;

//namespace Banana.Cache
//{
//    public class CacheRedis : ICache
//    {
//        public CacheRedis(/*RedisCacheOptions options, int database = 0*/)//这里可以做成依赖注入，但没打算做成通用类库，所以直接把连接信息直接写在帮助类里
//        {
//            options = new RedisCacheOptions();
//            options.Configuration = $"{Config.GetString("redis.connection")},password={Config.GetString("redis.password")}";
//            options.InstanceName = Config.GetString("redis.name");
//            int database = Config.GetInt("redis.database");
//            _connection = ConnectionMultiplexer.Connect(options.Configuration);
//            _cache = _connection.GetDatabase(database);
//            _instanceName = options.InstanceName;
//            _sub = _connection.GetSubscriber();
//        }

//        public static RedisCacheOptions options;
//        public static IDatabase _cache;

//        public static ConnectionMultiplexer _connection;

//        public static string _instanceName;

//        public static ISubscriber _sub;

//        /// <summary>
//        /// 取得redis的Key名称
//        /// </summary>
//        /// <param name="key"></param>
//        /// <returns></returns>
//        private string GetKeyForRedis(string key)
//        {
//            return _instanceName + key;
//        }
//        /// <summary>
//        /// 判断当前Key是否存在数据
//        /// </summary>
//        /// <param name="key"></param>
//        /// <returns></returns>
//        public bool Exists(string key)
//        {
//            if (string.IsNullOrWhiteSpace(key))
//                throw new ArgumentNullException(nameof(key));
//            return _cache.KeyExists(GetKeyForRedis(key));
//        }

//        /// <summary>
//        /// 取得缓存数据
//        /// </summary>
//        /// <typeparam name="T"></typeparam>
//        /// <param name="key"></param>
//        /// <returns></returns>
//        public T GetCache<T>(string key) where T : class
//        {
//            if (string.IsNullOrWhiteSpace(key))
//                throw new ArgumentNullException(nameof(key));
//            var value = _cache.StringGet(GetKeyForRedis(key));
//            if (!value.HasValue)
//                return default(T);
//            return JsonConvert.DeserializeObject<T>(value);
//        }
//        /// <summary>
//        /// 设置缓存数据
//        /// </summary>
//        /// <param name="key"></param>
//        /// <param name="value"></param>
//        public void SetCache(string key, object value)
//        {
//            if (string.IsNullOrWhiteSpace(key))
//                throw new ArgumentNullException(nameof(key));
//            if (value == null)
//                throw new ArgumentNullException(nameof(value));

//            if (Exists(GetKeyForRedis(key)))
//                RemoveCache(GetKeyForRedis(key));

//            _cache.StringSet(GetKeyForRedis(key), JsonConvert.SerializeObject(value));
//        }
//        /// <summary>
//        /// 设置绝对过期时间
//        /// </summary>
//        /// <param name="key"></param>
//        /// <param name="value"></param>
//        /// <param name="expiressAbsoulte"></param>
//        public void SetCache(string key, object value, DateTimeOffset expiressAbsoulte)
//        {
//            if (string.IsNullOrWhiteSpace(key))
//                throw new ArgumentNullException(nameof(key));
//            if (value == null)
//                throw new ArgumentNullException(nameof(value));

//            if (Exists(GetKeyForRedis(key)))
//                RemoveCache(GetKeyForRedis(key));
//            TimeSpan t = expiressAbsoulte - DateTimeOffset.Now;
//            _cache.StringSet(GetKeyForRedis(key), JsonConvert.SerializeObject(value), t);
//        }
//        /// <summary>
//        /// 设置相对过期时间
//        /// </summary>
//        /// <param name="key"></param>
//        /// <param name="value"></param>
//        /// <param name="expirationMinute"></param>
//        public void SetCache(string key, object value, double expirationMinute)
//        {
//            if (Exists(GetKeyForRedis(key)))
//                RemoveCache(GetKeyForRedis(key));

//            DateTime now = DateTime.Now;
//            TimeSpan ts = now.AddMinutes(expirationMinute) - now;
//            _cache.StringSet(GetKeyForRedis(key), JsonConvert.SerializeObject(value), ts);
//        }
//        /// <summary>
//        /// 
//        /// </summary>
//        /// <param name="key"></param>
//        /// <param name="endPoint"></param>
//        /// <param name="database"></param>
//        /// <param name="timeountseconds"></param>
//        public void KeyMigrate(string key, EndPoint endPoint, int database, int timeountseconds)
//        {
//            if (string.IsNullOrWhiteSpace(key))
//                throw new ArgumentNullException(nameof(key));
//            _cache.KeyMigrate(GetKeyForRedis(key), endPoint, database, timeountseconds);
//        }
//        /// <summary>
//        /// 移除redis
//        /// </summary>
//        /// <param name="key"></param>
//        public void RemoveCache(string key)
//        {
//            if (string.IsNullOrWhiteSpace(key))
//            {
//                throw new ArgumentNullException(nameof(key));
//            }

//            _cache.KeyDelete(GetKeyForRedis(key));
//        }

//        /// <summary>
//        /// 销毁连接
//        /// </summary>
//        public void Dispose()
//        {
//            if (_connection != null)
//            {
//                _connection.Dispose();
//            }

//            GC.SuppressFinalize(this);
//        }

//        public void SetSlidingCache(string key, object value, TimeSpan t)
//        {
//            throw new NotImplementedException();
//        }

//        public void GetMssages()
//        {
//            using (_connection = ConnectionMultiplexer.Connect(options.Configuration))
//            {
//                _sub.Subscribe("msg", (channel, message) =>
//                {
//                    string result = message;
//                });
//            }
//        }

//        public void Publish(string msg)
//        {
//            using (_connection = ConnectionMultiplexer.Connect(options.Configuration))
//            {
//                _sub.Publish("msg", msg);
//            }
//        }
//    }
//}
