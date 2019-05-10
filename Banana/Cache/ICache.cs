using System;
using System.Net;

namespace Banana.Cache
{
    public interface ICache
    {
        bool Exists(string key);
        T GetCache<T>(string key) where T : class;
        void SetCache(string key, object value);
        void SetCache(string key, object value, DateTimeOffset expiressAbsoulte);//设置绝对时间过期
        void SetSlidingCache(string key, object value, TimeSpan t);  //设置滑动过期， 因redis暂未找到自带的滑动过期类的API，暂无需实现该接口
        void RemoveCache(string key);
        void KeyMigrate(string key, EndPoint endPoint, int database, int timeountseconds);
        void Dispose();
        void GetMssages();
        void Publish(string msg);
    }
}
