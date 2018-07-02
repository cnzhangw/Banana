using Banana.Core.Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Banana.Core
{
    /// <summary>
    /// 说明：IService
    /// 作者：张炜 
    /// 时间：2018/5/13 7:52:00
    /// Email:cnzhangw@sina.com
    /// CLR版本：4.0.30319.42000
    /// 唯一标识：336c349f-a47d-4024-bc88-c51523382d78
    /// </summary>
    public interface IService<T, TId>
    {

        #region Single

        T Single(TId id);
        T Single<Tid>(Tid id);

        U Single<U>(TId id);
        U Single<U, Tid>(Tid id);

        T Single(Expression<Func<T, bool>> where);
        U Single<U>(Expression<Func<U, bool>> where);

        T Single(Sql sql);
        U Single<U>(Sql sql);

        dynamic SingleDynamic(Sql sql);

        #endregion

        #region Query

        IEnumerable<T> Query();
        IEnumerable<U> Query<U>();

        IEnumerable<T> Query(Sql sql);
        IEnumerable<U> Query<U>(Sql sql);

        IEnumerable<T> Query(Expression<Func<T, bool>> where);
        IEnumerable<U> Query<U>(Expression<Func<U, bool>> where);

        #endregion

        #region Fetch

        List<T> Fetch(Sql sql);
        List<U> Fetch<U>(Sql sql);

        List<T> Fetch(Expression<Func<T, bool>> where);
        List<U> Fetch<U>(Expression<Func<U, bool>> where);

        List<T> Fetch(long page, long limit, Sql sql);
        List<U> Fetch<U>(long page, long limit, Sql sql);

        List<dynamic> FetchDynamic(Sql sql);

        #endregion

        #region Delete

        bool Delete(T model);
        bool Delete<U>(U model);

        bool Delete(TId id);
        bool Delete<U, UId>(UId id);

        int Delete(Sql sql);
        int Delete<U>(Sql sql);

        int Delete(Expression<Func<T, bool>> where);
        int Delete<U>(Expression<Func<U, bool>> where);

        int Delete(TId[] ids);
        int Delete<U, UId>(UId[] ids);

        #endregion

        #region Update

        bool Update(T model);
        bool Update(T model, Expression<Func<T, object>> columns);
        bool Update(T model, Expression<Func<T, object>> columns, bool inverse);

        bool Update<U>(U model);
        bool Update<U>(U model, Expression<Func<U, object>> columns);
        bool Update<U>(U model, Expression<Func<U, object>> columns, bool inverse);

        #endregion

        #region Insert

        bool Insert(T model);
        bool Insert<U>(U model);

        #endregion

        /// <summary>
        /// 执行一个查询，返回单个结果值，如：select count(0) from table1;
        /// </summary>
        /// <typeparam name="TResult">结果类型，如：int</typeparam>
        /// <param name="sql"></param>
        /// <returns></returns>
        TResult ExecuteScalar<TResult>(Sql sql);

        /// <summary>
        /// 执行Insert/Update/Delete语句
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        int Execute(Sql sql);

        /// <summary>
        /// 多结果集查询
        /// </summary>
        /// <param name="sql">示例：select * from table1;select * from table2;</param>
        /// <param name="action">示例：var users = reader.Read&lt;User&gt;();</param>
        void QueryMultiple(Sql sql, Action<Banana.GridReader> action);

        /// <summary>
        /// 分页查询
        /// </summary>
        /// <param name="page">当前页码</param>
        /// <param name="limit">每页数据量</param>
        /// <param name="sql">查询sql（结尾处不可加 ; ）</param>
        /// <returns></returns>
        Pager<T> Page(int page, int limit, Sql sql);

        /// <summary>
        /// 分页查询
        /// </summary>
        /// <param name="page">当前页码</param>
        /// <param name="limit">每页数据量</param>
        /// <param name="sql">查询sql</param>
        /// <returns></returns>
        Pager<U> Page<U>(int page, int limit, Sql sql);

        /// <summary>
        /// 动态类型分页
        /// </summary>
        /// <param name="page">当前页码</param>
        /// <param name="limit">每页数据量</param>
        /// <param name="sql">查询sql</param>
        /// <returns></returns>
        Pager<dynamic> PageDynamic(int page, int limit, Sql sql);


        /// <summary>
        /// 获取类型解析对象
        /// </summary>
        /// <returns></returns>
        TypeUnfold<T> Expand();
        /// <summary>
        /// 获取类型解析对象
        /// </summary>
        /// <typeparam name="U"></typeparam>
        /// <returns></returns>
        TypeUnfold<U> Expand<U>();
        //string Expand<U>(Expression<Func<U, object>> columns, string prefix, bool inverse);
        //string Expand<U>(Expression<Func<U, object>> columns, string prefix = "");
        //string Expand(Expression<Func<T, object>> columns, string prefix = "");
        //string Expand(Expression<Func<T, object>> columns, string prefix, bool inverse);


        int Count(Sql sql);
        int Count(Expression<Func<T, bool>> where);
        int Count<U>(Expression<Func<U, bool>> where);


        DataTable GetDataTable(Sql sql);

        [Obsolete("推荐使用PageDynamic")]
        Pager<DataTable> GetDataTable(int page, int limit, Sql sql);

        /// <summary>
        /// 获取单列数据以数据返回
        /// </summary>
        /// <param name="sql">如：select name from xxx;</param>
        /// <returns></returns>
        object[] GetArray(Sql sql);

        /// <summary>
        /// 切换上下文数据库
        /// </summary>
        /// <param name="key">banana.json中的dbs节点的数据库key，值为数据库名</param>
        /// <param name="callback">成功切换数据库后的回调</param>
        void ChangeDatabase(string key, Action callback);

        void KeepConnection(Action callback);

        
    }
}
