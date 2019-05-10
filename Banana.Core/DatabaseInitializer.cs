using Banana.Core.Providers;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Threading;
using System.IO;
using Banana.Core.Interface;

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
        internal const string CONTEXT_DB = "BANANA.CORE.TRANSACTION";

        internal RewrittenDatabase GetFromContext()
        {
            RewrittenDatabase dbref = null;
#if NETSTANDARD2_0
            dbref = CallContext<RewrittenDatabase>.GetData(CONTEXT_DB);
#else
            dbref = CallContext.GetData(CONTEXT_DB) as RewrittenDatabase;
#endif
            return dbref;
        }

        internal RewrittenDatabase Create()
        {
            return GetFromContext() ?? CreateInstance();
        }

        internal RewrittenDatabase Create(string connection, string provider)
        {
            return GetFromContext() ?? CreateInstance(connection, provider);
        }

        private RewrittenDatabase CreateInstance(string connection = "", string provider_name = "")
        {
            string connection_string = connection.HasValue() ? connection : Config.GetString("connection");
            string connection_provider = provider_name.HasValue() ? provider_name : Config.GetString("provider");
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
                    // 对connection做特殊处理
                    var match = System.Text.RegularExpressions.Regex.Match(connection_string, @"password=([^;]+)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                    if (match.Success)
                    {
                        //string pwd = match.Groups[1].Value;
                        throw new NotSupportedException($"Banana.Core暂时不支持加密模式的sqlite");
                    }

                    string[] cs = connection_string.Split(";".ToArray(), StringSplitOptions.RemoveEmptyEntries);
                    StringBuilder builder = new StringBuilder();
                    foreach (var item in cs)
                    {
                        if (item.StartsWith("Data Source", StringComparison.OrdinalIgnoreCase))
                        {
                            string value = item.Split("=".ToArray(), StringSplitOptions.RemoveEmptyEntries)[1];
                            if (value.ToArray()[1] == ':')
                            {
                                // 绝对路径
                                if (!File.Exists(value))
                                {
                                    throw new FileNotFoundException($"sqlite db file not exists.\r\n{value}");
                                }

                                builder.Append(item);
                            }
                            else
                            {
                                // 相对路径
                                value = value.TrimStart('~').TrimStart('/').TrimStart('\\').TrimStart('\\'); // remove some special chars from start
                                if (new char[] { '/', '\\', '~' }.Contains(value.ToCharArray()[0]))
                                {
                                    throw new Exception($"sqlite connection string error\r\n{connection_string}");
                                }

                                value = value.Replace('/', '\\'); // convert / to \\
                                string fullname = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, value);
                                if (!File.Exists(fullname))
                                {
                                    throw new FileNotFoundException($"sqlite db file not exists.\r\n{fullname}");
                                }

                                builder.Append($"Data Source = {fullname}");
                            }
                        }
                        else
                        {
                            builder.Append($";{item}");
                        }
                    }

                    connection_string = builder.ToString();
                    provider = new SQLiteDatabaseProvider().GetFactory();
                    break;
                default:
                    //throw new NotSupportedException($"未实现{connection_provider}的Provider");
                    break;
            }

            if (provider == null)
            {
                return new RewrittenDatabase(connection_string, connection_provider);
            }
            else
            {
                return new RewrittenDatabase(connection_string, provider);
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

        static ConcurrentDictionary<string, ThreadLocal<T>> state = new ConcurrentDictionary<string, ThreadLocal<T>>();

        /// <summary>
        /// Stores a given object and associates it with the specified name.
        /// </summary>
        /// <param name="name">The name with which to associate the new item in the call context.</param>
        /// <param name="data">The object to store in the call context.</param>
        public static void SetData(string name, T data)
        {
            if (data == null)
            {
                var value = new ThreadLocal<T>();
                state.TryRemove(name, out value);
                return;
            }

            state.GetOrAdd(name, _ => new ThreadLocal<T>()).Value = data;
        }

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
