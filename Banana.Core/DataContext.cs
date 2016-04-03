using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Banana.Core
{
    using System.Linq.Expressions;
    using System.Reflection;
    using Poco;
    using System.Diagnostics;
    using System.Data;
    using System.Data.Common;

    abstract class DataContext<T, TId> : IDAL<T, TId>, IDisposable
        where T : DataContextModel, new()
    {
        private Database DBContext = null;
        private static Dictionary<Type, TableInfo> tableInfos = new Dictionary<Type, TableInfo>();

        public DataContext()
        {
            this.DBContext = new AbstractDataAccess();
        }

        public DataContext(Database context)
        {
            this.DBContext = context;
        }

        public void Dispose()
        {
            tableInfos.Clear();

            if (DBContext != null)
            {
                DBContext.Dispose();
            }
        }

        private TableInfo GetTableInfo()
        {
            TableInfo tableInfo = null;
            if (tableInfos.TryGetValue(typeof(T), out tableInfo))
            {
                return tableInfo;
            }

            tableInfo = new TableInfo();

            var a = typeof(T).GetCustomAttributes(typeof(TableNameAttribute), true);
            tableInfo.TableName = a.Length == 0 ? typeof(T).Name : (a.FirstOrDefault() as TableNameAttribute).Value;
            var b = typeof(T).GetCustomAttributes(typeof(PrimaryKeyAttribute), true);
            tableInfo.PrimaryKey = b.Length == 0 ? "Id" : (b.FirstOrDefault() as PrimaryKeyAttribute).Value;
            tableInfo.AutoIncrement = b.Length == 0 ? true : (b.FirstOrDefault() as PrimaryKeyAttribute).autoIncrement;
            tableInfo.SequenceName = b.Length == 0 ? null : (b.FirstOrDefault() as PrimaryKeyAttribute).sequenceName;

            tableInfos.Add(typeof(T), tableInfo);

            return tableInfo;
        }

        #region 解析表达式

        /// <summary>
        /// 将表达式转换为sql
        /// </summary>
        /// <param name="where">expression表达式</param>
        /// <returns></returns>
        protected virtual string AnalyticExpression(Expression where, bool recursion = true)
        {
            if (where == null) return "";

            if (where is BinaryExpression)
            {
                BinaryExpression exp = (BinaryExpression)where;
                if (recursion)
                {
                    if (exp.NodeType == ExpressionType.Or || exp.NodeType == ExpressionType.OrElse)
                    {
                        return string.Concat("( ", BinaryExpressionConvert(exp), " )");
                    }
                }
                return BinaryExpressionConvert(exp);
            }
            else if (where is MemberExpression)
            {
                MemberExpression exp = (MemberExpression)where;
                return MemberExpressionConvert(exp);
            }
            else if (where is MethodCallExpression)
            {
                MethodCallExpression exp = (MethodCallExpression)where;

                #region MethodCall

                StringBuilder sb = new StringBuilder();

                string field = AnalyticExpression(exp.Object);
                string value = AnalyticExpression(exp.Arguments.FirstOrDefault());

                switch (exp.Method.Name)
                {
                    case "Contains":
                        sb.Append(string.Concat(field, " LIKE '%", value.Replace("'", ""), "%' "));
                        break;
                    case "StartsWith":
                        sb.Append(string.Concat(field, " LIKE '", value.Replace("'", ""), "%' "));
                        break;
                    case "EndsWith":
                        sb.Append(string.Concat(field, " LIKE '%", value.Replace("'", ""), "' "));
                        break;
                    case "Equals":
                        sb.Append(string.Concat(field, " = ", value));
                        break;
                    case "IsNullOrEmpty":
                    case "IsNullOrWhiteSpace":
                        sb.Append(string.Concat(value, " = ''"));
                        break;
                    case "Trim":
                        sb.Append(string.Concat("TRIM(", field, ")"));
                        break;
                    case "TrimStart":
                        sb.Append(string.Concat("LTRIM(", field, ")"));
                        break;
                    case "TrimEnd":
                        sb.Append(string.Concat("RTRIM(", field, ")"));
                        break;
                    default:
                        break;
                }
                return sb.ToString();
                #endregion
            }
            else if (where is ConstantExpression)
            {
                ConstantExpression exp = (ConstantExpression)where;
                if (exp.Type == typeof(string))
                {
                    return "'" + exp.Value + "'";
                }
                else if (exp.Type == typeof(bool))
                {
                    return exp.Value.ToString().ToUpper();
                }
                else
                {
                    return exp.Value.ToString();
                }
            }
            else if (where is UnaryExpression)
            {
                UnaryExpression exp = (UnaryExpression)where;
                string sqlText = AnalyticExpression(exp.Operand);
                if (exp.NodeType == ExpressionType.Not || exp.NodeType == ExpressionType.NotEqual)
                {
                    sqlText = sqlText.Replace("=", "<>").Replace("LIKE", "NOT LIKE");
                }

                return sqlText;
            }
            else if (where is NewArrayExpression)
            {
                NewArrayExpression exp = (NewArrayExpression)where;
                StringBuilder sb = new StringBuilder();
                foreach (Expression item in exp.Expressions)
                {
                    sb.Append(AnalyticExpression(item));
                    sb.Append(",");
                }
                if (sb.Length > 0)
                {
                    return sb.ToString(0, sb.Length - 1);
                }
                return "";
            }
            else if (where is LambdaExpression)
            {
                LambdaExpression exp = (LambdaExpression)where;
                return AnalyticExpression(exp.Body);
            }
            else if (where is NewExpression)
            {
                NewExpression exp = (NewExpression)where;
                StringBuilder sb = new StringBuilder();
                foreach (var item in exp.Members)
                {
                    sb.Append(item.Name);
                    sb.Append(",");
                }
                if (sb.Length > 0)
                {
                    return sb.ToString(0, sb.Length - 1);
                }
                return "";
            }

            return "";
        }
        /// <summary>
        /// 表达式类型
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        protected virtual string ExpressionTypeCast(ExpressionType type)
        {
            switch (type)
            {
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                    return " AND ";
                case ExpressionType.Equal:
                    return " = ";
                case ExpressionType.GreaterThan:
                    return " > ";
                case ExpressionType.GreaterThanOrEqual:
                    return " >= ";
                case ExpressionType.LessThan:
                    return " < ";
                case ExpressionType.LessThanOrEqual:
                    return " <= ";
                case ExpressionType.Not:
                case ExpressionType.NotEqual:
                    return " <> ";
                case ExpressionType.Or:
                case ExpressionType.OrElse:
                    return " OR ";
                case ExpressionType.Add:
                case ExpressionType.AddChecked:
                    return " + ";
                case ExpressionType.Subtract:
                case ExpressionType.SubtractChecked:
                    return " - ";
                case ExpressionType.Divide:
                    return " / ";
                case ExpressionType.Multiply:
                case ExpressionType.MultiplyChecked:
                    return " * ";
                default:
                    throw new ArgumentException("Invalid lambda expression");
                //return null;
            }
        }
        /// <summary>
        /// 二叉树表达式
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        protected string BinaryExpressionConvert(BinaryExpression expression)
        {
            string leftText = AnalyticExpression(expression.Left);
            string symbolText = ExpressionTypeCast(expression.NodeType);
            string rightText = AnalyticExpression(expression.Right);
            return string.Concat(leftText, symbolText, rightText);
        }
        /// <summary>
        /// 成员表达式
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        protected virtual string MemberExpressionConvert(MemberExpression expression)
        {
            if (expression.Expression is ParameterExpression)
            {
                return expression.Member.Name;
            }

            object result = Expression.Lambda(expression).Compile().DynamicInvoke();
            if (result == null) result = string.Empty;

            if (result is string[] || result is String[])
            {
                return " '" + string.Join("','", (string[])result) + "' ";
            }
            else if (result is List<string>)
            {
                return " '" + string.Join("','", (List<string>)result) + "' ";
            }
            else if (result is int[])
            {
                return string.Join(",", (int[])result);
            }
            else if (result is List<int>)
            {
                return string.Join(",", (List<int>)result);
            }
            else if (result is long[])
            {
                return string.Join(",", (long[])result);
            }
            return string.Format("'{0}'", result);
        }

        #endregion

        #region QUERY

        public T Single(object primaryKey)
        {
            return DBContext.SingleOrDefault<T>(primaryKey);
        }

        public T Single(Expression<Func<T, bool>> where)
        {
            string sqlText = string.Format("WHERE {0}", AnalyticExpression(where.Body, false));
            return DBContext.FirstOrDefault<T>(new Sql(sqlText));
        }

        public T Single(Sql sql)
        {
            return DBContext.FirstOrDefault<T>(sql);
        }

        public IList<T> Query(Expression<Func<T, bool>> where)
        {
            return this.Query(where, null);
        }

        public IList<T> Query(Expression<Func<T, bool>> where, Expression<Func<T, object>> order, bool isDesc = true)
        {
            string sqlText = string.Format("SELECT * FROM {0} WHERE {1}", GetTableInfo().TableName, AnalyticExpression(where.Body, false));
            if (order != null)
            {
                string orderText = AnalyticExpression(order.Body, false);
                sqlText += " ORDER BY " + orderText;
                sqlText += isDesc ? " DESC" : " ASC";
            }
            Debug.WriteLine("sql：" + sqlText);
            return DBContext.Query<T>(new Sql(sqlText)).ToList();
        }

        public DataTable GetDataTable(string sqlText)
        {
            DataTable table = new DataTable("table0");
            try
            {
                DBContext.OpenSharedConnection();
                using (var cmd = DBContext.CreateCommand(DBContext.GetConnection(), sqlText))
                {
                    table.Load(cmd.ExecuteReader());
                    //using (DbDataAdapter dbDataAdapter = DBContext.GetFactory().CreateDataAdapter())
                    //{
                    //    dbDataAdapter.SelectCommand = (DbCommand)cmd;
                    //    dbDataAdapter.Fill(table);
                    //}
                }
            }
            catch (Exception ex) { }
            finally
            {
                DBContext.CloseSharedConnection();
            }
            return table;
        }

        //public DataTable GetDataTable(Sql sql)
        //{

        //    return GetDataTable(sql.SQL, sql.Arguments);
        //}

        public Page<T> QueryPage(Expression<Func<T, bool>> where, long pageIndex = 1, long pageSize = 20)
        {
            string sqlText = string.Format("WHERE {1}", GetTableInfo().TableName, AnalyticExpression(where.Body, false));
            return DBContext.Page<T>(pageIndex, pageSize, new Sql(sqlText));
        }

        public Page<T> QueryPage(Sql sql, long pageIndex = 1, long pageSize = 20)
        {
            return DBContext.Page<T>(pageIndex, pageSize, sql);
        }

        public IList<T> SkipTake(long skip, long take, Expression<Func<T, bool>> where)
        {
            return SkipTake(skip, take, where, null);
        }

        public IList<T> SkipTake(long skip, long take, Expression<Func<T, bool>> where, Expression<Func<T, object>> order, bool isDesc = true)
        {
            string sqlText = string.Format("WHERE {1}", GetTableInfo().TableName, AnalyticExpression(where.Body, false));
            if (order != null)
            {
                string orderText = AnalyticExpression(order.Body, false);
                sqlText += " ORDER BY " + orderText;
                sqlText += isDesc ? " DESC" : " ASC";
            }
            return DBContext.SkipTake<T>(skip, take, new Sql(sqlText));
        }

        public IList<T> SkipTake(long skip, long take, Sql sql)
        {
            return DBContext.SkipTake<T>(skip, take, sql);
        }

        #endregion

        #region ADD & UPDATE

        public TId Insert(T entity)
        {
            return (TId)DBContext.Insert(entity);
        }

        public bool Insert(List<T> entities)
        {
            return Transaction((e) =>
            {
                foreach (var item in entities)
                {
                    e.Insert(item);
                }
            });
        }

        public bool Save(T entity)
        {
            try
            {
                DBContext.Save(entity);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public int Update(T entity)
        {
            return DBContext.Update(entity);
        }

        public bool Update(List<T> entities)
        {
            return Transaction((e) =>
            {
                foreach (var item in entities)
                {
                    e.Update(item);
                }
            });
        }

        public int Update(T entity, Expression<Func<T, object>> columns)
        {
            //DBContext.Update<T>(new Sql("where id=@id", new { id = 41 }));
            string columnText = AnalyticExpression(columns, true);
            return DBContext.Update(entity, columnText.Split(','));
        }

        #endregion

        #region DELETE

        public int Delete(Expression<Func<T, bool>> where)
        {
            string sqlText = string.Format("WHERE {1}", GetTableInfo().TableName, AnalyticExpression(where.Body, false));
            return DBContext.Delete<T>(new Sql(sqlText));
        }

        public int Delete(object primaryKey)
        {
            return DBContext.Delete<T>(primaryKey);
        }

        public int Delete(T entity)
        {
            return DBContext.Delete<T>(entity);
        }

        public bool Delete(List<T> entities)
        {
            return Transaction((e) =>
            {
                foreach (var item in entities)
                {
                    e.Delete(item);
                }
            });
        }

        #endregion

        public virtual bool Transaction(Action<IDAL<T, TId>> action)
        {
            bool result = false;
            using (var scope = DBContext.GetTransaction())
            {
                try
                {
                    action.Invoke(this);
                    scope.Complete();
                    result = true;
                }
                catch (Exception ex)
                {
                    scope.Dispose();
                }
            }
            return result;
        }

        public int ExecuteProcedure(string procedureName)
        {
            return this.ExecuteProcedure(procedureName, e => { });
        }

        public int ExecuteProcedure(string procedureName, params object[] args)
        {
            return this.ExecuteProcedure(procedureName, e =>
            {
                if (args != null && args.Length > 0)
                {
                    foreach (var item in args)
                    {
                        e.Add(item);
                    }
                }
            });
        }

        public virtual int ExecuteProcedure(string procedureName, Action<List<object>> action)
        {
            int result = 0;
            try
            {
                List<object> args = new List<object>();
                if (action != null)
                {
                    action.Invoke(args);
                }

                StringBuilder sb = new StringBuilder("CALL ");
                sb.Append(procedureName + "(");
                if (args != null && args.Count > 0)
                {
                    foreach (var item in args)
                    {
                        sb.Append(item + ",");
                    }
                    sb = sb.Remove(sb.Length - 1, 1);
                    sb.Append(");");
                }
                result = DBContext.Execute(sb.ToString());
                //DBContext.OpenSharedConnection();
                //using (IDbCommand cmd = DBContext.CreateCommand(DBContext.GetConnection(), procedureName))
                //{
                //    cmd.CommandType = CommandType.StoredProcedure;                    
                //    if (action != null)
                //    {
                //        action.Invoke(cmd.Parameters);
                //    }

                //    result = cmd.ExecuteNonQuery();
                //}
            }
            catch (Exception ex) { }
            return result;
        }

    }

    abstract class DataContext<T> : DataContext<T, int> where T : DataContextModel, new()
    {
    }

}
