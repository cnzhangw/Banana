using Banana.Core.Providers;
using Banana.Utility;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Threading;

#if !NETSTANDARD2_0
using System.Runtime.Remoting.Messaging;
#endif

namespace Banana.Core
{
    /// <summary>
    /// 说明：DatabaseInitializer
    /// 作者：张炜 
    /// 时间：2018/5/19 21:48:10
    /// Email:cnzhangw@sina.com
    /// CLR版本：4.0.30319.42000
    /// 唯一标识：0277592f-79de-42ac-8bda-f87ca4ed237d
    /// </summary>
    internal sealed class DatabaseInitializer
    {
        internal static DatabaseInitializer Instance = new DatabaseInitializer();
        internal const string DATABASE_CONTEXT_KEY = "banana_core_transaction";

        internal Database Init()
        {
            Database refdb = null;

#if NETSTANDARD2_0
            refdb = CallContext<Database>.GetData(DATABASE_CONTEXT_KEY);
#else
            refdb = CallContext.GetData(DATABASE_CONTEXT_KEY) as Database;
#endif
            if (refdb != null)
            {
                return refdb;
            }

            string connection_string = Config.GetString("connection_string");
            string connection_provider = Config.GetString("connection_provider");

            DbProviderFactory provider = null;
            switch (connection_provider.ToLower())
            {
                case "mysql":
                    provider = new MySqlDatabaseProvider().GetFactory();
                    break;
                case "oracle":
                    provider = new OracleDatabaseProvider().GetFactory();
                    break;
                case "sqlserver":
                    provider = new SqlServerDatabaseProvider().GetFactory();
                    break;
                case "sqlite":
                    provider = new SQLiteDatabaseProvider().GetFactory();
                    break;
                default:
                    //throw new NotSupportedException($"未实现{connection_provider}的Provider");
                    break;
            }

            if (provider == null)
            {
                return new Database(connection_string, connection_provider);
            }
            else
            {
                return new Database(connection_string, provider);
            }
        }
    }



#if NETSTANDARD2_0

    /// <summary>
    /// netstandard下无callcontext对象，需通过asynclocal来实现多线程下变量同步共享
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal static class CallContext<T>
    {

        //ThreadLocal 线程内变量共享
        //AsyncLocal 线程间变量共享
        //此处将ThreadLocal替换AsyncLocal，为了解决不同请求，保存各自的db实例

        static ConcurrentDictionary<string, ThreadLocal<T>> state
            = new ConcurrentDictionary<string, ThreadLocal<T>>();

        /// <summary>
        /// Stores a given object and associates it with the specified name.
        /// </summary>
        /// <param name="name">The name with which to associate the new item in the call context.</param>
        /// <param name="data">The object to store in the call context.</param>
        public static void SetData(string name, T data) =>
            state.GetOrAdd(name, _ => new ThreadLocal<T>()).Value = data;

        /// <summary>
        /// Retrieves an object with the specified name from the <see cref="CallContext"/>.
        /// </summary>
        /// <typeparam name="T">The type of the data being retrieved. Must match the type used when the <paramref name="name"/> was set via <see cref="SetData{T}(string, T)"/>.</typeparam>
        /// <param name="name">The name of the item in the call context.</param>
        /// <returns>The object in the call context associated with the specified name, or a default value for <typeparamref name="T"/> if none is found.</returns>
        public static T GetData(string name) =>
            state.TryGetValue(name, out ThreadLocal<T> data) ? data.Value : default(T);
    }

#endif

}
