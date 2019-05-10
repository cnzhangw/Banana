using System;

namespace Banana.Core
{
    /// <summary>
    /// 服务基类
    /// </summary>
    /// <typeparam name="TId">主键类型，默认string</typeparam>
    public abstract class BaseService<T, TId> where T : class, new()
    {
        private IService<T, TId> _instance = null;// 普通查询实例
        private IService<T, TId> _instanceSpecial = null;// 事务、切换数据库专用实例
        private static readonly object _lock = new object();
        protected LogFactory Logger { get; private set; }
        protected IInterceptor Interceptor { get; private set; }

        protected BaseService()
        {
            Initialize();
        }
        protected BaseService(IInterceptor interceptor)
        {
            Interceptor = interceptor;
            Initialize();
        }

        private void Initialize()
        {
            try
            {
                if (Config.GetValue("log", false))
                {
                    Logger = LogFactory.GetLogger(typeof(BaseService<T, TId>));
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// 数据服务对象
        /// </summary>
        protected internal IService<T, TId> Service
        {
            get
            {
                if (InvokeSpecial)
                {
                    if (_instanceSpecial == null)
                    {
                        lock (_lock)
                        {
                            if (_instanceSpecial == null)
                            {
                                _instanceSpecial = new DataContext<T, TId>(Interceptor);
                            }
                        }
                    }
                    return _instanceSpecial;
                }
                else
                {
                    if (_instance == null)
                    {
                        lock (_lock)
                        {
                            if (_instance == null)
                            {
                                _instance = new DataContext<T, TId>(Interceptor);
                            }
                        }
                    }
                    return _instance;
                }
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
            if (_instance != null)
                _instance = null;//调用事务前，需删除service实例对象，目的在于重新构建线程内共享的db对象

            var result = Result.Create<Exception>();
            result.Success = new Transaction().Create(action, (ex) =>
            {
                Logger?.Error(ex, "Banana.Core");
                result.Fail(ex.Message).SetData(ex);
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
            if (_instance != null)
                _instance = null;//调用事务前，需删除service实例对象，目的在于重新构建线程内共享的db对象

            return new Transaction().Create(() =>
            {
                try
                {
                    action.Invoke();
                }
                catch (Exception ex)
                {
                    Logger?.Error(ex, "Banana.Core");
                    throw ex;
                }
            }, onException);
        }


#if !NETSTANDARD2_0

        //net core 环境下无此函数，net framework 环境中不推荐此函数

        [Obsolete("推荐使用：Transaction(Action action, Action<Exception> onException)", true)]
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
        [Obsolete("推荐使用 Transaction(Action action, Action<Exception> onException)", true)]
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

        internal bool InvokeSpecial { get; private set; }
        /// <summary>
        /// 主库名（建议默认db为slave库，所以此处为master库）
        /// </summary>
        internal string MasterDatabaseName
        {
            get; private set;
        } = "master";
        /// <summary>
        /// 设置master库名
        /// </summary>
        /// <param name="name">默认master</param>
        protected void SetMasterDatabase(string name)
        {
            if (name.IsNullOrWhiteSpace())
            {
                throw new ArgumentNullException("无效的主库名");
            }

            this.MasterDatabaseName = name;
        }

        protected internal void ChangeDatabase(Action callback)
        {
            ChangeDatabase(callback, null, null);
        }
        protected internal void ChangeDatabase(Action callback, Action<Exception> onException)
        {
            ChangeDatabase(callback, null, onException);
        }
        protected internal void ChangeDatabase(Action callback, ChangeDatabaseOptions options)
        {
            ChangeDatabase(callback, options, null);
        }
        protected internal void ChangeDatabase(Action callback, ChangeDatabaseOptions options, Action<Exception> onException)
        {
            try
            {
                InvokeSpecial = true;
                if (options != null)
                {
                    SetMasterDatabase(options.MasterName);
                }

                DatabaseSwitch.ChangeDatabase(MasterDatabaseName, () =>
                 {
                     try
                     {
                         callback.Invoke();
                     }
                     catch (Exception ex)
                     {
                         Logger?.Error(ex, "Banana.Core");
                         throw ex;
                     }
                 }, onException);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                InvokeSpecial = false;
                _instanceSpecial = null;
            }
        }

    }

    /// <summary>
    /// 服务层基类
    /// </summary>
    public abstract class BaseService<T> : BaseService<T, string> where T : class, new()
    {
        public BaseService() : base() { }
        public BaseService(IInterceptor interceptor) : base(interceptor) { }
    }


    public class ChangeDatabaseOptions
    {
        /// <summary>
        /// 主库名
        /// </summary>
        public string MasterName { get; set; }
        ///// <summary>
        ///// 启用事务
        ///// </summary>
        //public bool Transaction { get; set; }
    }

}