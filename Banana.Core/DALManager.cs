using Banana.Core.Provider;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Banana.Core
{
    public class DALManager<T, TId>
        where T : DataContextModel, new()
    {
        private static IDAL<T, TId> _instance = null;
        private static object _lockObject = new object();
        protected DALManager() { }

        public static IDAL<T, TId> GetInstance()
        {
            if (_instance == null)
            {
                lock (_lockObject)
                {
                    if (_instance == null)
                    {
                        using (var db = new AbstractDataAccess())
                        {
                            if (_instance == null)
                            {
                                switch (db.GetDBType())
                                {
                                    case DBType.SqlServer:
                                        _instance = new SqlServerContext<T, TId>();
                                        break;
                                    case DBType.SqlServerCE:
                                        throw new Exception("Banana.Core还未实现SqlServerCE数据库的驱动实例");
                                        break;
                                    case DBType.MySql:
                                        _instance = new MySqlContext<T, TId>();
                                        break;
                                    case DBType.PostgreSQL:
                                        throw new Exception("Banana.Core还未实现PostgreSQL数据库的驱动实例");
                                        break;
                                    case DBType.Oracle:
                                        _instance = new OracleContext<T, TId>();
                                        break;
                                    case DBType.SQLite:
                                        throw new Exception("Banana.Core还未实现SQLite数据库的驱动实例");
                                        break;
                                    default:
                                        throw new Exception("Banana.Core无法识别当前数据库的驱动实例");
                                        break;
                                }
                            }
                        }
                    }
                }
            }
            return _instance;
        }
    }

    public class DALManager<T> : DALManager<T, int> where T : DataContextModel, new() { }

}
