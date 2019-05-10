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
        bool Delete<U>(TId id);
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
        bool Update<U>(string tableName, U model);
        bool Update<U>(U model, Expression<Func<U, object>> columns);
        bool Update<U>(U model, Expression<Func<U, object>> columns, bool inverse);
        /// <summary>
        /// 更新
        /// </summary>
        /// <typeparam name="U"></typeparam>
        /// <param name="model"></param>
        /// <param name="columns">需更新的字段</param>
        /// <param name="inverse">反选前面所选择的字段（会跳过主键，无需处理）</param>
        /// <param name="tableName">表名</param>
        /// <returns></returns>
        bool Update<U>(string tableName, U model, Expression<Func<U, object>> columns, bool inverse);

        int Update<U>(U model, Expression<Func<U, object>> columns, Expression<Func<U, bool>> where);
        int Update<U>(U model, Expression<Func<U, object>> columns, Expression<Func<U, bool>> where, bool inverse);
        int Update<U>(string tableName, U model, Expression<Func<U, object>> columns, Expression<Func<U, bool>> where);
        int Update<U>(string tableName, U model, Expression<Func<U, object>> columns, Expression<Func<U, bool>> where, bool inverse);

        #endregion

        #region Insert

        bool Insert(T model);
        bool Insert<U>(U model);
        /// <summary>
        /// 插入
        /// </summary>
        /// <typeparam name="U"></typeparam>
        /// <param name="model"></param>
        /// <param name="tableName">表名</param>
        /// <returns></returns>
        bool Insert<U>(string tableName, U model);

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
        PagingResult<T> Paging(int page, int limit, Sql sql);

        /// <summary>
        /// 分页查询
        /// </summary>
        /// <param name="page">当前页码</param>
        /// <param name="limit">每页数据量</param>
        /// <param name="sql">查询sql</param>
        /// <returns></returns>
        PagingResult<U> Paging<U>(int page, int limit, Sql sql);

        /// <summary>
        /// 动态类型分页
        /// </summary>
        /// <param name="page">当前页码</param>
        /// <param name="limit">每页数据量</param>
        /// <param name="sql">查询sql</param>
        /// <returns></returns>
        PagingResult<dynamic> PagingDynamic(int page, int limit, Sql sql);


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
        int Count<U>(Sql sql);
        int Count(Expression<Func<T, bool>> where);
        int Count<U>(Expression<Func<U, bool>> where);


        DataTable GetDataTable(Sql sql);

        [Obsolete("推荐使用PageDynamic")]
        PagingResult<DataTable> GetDataTable(int page, int limit, Sql sql);

        /// <summary>
        /// 获取单列数据以数据返回
        /// </summary>
        /// <param name="sql">如：select name from xxx;</param>
        /// <returns></returns>
        object[] GetArray(Sql sql);

        /// <summary>
        /// 切换数据库，适用于 相同连接字符串不同库 的情况
        /// </summary>
        /// <param name="key">banana.json中的database节点的数据库key，值为数据库名</param>
        /// <param name="callback">成功切换数据库后的回调</param>
        /// <param name="onException">异常回调，为空时异常发生会向上抛出</param>
        [Obsolete("推荐使用BaseService基类中的ChangeDatabase函数", true)]
        void ChangeDatabase(string key, Action callback, Action<Exception> onException = null);

        /// <summary>
        /// 切换数据库，适用于 不同连接字符串/不同数据库类型 的情况
        /// </summary>
        /// <param name="key">banana.json中的database节点的数据库key，值为数据库名</param>
        /// <param name="callback">成功切换数据库后的回调</param>
        /// <param name="onException">异常回调，为空时异常发生会向上抛出</param>
        [Obsolete("推荐使用BaseService基类中的ChangeDatabase函数", true)]
        void ChangeDatabase(string key, Action<IService<T, TId>> callback, Action<Exception> onException = null);

        /// <summary>
        /// 切换数据库，适用于 不同连接字符串/不同数据库类型 的情况
        /// </summary>
        /// <param name="key">banana.json中的database节点的数据库key，值为数据库名</param>
        /// <param name="callback">成功切换数据库后的回调</param>
        /// <param name="onException">异常回调，为空时异常发生会向上抛出</param>
        void ChangeDatabase<U, UId>(string key, Action<IService<U, UId>> callback, Action<Exception> onException = null) where U : class, new();

        /// <summary>
        /// 保持数据库连接，适用于多个查询集，避免频繁开关数据库连接
        /// </summary>
        /// <param name="callback">查询集回调</param>
        void KeepConnection(Action callback);

        /// <summary>
        /// 保持数据库连接，适用于多个查询集，避免频繁开关数据库连接
        /// </summary>
        /// <param name="callback">查询集回调</param>
        /// <param name="onException">异常回调，为空时异常发生会向上抛出</param>
        /// <returns></returns>
        bool KeepConnection(Action callback, Action<Exception> onException);

        /// <summary>
        /// 保持数据库连接，适用于多个查询集，避免频繁开关数据库连接
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="callback"></param>
        /// <param name="onException"></param>
        /// <returns></returns>
        TResult KeepConnection<TResult>(Func<TResult> callback, Action<Exception> onException);
    }
}
