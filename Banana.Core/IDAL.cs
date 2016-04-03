using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Banana.Core
{
    using System.Data;
    using System.Linq.Expressions;
    public interface IDAL<T, TId>
        where T : DataContextModel
    {
        #region QUERY

        T Single(object primaryKey);

        T Single(Expression<Func<T, bool>> where);

        T Single(Sql sql);

        IList<T> Query(Expression<Func<T, bool>> where);

        IList<T> Query(Expression<Func<T, bool>> where, Expression<Func<T, object>> order, bool isDesc = true);

        DataTable GetDataTable(string sqlText);

        //DataTable GetDataTable(Sql sql);

        Page<T> QueryPage(Expression<Func<T, bool>> where, long pageIndex = 1, long pageSize = 20);

        Page<T> QueryPage(Sql sql, long pageIndex = 1, long pageSize = 20);

        IList<T> SkipTake(long skip, long take, Expression<Func<T, bool>> where);

        IList<T> SkipTake(long skip, long take, Expression<Func<T, bool>> where, Expression<Func<T, object>> order, bool isDesc = true);

        IList<T> SkipTake(long skip, long take, Sql sql);

        #endregion

        #region ADD & UPDATE

        TId Insert(T entity);

        bool Save(T entity);

        int Update(T entity);

        bool Update(List<T> entities);

        int Update(T entity, Expression<Func<T, object>> columns);

        bool Insert(List<T> entities);

        #endregion

        #region DELETE

        int Delete(Expression<Func<T, bool>> where);

        int Delete(object primaryKey);

        int Delete(T entity);

        bool Delete(List<T> entities);

        #endregion

        bool Transaction(Action<IDAL<T, TId>> action);

        int ExecuteProcedure(string procedureName);

        int ExecuteProcedure(string procedureName, Action<List<object>> action);

        int ExecuteProcedure(string procedureName, params object[] args);

    }
}
