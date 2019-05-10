using System;
using System.Linq;
using System.Collections.Generic;
using Banana.Core.Interface;
using Banana.Core.Providers;
using Banana.Core.Dapper;
using System.Data.Common;
using System.Linq.Expressions;
using System.Reflection;
using System.Data;
using System.Dynamic;
using Newtonsoft.Json.Linq;
using System.Text;

namespace Banana.Core
{
    /// <summary>
    /// 说明：DataContext
    /// 作者：张炜 
    /// 时间：2018/5/13 8:00:41
    /// Email:cnzhangw@sina.com
    /// CLR版本：4.0.30319.42000
    /// 唯一标识：1efae6ee-4a01-4ad3-8f9e-1b13196b5bbe
    /// </summary>
    internal partial class DataContext<T, TId> : IService<T, TId>, IDisposable
        where T : class, new()
    {
        LogFactory logger = LogFactory.GetLogger(typeof(DataContext<T, TId>));
        private RewrittenDatabase _rdb = null;

        private IInterceptor Interceptor { get; set; }

        public DataContext(IInterceptor interceptor)
        {
            _rdb = DatabaseInitializer.Instance.Create();
            ExpressionHelper.Provider = _rdb.Provider;

            if (interceptor != null)
            {
                Interceptor = interceptor;
                _rdb.OnExecutingEvent += (sql, args) =>
                {
                    Interceptor.OnExecutingCommand(sql, args);
                };
            }
        }

        internal DataContext(RewrittenDatabase db, IInterceptor interceptor)
        {
            _rdb = db;
            ExpressionHelper.Provider = _rdb.Provider;

            if (interceptor != null)
            {
                _rdb.OnExecutingEvent += (sql, args) =>
                {
                    interceptor.OnExecutingCommand(sql, args);
                };
            }
        }

        public void Dispose()
        {
            _rdb.Dispose();
        }

        /// <summary>
        /// 解析表达式得到列名数组
        /// </summary>
        /// <typeparam name="U"></typeparam>
        /// <param name="db"></param>
        /// <param name="expression"></param>
        /// <param name="inverse">是否反转当前所选字段（即排除所选后剩下的列）</param>
        /// <returns></returns>
        internal string[] GetColumns<U>(RewrittenDatabase db, Expression<Func<U, object>> expression, bool inverse)
        {
            var pd = PocoData.ForType(typeof(U), db.DefaultMapper);
            string primaryKey = pd.TableInfo.PrimaryKey;
            var columns_increase = new List<string>();
            var columns_decrease = (from x in pd.Columns where x.Value.ResultColumn == false select x.Value.ColumnName).ToList();
            columns_decrease.RemoveAll(x => x.Equals(primaryKey, StringComparison.OrdinalIgnoreCase));

            var action = new Action<MemberInfo>((member) =>
            {
                var column = (ColumnAttribute)member.GetCustomAttribute(typeof(ColumnAttribute), false);
                if (column == null)
                {
                    string name = member.Name.ToLower();
                    if (name.Equals(primaryKey, StringComparison.OrdinalIgnoreCase)) return;
                    if (inverse)
                    {
                        columns_decrease.RemoveAll(x => x.Equals(name, StringComparison.OrdinalIgnoreCase));
                    }
                    else
                    {
                        columns_increase.Add(name);
                    }
                }
                else
                {
                    string name = column.Name;
                    if (name.Equals(primaryKey, StringComparison.OrdinalIgnoreCase)) return;
                    if (inverse)
                    {
                        columns_decrease.RemoveAll(x => x.Equals(name, StringComparison.OrdinalIgnoreCase));
                    }
                    else
                    {
                        columns_increase.Add(name);
                    }
                }
            });

            if (expression.Body is NewExpression)
            {
                foreach (var item in (expression.Body as NewExpression).Arguments)
                {
                    var temp = item as MemberExpression;
                    if (temp != null)
                    {
                        action.Invoke(temp.Member);
                    }
                }
            }
            else if (expression.Body is MemberExpression)
            {
                action.Invoke((expression.Body as MemberExpression).Member);
            }
            else if (expression.Body is NewArrayExpression)
            {
                foreach (var item in (expression.Body as NewArrayExpression).Expressions)
                {
                    var temp = item as MemberExpression;
                    if (temp != null)
                    {
                        action.Invoke(temp.Member);
                    }
                }
            }
            else if (expression.Body is UnaryExpression)//一元表达式
            {
                action.Invoke(((expression.Body as UnaryExpression).Operand as MemberExpression).Member);
            }
            else
            {
                throw new NotSupportedException($"Banana.Core：不支持解析当前的表达式");
            }
            //else if (expression.Body is UnaryExpression)
            //{
            //    action.Invoke(((MemberExpression)((UnaryExpression)expression.Body).Operand).Member);
            //}

            return (inverse ? columns_decrease : columns_increase).ToArray();
        }

        protected TResult Execute<TResult>(Func<RewrittenDatabase, TResult> action)
        {
            TResult result = default(TResult);
            try
            {
                _rdb.OpenSharedConnection();
                result = action.Invoke(_rdb);
            }
            catch (Exception ex)
            {
                logger.Fatal(ex, "Banana.Core");
                throw ex;
            }
            finally
            {
                _rdb.CloseSharedConnection();
            }
            return result;
        }
        protected TResult ExecuteWithoutConnect<TResult>(Func<Database, TResult> action)
        {
            TResult result = default(TResult);
            try
            {
                result = action.Invoke(_rdb);
            }
            catch (Exception ex)
            {
                logger.Fatal(ex, "Banana.Core");
                throw ex;
            }
            finally
            {
                _rdb.CloseSharedConnection();
            }
            return result;
        }

        #region Single

        public T Single(TId id)
        {
            return this.Single<T>(id);
        }
        public T Single<Tid>(Tid id)
        {
            return this.Single<T, Tid>(id);
        }


        public U Single<U>(TId id)
        {
            return Execute((db) =>
            {
                return this.Single<U, TId>(id);
            });
        }
        public U Single<U, Tid>(Tid id)
        {
            return Execute((db) =>
            {
                return db.SingleOrDefault<U>(primaryKey: id);
            });
        }


        public T Single(Expression<Func<T, bool>> where)
        {
            return this.Single<T>(where);
        }
        public U Single<U>(Expression<Func<U, bool>> where)
        {
            return Execute((db) =>
            {
                string sql_parts = ExpressionHelper.ReplaceSpecialWords(ExpressionHelper.AnalyticExpression(where));
                var sql = Sql.Builder.Append($"where {sql_parts}");
                return db.FirstOrDefault<U>(sql);
            });
        }

        public T Single(Sql sql)
        {
            return this.Single<T>(sql);
        }
        public U Single<U>(Sql sql)
        {
            return Execute((db) =>
            {
                return db.FirstOrDefault<U>(sql);
            });
        }

        public dynamic SingleDynamic(Sql sql)
        {
            return this.Execute((db) =>
            {
                if (Interceptor != null)
                {
                    Interceptor.OnExecutingCommand(sql.ToString(), sql.Arguments);
                }

                object args = ArrayToObject(sql.Arguments);
                return db.Connection.QueryFirstOrDefault(sql.ToString(), args);
            });
        }

        #endregion

        #region Query

        public IEnumerable<T> Query()
        {
            return this.Query<T>();
        }
        public IEnumerable<U> Query<U>()
        {
            return this.Query<U>(Sql.Builder);
        }

        public IEnumerable<T> Query(Sql sql)
        {
            return this.Query<T>(sql);
        }
        public IEnumerable<U> Query<U>(Sql sql)
        {
            return Execute((db) =>
            {
                return db.Query<U>(sql);
            });
        }

        public IEnumerable<T> Query(Expression<Func<T, bool>> where)
        {
            return this.Query<T>(where);
        }
        public IEnumerable<U> Query<U>(Expression<Func<U, bool>> where)
        {
            return Execute((db) =>
            {
                string sql_parts = ExpressionHelper.ReplaceSpecialWords(ExpressionHelper.AnalyticExpression(where));
                var sql = Sql.Builder.Append($"where {sql_parts}");
                return db.Query<U>(sql);
            });
        }

        #endregion

        #region Fetch

        public List<T> Fetch(Sql sql)
        {
            return this.Fetch<T>(sql);
        }
        public List<U> Fetch<U>(Sql sql)
        {
            return Execute((db) =>
            {
                return db.Fetch<U>(sql);
            });
        }

        public List<T> Fetch(Expression<Func<T, bool>> where)
        {
            return this.Fetch<T>(where);
        }
        public List<U> Fetch<U>(Expression<Func<U, bool>> where)
        {
            return Execute((db) =>
            {
                string sql_parts = ExpressionHelper.ReplaceSpecialWords(ExpressionHelper.AnalyticExpression(where));
                var sql = Sql.Builder.Append($"where {sql_parts}");
                return db.Fetch<U>(sql);
            });
        }

        public List<T> Fetch(long page, long limit, Sql sql)
        {
            return this.Fetch<T>(page, limit, sql);
        }
        public List<U> Fetch<U>(long page, long limit, Sql sql)
        {
            return _rdb.Fetch<U>(page, limit, sql);
        }

        public List<dynamic> FetchDynamic(Sql sql)
        {
            var list = new List<dynamic>();
            this.QueryMultiple(sql, (reader) =>
            {
                list = reader.Read().AsList();
            });
            return list;
        }

        #endregion

        #region Delete

        public bool Delete(T model)
        {
            return this.Delete<T>(model);
        }
        public bool Delete<U>(U model)
        {
            return Execute((db) =>
            {
                return db.Delete<U>(model) > 0;
            });
        }


        public bool Delete(TId id)
        {
            return this.Delete<T, TId>(id);
        }
        public bool Delete<U>(TId id)
        {
            return this.Delete<U, TId>(id);
        }
        public bool Delete<U, UId>(UId id)
        {
            return Execute((db) =>
            {
                return db.Delete<U>(id) > 0;
            });
        }


        public int Delete(Sql sql)
        {
            return this.Delete<T>(sql);
        }
        public int Delete<U>(Sql sql)
        {
            return Execute((db) =>
            {
                return db.Delete<U>(sql);
            });
        }


        public int Delete(Expression<Func<T, bool>> where)
        {
            return this.Delete<T>(where);
        }
        public int Delete<U>(Expression<Func<U, bool>> where)
        {
            return Execute((db) =>
            {
                string sql_parts = ExpressionHelper.ReplaceSpecialWords(ExpressionHelper.AnalyticExpression(where));
                var sql = Sql.Builder.Append($"WHERE {sql_parts}");
                return db.Delete<U>(sql);
            });
        }


        public int Delete(TId[] ids)
        {
            return this.Delete<T, TId>(ids);
        }
        public int Delete<U, UId>(UId[] ids)
        {
            return Execute((db) =>
            {
                var pd = PocoData.ForType(typeof(U), db.DefaultMapper);
                bool isString = !pd.TableInfo.AutoIncrement;
                var sql = Sql.Builder;
                sql.Append($"WHERE {pd.TableInfo.PrimaryKey} IN( {(isString ? "'" : "")}{string.Join(isString ? "', '" : ",", ids)}{(isString ? "'" : "")} )");
                return db.Delete<U>(sql);
            });
        }

        #endregion

        #region Update

        /// <summary>
        /// 属性设置
        /// </summary>
        /// <typeparam name="U"></typeparam>
        /// <param name="model"></param>
        internal void PropertySet<U>(U model, Dictionary<string, object> args)
        {
            Type type = typeof(U);
            PropertyInfo pi = null;
            foreach (var key in args.Keys)
            {
                pi = type.GetProperty(key);
                if (pi != null)
                {
                    pi.SetValue(model, args[key]);
                }
            }
        }


        public bool Update(T model)
        {
            return this.Update<T>(model);
        }
        public bool Update(T model, Expression<Func<T, object>> columns)
        {
            return this.Update(model, columns, false);
        }
        public bool Update(T model, Expression<Func<T, object>> columns, bool inverse)
        {
            return this.Update<T>(model, columns, inverse);
        }


        public bool Update<U>(U model)
        {
            return Update<U>(string.Empty, model);
        }

        public bool Update<U>(string tableName, U model)
        {
            return Execute((db) =>
            {
                var pd = PocoData.ForType(typeof(U), db.DefaultMapper);
                PropertySet(model, new Dictionary<string, object>()
                {
                    { "UpdateOn", DateTime.Now }
                });
                return db.Update((tableName.HasValue() ? tableName : pd.TableInfo.TableName), pd.TableInfo.PrimaryKey, model) > 0;
            });
        }

        public bool Update<U>(U model, Expression<Func<U, object>> columns)
        {
            return this.Update<U>(model, columns, false);
        }

        public bool Update<U>(U model, Expression<Func<U, object>> columns, bool inverse)
        {
            return Update<U>(string.Empty, model, columns, inverse);
        }

        public bool Update<U>(string tableName, U model, Expression<Func<U, object>> columns, bool inverse)
        {
            return Execute((db) =>
            {
                string[] column_array = GetColumns(db, columns, inverse);
                if (column_array.Length == 0)
                {
                    return false;
                }

                PropertySet(model, new Dictionary<string, object>()
                {
                    { "UpdateOn",DateTime.Now }
                });

                var pd = PocoData.ForType(typeof(U), db.DefaultMapper);
                if (tableName.IsNullOrWhiteSpace())
                {
                    tableName = pd.TableInfo.TableName;
                }

                return db.Update(tableName, pd.TableInfo.PrimaryKey, model, column_array) > 0;
            });
        }

        public int Update<U>(U model, Expression<Func<U, object>> columns, Expression<Func<U, bool>> where)
        {
            return Update<U>(string.Empty, model, columns, where);
        }
        public int Update<U>(U model, Expression<Func<U, object>> columns, Expression<Func<U, bool>> where, bool inverse)
        {
            return Update<U>(string.Empty, model, columns, where, inverse);
        }

        public int Update<U>(string tableName, U model, Expression<Func<U, object>> columns, Expression<Func<U, bool>> where)
        {
            return Execute((db) =>
            {
                string[] column_array = GetColumns(db, columns, false);
                if (column_array.Length == 0)
                {
                    return 0;
                }

                var pd = PocoData.ForType(typeof(U), db.DefaultMapper);
                if (tableName.IsNullOrWhiteSpace())
                {
                    tableName = pd.TableInfo.TableName;
                }

                // check primay key (processed in getcolumns method)
                if (column_array.Count(x => x.Equals(pd.TableInfo.PrimaryKey, StringComparison.OrdinalIgnoreCase)) > 0)
                {
                    throw new Exception("prohibit updating primary key");
                }

                var whereSql = ExpressionHelper.AnalyticExpression(where);

                PropertySet(model, new Dictionary<string, object>()
                {
                    { "UpdateOn", DateTime.Now }
                });

                return db.Update(tableName, pd.TableInfo.PrimaryKey, model, column_array, whereSql);
            });
        }

        public int Update<U>(string tableName, U model, Expression<Func<U, object>> columns, Expression<Func<U, bool>> where, bool inverse)
        {
            return Execute((db) =>
            {
                string[] column_array = GetColumns(db, columns, inverse);
                if (column_array.Length == 0)
                {
                    return 0;
                }

                var pd = PocoData.ForType(typeof(U), db.DefaultMapper);
                if (tableName.IsNullOrWhiteSpace())
                {
                    tableName = pd.TableInfo.TableName;
                }

                // check primay key (processed in getcolumns method)
                if (column_array.Count(x => x.Equals(pd.TableInfo.PrimaryKey, StringComparison.OrdinalIgnoreCase)) > 0)
                {
                    throw new Exception("prohibit updating primary key");
                }

                var whereSql = ExpressionHelper.AnalyticExpression(where);

                PropertySet(model, new Dictionary<string, object>()
                {
                    { "UpdateOn", DateTime.Now }
                });

                return db.Update(tableName, pd.TableInfo.PrimaryKey, model, column_array, whereSql);
            });
        }

        #endregion

        #region Insert

        internal static IDWorker idWorker = new IDWorker();

        public bool Insert(T model)
        {
            return this.Insert<T>(model);
        }

        public bool Insert<U>(U model)
        {
            return Insert<U>(string.Empty, model);
        }

        public bool Insert<U>(string tableName, U model)
        {
            return Execute((db) =>
            {
                var pd = PocoData.ForType(typeof(U), db.DefaultMapper);
                var pk = pd.Columns[pd.TableInfo.PrimaryKey];
                if (pk.GetValue(model) == null)
                {
                    string pk_type_name = pk.PropertyInfo.PropertyType.Name;
                    if (pk_type_name.Equals("string", StringComparison.OrdinalIgnoreCase))
                    {
                        string propertyName = (from x in pd.Columns where x.Value.ColumnName.Equals(pd.TableInfo.PrimaryKey, StringComparison.OrdinalIgnoreCase) select x.Value.PropertyInfo.Name).SingleOrDefault();
                        PropertySet(model, new Dictionary<string, object>()
                        {
                            { propertyName ,  idWorker.Next().ToString() } // Guid.NewGuid().ToString("N") }
                        });
                    }
                }

                var columns = new string[] { "create_on", "update_on" };
                foreach (var item in columns)
                {
                    if (pd.Columns.TryGetValue(item, out PocoColumn pc))
                    {
                        if (DateTime.Parse(pc.GetValue(model).ToString()) == new DateTime(1, 1, 1, 0, 0, 0))
                        {
                            string propertyName = (from x in pd.Columns where x.Value.ColumnName.Equals(item, StringComparison.OrdinalIgnoreCase) select x.Value.PropertyInfo.Name).SingleOrDefault();
                            PropertySet(model, new Dictionary<string, object>()
                            {
                                { propertyName , DateTime.Now }
                            });
                        }
                    }
                }

                object id = db.Insert((tableName.HasValue() ? tableName : pd.TableInfo.TableName), model);
                if (pk.PropertyInfo.PropertyType.Name.Equals("string", StringComparison.OrdinalIgnoreCase))
                {
                    return id.ToString().HasValue();
                }
                return long.Parse(id.ToString()) > 0;
            });
        }

        #endregion


        public TResult ExecuteScalar<TResult>(Sql sql)
        {
            return Execute((db) =>
            {
                return db.ExecuteScalar<TResult>(sql);
            });
        }

        public int Execute(Sql sql)
        {
            return this.Execute<int>((db) =>
            {
                return db.Execute(sql);
            });
        }

        private dynamic ArrayToObject(object[] args)
        {
            dynamic obj = null;
            if (args.Length > 0)
            {
                obj = new ExpandoObject();
                var args_kv = (obj as ICollection<KeyValuePair<string, object>>);
                for (int i = 0; i < args.Length; i++)
                {
                    args_kv.Add(new KeyValuePair<string, object>(i.ToString(), args[i]));
                }
            }
            return obj;
        }

        public void QueryMultiple(Sql sql, Action<Banana.GridReader> action)
        {
            this.Execute<bool>((db) =>
            {
                //if (sql.Arguments.Length > 0)
                //{
                //    args = new ExpandoObject();
                //    var args_kv = (args as ICollection<KeyValuePair<string, object>>);
                //    for (int i = 0; i < sql.Arguments.Length; i++)
                //    {
                //        args_kv.Add(new KeyValuePair<string, object>(i.ToString(), sql.Arguments[i]));
                //    }
                //}
                bool result = false;

                if (Interceptor != null)
                {
                    Interceptor.OnExecutingCommand(sql.ToString(), sql.Arguments);
                }

                dynamic args = ArrayToObject(sql.Arguments);
                var cmd = new CommandDefinition(sql.ToString(), args);
                using (var gridReader = db.Connection.QueryMultiple(cmd))
                {
                    if (!gridReader.IsConsumed)
                    {
                        action.Invoke(new Banana.GridReader(gridReader));
                        result = true;
                    }
                }
                return result;
            });
        }


        #region Page

        public PagingResult<T> Paging(int page, int limit, Sql sql)
        {
            return this.Paging<T>(page, limit, sql);
        }

        public PagingResult<U> Paging<U>(int page, int limit, Sql sql)
        {
            return _rdb.Page<U>(page, limit, sql);
        }

        public PagingResult<dynamic> PagingDynamic(int page, int limit, Sql sql)
        {
            return this.Execute((db) =>
            {
                string sqlCount, sqlPage;
                object[] args = sql.Arguments;
                db.BuildPageQueries((page - 1) * limit, limit, sql.ToString().TrimEnd(';'), ref args, out sqlCount, out sqlPage);

                string sqlFinal = string.Concat(sqlPage, ";", sqlCount, ";");

                var pageResult = new PagingResult<dynamic>();
                this.QueryMultiple(new Sql(sqlFinal, args), (reader) =>
                {
                    pageResult.Data = reader.Read().AsList();
                    pageResult.Count = reader.ReadSingle<int>();

                    pageResult.Page = page;
                    pageResult.Limit = limit;

                    pageResult.PageCount = pageResult.Count / limit;
                    if ((pageResult.Count % limit) != 0)
                        pageResult.PageCount++;

                });
                return pageResult;
            });
        }

        #endregion


        public TypeUnfold<T> Expand()
        {
            return ExecuteWithoutConnect((db) =>
            {
                var typeUnfold = new TypeUnfold<T>();
                typeUnfold.SetDatabase(db);
                return typeUnfold;
            });
        }
        public TypeUnfold<U> Expand<U>()
        {
            return ExecuteWithoutConnect((db) =>
            {
                var typeUnfold = new TypeUnfold<U>();
                typeUnfold.SetDatabase(db);
                return typeUnfold;
            });
        }

        public int Count(Sql sql)
        {
            return Execute((db) =>
            {
                return db.ExecuteScalar<int>(sql);
            });
        }

        public int Count<U>(Sql sql)
        {
            return Execute((db) =>
            {
                var finalSql = new Sql();
                bool autoSelect = sql.SQL.Trim().StartsWith("select", StringComparison.OrdinalIgnoreCase);
                //var match = new System.Text.RegularExpressions.Regex("select count", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                if (autoSelect)
                {
                    finalSql = sql;
                }
                else
                {
                    var poco = PocoData.ForType(typeof(U), db.DefaultMapper);
                    finalSql.Append($"select count(0) from {poco.TableInfo.TableName}").Append(sql);
                }

                return db.ExecuteScalar<int>(sql);
            });
        }

        public int Count(Expression<Func<T, bool>> where)
        {
            return this.Count<T>(where);
        }

        public int Count<U>(Expression<Func<U, bool>> where)
        {
            return Execute((db) =>
            {
                var poco = PocoData.ForType(typeof(U), db.DefaultMapper);
                string sql_parts = ExpressionHelper.ReplaceSpecialWords(ExpressionHelper.AnalyticExpression(where));
                var sql = Sql.Builder.Append($"select count(0) from {poco.TableInfo.TableName} where {sql_parts}");
                return db.ExecuteScalar<int>(sql);
            });
        }


        public DataTable GetDataTable(Sql sql)
        {
            return this.Execute((db) =>
            {
                var table = new DataTable();
                using (var cmd = db.CreateCommand(db.Connection, sql.SQL, sql.Arguments))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        db.OnExecutedCommand(cmd);
                        table.Load(reader);
                    }
                }
                return table;
            });
        }

        public PagingResult<DataTable> GetDataTable(int page, int limit, Sql sql)
        {
            return this.Execute((db) =>
            {
                var pageResult = new PagingResult<DataTable>()
                {
                    Page = page,
                    Limit = limit
                };

                string sqlCount, sqlPage;
                object[] args = sql.Arguments;
                db.BuildPageQueries((page - 1) * limit, limit, sql.SQL, ref args, out sqlCount, out sqlPage);

                db.KeepConnectionAlive = true;

                //1.get count
                pageResult.Count = db.ExecuteScalar<int>(sqlCount, args);

                if (pageResult.Count > 0)
                {
                    //2.get data
                    var table = new DataTable();
                    using (var cmd = db.CreateCommand(db.Connection, sql.SQL, args))
                    {
                        using (var reader = cmd.ExecuteReader())
                        {
                            db.OnExecutedCommand(cmd);
                            table.Load(reader);
                        }
                    }
                    pageResult.Data = new List<DataTable>() { table };
                }

                pageResult.PageCount = pageResult.Count / limit;
                if ((pageResult.Count % limit) != 0)
                    pageResult.PageCount++;

                return pageResult;
            });
        }

        public object[] GetArray(Sql sql)
        {
            return this.Execute((db) =>
            {
                object args = ArrayToObject(sql.Arguments);
                return db.Connection.Query(sql.ToString(), args).Select(x => ((IDictionary<string, object>)x).Values.FirstOrDefault()).ToArray();
            });
        }

        public void ChangeDatabase(string key, Action callback, Action<Exception> onException = null)
        {
            try
            {
                var dbs = Config.GetValue<JToken>("database");
                if (dbs != null)
                {
                    try
                    {
                        string databaseName = dbs.GetValue<string>(key);
                        if (databaseName.IsNullOrWhiteSpace())
                        {
                            throw new Exception($"配置项 database.{key} 不能为空");
                        }

                        this.Execute<bool>((db) =>
                        {
                            if (db.Connection != null)
                            {
                                db.Connection.ChangeDatabase(databaseName);
                                callback.Invoke();
                                return true;
                            }
                            return false;
                        });
                    }
                    catch (InvalidCastException)
                    {
                        throw new Exception("此方法只支持同连接字符串不同库的切换");
                    }
                }
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

        public void ChangeDatabase(string key, Action<IService<T, TId>> callback, Action<Exception> onException = null)
        {
            this.ChangeDatabase<T, TId>(key, callback, onException);
        }

        public void ChangeDatabase<U, UId>(string key, Action<IService<U, UId>> callback, Action<Exception> onException = null) where U : class, new()
        {
            try
            {
                var dbs = Config.GetValue<JToken>("database");
                if (dbs != null)
                {
                    try
                    {
                        string databaseName = dbs.GetValue<string>(key);
                        if (databaseName.IsNullOrWhiteSpace())
                        {
                            throw new Exception($"配置项 database.{key} 不能为空");
                        }

                        this.Execute<bool>((db) =>
                        {
                            if (db.Connection != null)
                            {
                                db.Connection.ChangeDatabase(databaseName);
                                callback.Invoke(new DataContext<U, UId>(db, Interceptor));
                                return true;
                            }
                            return false;
                        });
                    }
                    catch (InvalidCastException)
                    {
                        var that = dbs.GetValue<JToken>(key);
                        callback.Invoke(new DataContext<U, UId>(DatabaseInitializer.Instance.Create(that.GetString("connection"),
                            that.GetString("provider", "mysql")), Interceptor));
                    }
                }
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

        public void KeepConnection(Action callback)
        {
            this.Execute<bool>((db) =>
            {
                db.KeepConnectionAlive = true;
                callback.Invoke();
                db.KeepConnectionAlive = false;
                return true;
            });
        }

        public bool KeepConnection(Action callback, Action<Exception> onException)
        {
            return this.Execute<bool>((db) =>
            {
                try
                {
                    db.KeepConnectionAlive = true;
                    callback.Invoke();
                    return true;
                }
                catch (Exception ex)
                {
                    onException.Invoke(ex);
                }
                finally
                {
                    db.KeepConnectionAlive = false;
                }
                return false;
            });
        }

        public TResult KeepConnection<TResult>(Func<TResult> callback, Action<Exception> onException)
        {
            return Execute((db) =>
            {
                TResult value = default(TResult);
                try
                {
                    db.KeepConnectionAlive = true;
                    value = callback.Invoke();
                }
                catch (Exception ex)
                {
                    onException.Invoke(ex);
                }
                finally
                {
                    db.KeepConnectionAlive = false;
                }
                return value;
            });
        }

    }


    /// <summary>
    /// 类型解析
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class TypeUnfold<T>
    {
        private PocoData current_pd = null;
        private Database current_db = null;
        private bool additional_sql = false;//是否为追加的sql内容，为true时前面会追加逗号“,”
        private bool map_property_name = false;//是否自动添加属性名作为查询字段结果的别名
        private string global_alias_table = string.Empty;//全局表别名
        private bool no_alias_this_time = false;//本次取消列别名

        internal TypeUnfold()
        {
            //this.map_property_name = true;
        }

        internal void SetDatabase(Database db)
        {
            this.current_db = db;
            this.current_pd = PocoData.ForType(typeof(T), current_db.DefaultMapper);
            this.Alias = this.Table = this.current_pd.TableInfo.TableName;
        }

        /// <summary>
        /// 表名
        /// </summary>
        public string Table
        {
            get;
            private set;
        }
        /// <summary>
        /// 表别名
        /// </summary>
        public string Alias
        {
            get; private set;
        }

        /// <summary>
        /// 解析所有字段
        /// </summary>
        /// <returns></returns>
        public string Resolve()
        {
            return this.Resolve(null, string.Empty);
        }

        /// <summary>
        /// 解析，可设定单个或多个属性
        /// </summary>
        /// <param name="prefix">表的别名</param>
        /// <returns></returns>
        public string Resolve(string prefix)
        {
            return this.Resolve(columns: null, prefix: prefix);
        }
        /// <summary>
        /// 解析，可设定单个或多个属性
        /// </summary>
        /// <param name="columns">字段</param>
        /// <returns></returns>
        public string Resolve(Expression<Func<T, object>> columns)
        {
            return this.Resolve(columns, string.Empty);
        }
        /// <summary>
        /// 解析，可设定单个或多个属性
        /// </summary>
        /// <param name="columns">字段</param>
        /// <param name="prefix">表的别名</param>
        /// <returns></returns>
        public string Resolve(Expression<Func<T, object>> columns, string prefix)
        {
            return this.Resolve(columns, prefix, false);
        }
        /// <summary>
        /// 解析，可设定单个或多个属性
        /// </summary>
        /// <param name="columns">字段</param>
        /// <param name="prefix">表的别名</param>
        /// <param name="inverse">反转字段</param>
        /// <returns></returns>
        public string Resolve(Expression<Func<T, object>> columns, string prefix, bool inverse)
        {
            string[] sql_parts = GetMultiColumnForSql(columns, prefix, inverse);
            string final_sql = string.Join(", ", sql_parts);
            if (additional_sql)
            {
                final_sql = ", " + final_sql;
                additional_sql = false;
            }
            return final_sql;
        }

        internal string GetSingleColumnForSql(Expression<Func<T, object>> expression, string alias_column, string alias_table)
        {
            //var pd = PocoData.ForType(typeof(U), current_db.DefaultMapper);
            if (alias_table.IsNullOrWhiteSpace())
            {
                alias_table = global_alias_table;
            }
            alias_table = string.Concat("`", alias_table.HasValue() ? alias_table : current_pd.TableInfo.TableName, "`");

            var action = new Func<MemberInfo, string>((member) =>
            {
                string propertyName = string.Empty;
                if (!no_alias_this_time)
                {
                    propertyName = alias_column.HasValue() ? alias_column : (this.map_property_name ? member.Name : string.Empty);
                }
                var column = (ColumnAttribute)member.GetCustomAttribute(typeof(ColumnAttribute), false);
                StringBuilder builder = new StringBuilder();
                if (column == null)
                {
                    string name = member.Name;
                    builder.Append($"{(alias_table.HasValue() ? $"{alias_table}." : "")}`{name}`");
                }
                else
                {
                    string name = column.Name;
                    builder.Append($"{(alias_table.HasValue() ? $"{alias_table}." : "")}`{name}`");
                }
                builder.Append(propertyName.HasValue() ? $" AS '{propertyName}'" : "");
                return builder.ToString();
            });

            //if (expression.Body is NewExpression)
            //{
            //    foreach (var item in (expression.Body as NewExpression).Arguments)
            //    {
            //        var temp = item as MemberExpression;
            //        if (temp != null)
            //        {
            //            action.Invoke(temp.Member);
            //        }
            //    }
            //}
            //else 
            if (expression.Body is MemberExpression)
            {
                return action.Invoke((expression.Body as MemberExpression).Member);
            }
            //else if (expression.Body is NewArrayExpression)
            //{
            //    foreach (var item in (expression.Body as NewArrayExpression).Expressions)
            //    {
            //        var temp = item as MemberExpression;
            //        if (temp != null)
            //        {
            //            action.Invoke(temp.Member);
            //        }
            //    }
            //}
            else if (expression.Body is UnaryExpression)//一元表达式
            {
                return action.Invoke(((expression.Body as UnaryExpression).Operand as MemberExpression).Member);
            }
            else
            {
                throw new NotSupportedException($"Banana.Core：{expression.Body.NodeType} 不支持解析当前的表达式");
            }
        }
        internal string[] GetMultiColumnForSql(Expression<Func<T, object>> expression, string alias_table, bool inverse)
        {
            //var current_pd = PocoData.ForType(typeof(U), db.DefaultMapper);
            if (alias_table.IsNullOrWhiteSpace())
            {
                alias_table = global_alias_table;
            }
            alias_table = string.Concat("`", alias_table.HasValue() ? alias_table : current_pd.TableInfo.TableName, "`");
            //string primaryKey = pd.TableInfo.PrimaryKey;
            var columns_increase = new List<string>();
            var columns_decrease = (from x in current_pd.Columns select x.Value.ColumnName).ToList();
            //columns_decrease.RemoveAll(x => x.Equals(primaryKey, StringComparison.OrdinalIgnoreCase));

            var action = new Action<MemberInfo>((member) =>
            {
                string propertyName = string.Empty;
                if (!no_alias_this_time)
                {
                    propertyName = this.map_property_name ? member.Name : string.Empty;
                }
                var column = (ColumnAttribute)member.GetCustomAttribute(typeof(ColumnAttribute), false);
                StringBuilder builder = new StringBuilder();
                if (column == null)
                {
                    string name = member.Name;
                    //if (name.Equals(primaryKey, StringComparison.OrdinalIgnoreCase)) return;
                    if (inverse)
                    {
                        columns_decrease.RemoveAll(x => x.Equals(name, StringComparison.OrdinalIgnoreCase));
                    }
                    else
                    {
                        columns_increase.Add($"{(alias_table.HasValue() ? $"{alias_table}." : "")}`{name}`{(propertyName.HasValue() ? $" AS '{propertyName}'" : "")}");
                    }
                }
                else
                {
                    string name = column.Name;
                    //if (name.Equals(primaryKey, StringComparison.OrdinalIgnoreCase)) return;
                    if (inverse)
                    {
                        columns_decrease.RemoveAll(x => x.Equals(name, StringComparison.OrdinalIgnoreCase));
                    }
                    else
                    {
                        columns_increase.Add($"{(alias_table.HasValue() ? $"{alias_table}." : "")}`{name}`{(propertyName.HasValue() ? $" AS '{propertyName}'" : "")}");
                    }
                }
            });

            if (expression == null)
            {
                inverse = true;//空表达式时，反转字段即取所有字段
            }
            else
            {
                if (expression.Body is NewExpression)
                {
                    foreach (var item in (expression.Body as NewExpression).Arguments)
                    {
                        var temp = item as MemberExpression;
                        if (temp != null)
                        {
                            action.Invoke(temp.Member);
                        }
                    }
                }
                else if (expression.Body is MemberExpression)
                {
                    action.Invoke((expression.Body as MemberExpression).Member);
                }
                else if (expression.Body is NewArrayExpression)
                {
                    foreach (var item in (expression.Body as NewArrayExpression).Expressions)
                    {
                        var temp = item as MemberExpression;
                        if (temp != null)
                        {
                            action.Invoke(temp.Member);
                        }
                    }
                }
                else if (expression.Body is UnaryExpression)
                {
                    action.Invoke(((expression.Body as UnaryExpression).Operand as MemberExpression).Member);
                }
                else
                {
                    throw new NotSupportedException($"Banana.Core：{expression.Body.NodeType} 不支持解析当前的表达式");
                }
            }

            if (inverse)
            {
                for (int i = 0; i < columns_decrease.Count; i++)
                {
                    string item = columns_decrease[i];
                    var pc = current_pd.Columns[item];
                    if (pc.ResultColumn)
                    {
                        columns_decrease[i] = "";
                        continue;
                    }
                    string pname = this.map_property_name ? ($" as '{pc.PropertyInfo.Name}'") : "";
                    columns_decrease[i] = $"{(alias_table.HasValue() ? $"{alias_table}." : "")}`{item}`{pname}";
                }
                return columns_decrease.Where(x => x.HasValue()).ToArray();
            }
            else
            {
                return columns_increase.ToArray();
            }
        }

        /// <summary>
        /// 解析，可设定字段别名
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public string Resolve(Action<PropertyMapping<T>> action)
        {
            return this.Resolve(action, string.Empty);
        }
        /// <summary>
        /// 解析，可设定字段别名
        /// </summary>
        /// <param name="action"></param>
        /// <param name="prefix">表的别名</param>
        /// <returns></returns>
        public string Resolve(Action<PropertyMapping<T>> action, string prefix)
        {
            var propertyMapping = new PropertyMapping<T>();
            action.Invoke(propertyMapping);
            var maps = propertyMapping.Collect();
            var values = new List<string>();
            foreach (var item in maps.Keys)
            {
                values.Add(GetSingleColumnForSql(item, maps[item], prefix));
            }

            string final_sql = string.Join(", ", values);
            if (additional_sql)
            {
                final_sql = ", " + final_sql;
                additional_sql = false;
            }
            return final_sql;
        }

        /// <summary>
        /// 阻止添加属性名作为查询字段的别名
        /// </summary>
        /// <returns></returns>
        public TypeUnfold<T> MapPropertyName()
        {
            this.map_property_name = true;
            return this;
        }

        /// <summary>
        /// 追加额外的对象解析
        /// </summary>
        /// <returns></returns>
        public TypeUnfold<T> Additional()
        {
            additional_sql = true;
            return this;
        }

        /// <summary>
        /// 设置表别名
        /// </summary>
        /// <returns></returns>
        public TypeUnfold<T> SetAlias(string alias)
        {
            this.global_alias_table = alias;
            this.Alias = alias;
            return this;
        }

        /// <summary>
        /// 本次取消别名
        /// </summary>
        /// <returns></returns>
        public TypeUnfold<T> AliasNot()
        {
            this.no_alias_this_time = true;
            return this;
        }

        /// <summary>
        /// 根据属性名获取字段名
        /// </summary>
        /// <param name="propertyName">属性名</param>
        /// <returns>字段名</returns>
        public string ResolveColumn(string propertyName)
        {
            var pi = typeof(T).GetProperty(propertyName);
            if (pi == null)
            {
                return string.Empty;
            }
            var attr = pi.GetCustomAttribute(typeof(ColumnAttribute));
            if (attr == null)
            {
                //return pi.Name;
                return string.Empty;
            }
            return (attr as ColumnAttribute).Name;
        }

    }

    public sealed class PropertyMapping<T>
    {
        private Dictionary<Expression<Func<T, object>>, string>
            stores = new Dictionary<Expression<Func<T, object>>, string>();
        public void Property(Expression<Func<T, object>> property, string alias)
        {
            stores.Add(property, alias);
        }
        public void Property(Expression<Func<T, object>> property)
        {
            stores.Add(property, string.Empty);
        }

        internal Dictionary<Expression<Func<T, object>>, string> Collect()
        {
            return stores;
        }
    }

}
