using System;
using Banana.Utility;

namespace Banana.Core
{
    /// <summary>
    /// 服务基类
    /// </summary>
    /// <typeparam name="TId">主键类型，默认string</typeparam>
    public abstract class BaseService<T, TId> where T : class, new()
    {
        private IService<T, TId> instance = null;
        private static readonly object lockObj = new object();
        protected LogFactory Logger { get; private set; }

        protected BaseService()
        {
            Logger = LogFactory.GetLogger(typeof(BaseService<T, TId>));
        }

        /// <summary>
        /// 数据服务对象
        /// </summary>
        protected internal IService<T, TId> Service
        {
            get
            {
                if (instance == null)
                {
                    lock (lockObj)
                    {
                        if (instance == null)
                        {
                            instance = new DataContext<T, TId>();
                        }
                    }
                }
                return instance;
            }
        }

        #region Transaction

        /// <summary>
        /// 事务
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        protected internal Result<Exception> Transaction(Action action)
        {
            if (instance != null)
                instance = null;//调用事务前，需删除service实例对象，目的在于重新构建线程内共享的db对象

            var result = Result.Create<Exception>();
            result.Success = new Transaction().Create(action, (ex) =>
            {
                Logger.Error(ex, "Banana.Core");

                result.Data = ex;
                result.Message = ex.Message;
            });
            return result;
        }
        /// <summary>
        /// 事务，提供异常回调函数
        /// </summary>
        /// <param name="action"></param>
        /// <param name="onException">异常回调函数</param>
        /// <returns></returns>
        protected internal bool Transaction(Action action, Action<Exception> onException)
        {
            if (instance != null)
                instance = null;//调用事务前，需删除service实例对象，目的在于重新构建线程内共享的db对象

            return new Transaction().Create(() =>
            {
                try
                {
                    action.Invoke();
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Banana.Core");
                    throw ex;
                }
            }, onException);
        }


#if !NETSTANDARD2_0

        //net core 环境下无此函数，net framework 环境中不推荐此函数

        [Obsolete("推荐使用 Transaction(Action action)")]
        protected internal Result<Exception> Transaction(Func<bool> action)
        {
            var result = Result.Create<Exception>();
            result.Success = new Transaction().Create(action, (ex) =>
            {
                Logger.Error(ex, "Banana.Core");
                result.Data = ex;
                result.Message = ex.Message;
            });
            return result;
        }
        [Obsolete("推荐使用 Transaction(Action action, Action<Exception> onException)")]
        protected internal bool Transaction(Func<bool> action, Action<Exception> onException)
        {
            return new Transaction().Create(() =>
            {
                try
                {
                    action.Invoke();
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Banana.Core");
                    throw ex;
                }
            }, onException);
        }

#endif

        #endregion

        /// <summary>
        /// 获取其他服务层接口实例
        /// </summary>
        /// <typeparam name="TRelated">xxxBLL</typeparam>
        /// <returns></returns>
        //protected internal IService<TRelated> GetService<TRelated>() where TRelated : class
        //{
        //    var key = typeof(TRelated).FullName.ToUpper();
        //    if (serviceCache.ContainsKey(key))
        //    {
        //        return (IService<TRelated>)serviceCache[key];
        //    }
        //    var instance = new DataContext<TRelated>();
        //    serviceCache[key] = instance;
        //    return instance;
        //}

    }

    /// <summary>
    /// 服务层基类
    /// </summary>
    public abstract class BaseService<T> : BaseService<T, string> where T : class, new() { }
}