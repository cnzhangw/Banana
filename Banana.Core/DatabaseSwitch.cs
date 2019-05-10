using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Banana.Core
{
    /// <summary>
    /// 说明：DatabaseSwitch
    /// 作者：张炜 
    /// 时间：2018/11/20 21:25:57
    /// Email:cnzhangw@sina.com
    /// CLR版本：4.0.30319.42000
    /// 唯一标识：8d66b45f-a009-4d6d-bbb3-743ddbb9c415
    /// </summary>
    public class DatabaseSwitch
    {

        public static void ChangeDatabase(string key, Action action, Action<Exception> onException)
        {
            new DatabaseSwitch().Change(key, action, onException);
        }

        private void Change(string key, Action action, Action<Exception> onException)
        {
            try
            {
#if NETSTANDARD2_0
                InvokeService.Proxy<ISwitchProxy>().ProxyAction(key, action);
#else
                // action.Invoke();
                throw new NotImplementedException("非NET CORE模式还未实现此方法，详情可联系QQ:316804454");
#endif
            }
            catch (Exception ex)
            {
                if (onException == null)
                {
                    throw ex;
                }
                else
                {
                    onException.Invoke(ex);
                }
            }
        }

#if NETSTANDARD2_0
        internal class InvokeService
        {
            public static T Proxy<T>()
            {
                return DispatchProxy.Create<T, InvokeProxy<T>>();
            }
        }
        public class InvokeProxy<T> : DispatchProxy
        {
            private Type type = null;
            public InvokeProxy()
            {
                type = typeof(T);
            }
            protected override object Invoke(MethodInfo targetMethod, object[] args)
            {
                string key = args[0] as string;

                var dbs = Config.GetValue<JToken>("database");
                if (dbs == null)
                {
                    throw new Exception("banana.json 缺少配置项 database");
                }

                if (dbs.Type != JTokenType.Object || !dbs.HasValues)
                {
                    throw new Exception("配置项 database 应该是一个object类型");
                }

                var that = dbs.GetValue<JToken>(key);
                if (that == null)
                {
                    throw new Exception($"banana.json 缺少配置项 database/{key}");
                }

                string connection = string.Empty, provider = string.Empty, databaseName = string.Empty;
                if (that.Type == JTokenType.String)
                {
                    // 同连接地址不同库
                    connection = Config.GetString("connection");
                    provider = Config.GetString("provider", "mysql");
                    databaseName = that.Value<string>();
                }
                else if (that.Type == JTokenType.Object)
                {
                    connection = that.GetString("connection");
                    provider = that.GetString("provider", "mysql");
                }
                else
                {
                    throw new Exception($"配置项 database/{key} 错误");
                }

                if (DatabaseInitializer.Instance.GetFromContext() != null)
                {
                    throw new Exception("ChangeDatabase函数不可以在Transaction内或多重自嵌套使用");
                }

                // 替换数据库名
                if (databaseName.HasValue())
                {
                    connection = new Regex(@"database=\w*", RegexOptions.IgnoreCase).Replace(connection, $"database={databaseName}");
                }

                using (var db = DatabaseInitializer.Instance.Create(connection, provider))
                {
                    db.KeepConnectionAlive = true;
                    CallContext<Database>.SetData(DatabaseInitializer.CONTEXT_DB, db);
                    try
                    {
                        targetMethod.Invoke(Activator.CreateInstance(typeof(SwitchProxy)), args);
                    }
                    catch (TargetInvocationException ex)
                    {
                        throw ex.InnerException;
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                    finally
                    {
                        db.KeepConnectionAlive = false;
                        CallContext<Database>.SetData(DatabaseInitializer.CONTEXT_DB, null);
                    }
                    return true;
                }
            }
        }
        internal interface ISwitchProxy
        {
            void ProxyAction(string key, Action action);
        }
        internal class SwitchProxy : ISwitchProxy
        {
            public void ProxyAction(string key, Action action)
            {
                action.Invoke();
            }
        }
#endif

    }
}
