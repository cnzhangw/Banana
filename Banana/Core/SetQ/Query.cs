﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Linq.Expressions;
using Banana.Core.Interfaces;
using Banana.Entites;
using Banana.Extension;
using Banana;

namespace Banana.Core.SetQ
{
    /// <summary>
    /// 查询
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class Query<T> : AbstractSet, IQuery<T>
    {
        public readonly IDbConnection DbCon;
        public readonly IDbTransaction DbTransaction;

        protected DataBaseContext<T> SetContext { get; set; }

        protected Query(IDbConnection conn, SqlProvider sqlProvider)
        {
            SqlProvider = sqlProvider;
            DbCon = conn;
        }

        protected Query(IDbConnection conn, SqlProvider sqlProvider, IDbTransaction dbTransaction)
        {
            SqlProvider = sqlProvider;
            DbCon = conn;
            DbTransaction = dbTransaction;
        }

        public T Get()
        {
            SqlProvider.FormatGet<T>();
            return DbCon.QueryFirstOrDefaults<T>(SqlProvider, DbTransaction);
        }

        public TSource Get<TSource>()
        {
            SqlProvider.FormatGet<T>();
            return DbCon.QueryFirstOrDefaults<TSource>(SqlProvider, DbTransaction);
        }

        public TReturn Get<TReturn>(Expression<Func<T, TReturn>> select)
        {
            SqlProvider.Context.Set.SelectExpression = select;
            SqlProvider.FormatGet<T>();
            return DbCon.QueryFirst_1<TReturn>(SqlProvider, DbTransaction);
        }

        public TReturn Get<TReturn>(bool where, Expression<Func<T, TReturn>> trueSelect, Expression<Func<T, TReturn>> falseSelect)
        {
            if (where)
                SqlProvider.Context.Set.SelectExpression = trueSelect;
            else
                SqlProvider.Context.Set.SelectExpression = falseSelect;
            SqlProvider.FormatGet<T>();
            return DbCon.QueryFirst_1<TReturn>(SqlProvider, DbTransaction);
        }

        public async Task<T> GetAsync()
        {
            SqlProvider.FormatGet<T>();
            return await DbCon.QueryFirstOrDefaultAsyncs<T>(SqlProvider, DbTransaction);
        }

        public async Task<TSource> GetAsync<TSource>()
        {
            SqlProvider.FormatGet<T>();
            return await DbCon.QueryFirstOrDefaultAsyncs<TSource>(SqlProvider, DbTransaction);
        }

        public async Task<TReturn> GetAsync<TReturn>(Expression<Func<T, TReturn>> select)
        {
            SqlProvider.Context.Set.SelectExpression = select;
            SqlProvider.FormatGet<T>();
            return await DbCon.QueryFirst_1Async<TReturn>(SqlProvider, DbTransaction);
        }

        public async Task<TReturn> GetAsync<TReturn>(bool where, Expression<Func<T, TReturn>> trueSelect, Expression<Func<T, TReturn>> falseSelect)
        {
            if (where)
                SqlProvider.Context.Set.SelectExpression = trueSelect;
            else
                SqlProvider.Context.Set.SelectExpression = falseSelect;
            SqlProvider.FormatGet<T>();
            return await DbCon.QueryFirst_1Async<TReturn>(SqlProvider, DbTransaction);
        }

        public IEnumerable<T> ToIEnumerable()
        {
            SqlProvider.FormatToList<T>();
            return DbCon.Query_1<T>(SqlProvider, DbTransaction);
        }

        public IEnumerable<TSource> ToIEnumerable<TSource>()
        {
            SqlProvider.FormatToList<T>();
            return DbCon.Query_1<TSource>(SqlProvider, DbTransaction);
        }

        public IEnumerable<TReturn> ToIEnumerable<TReturn>(Expression<Func<T, TReturn>> select)
        {
            SqlProvider.Context.Set.SelectExpression = select;
            SqlProvider.FormatToList<T>();
            return DbCon.Query_1<TReturn>(SqlProvider, DbTransaction);
        }

        public IEnumerable<TReturn> ToIEnumerable<TReturn>(bool where, Expression<Func<T, TReturn>> trueSelect, Expression<Func<T, TReturn>> falseSelect)
        {
            if (where)
                SqlProvider.Context.Set.SelectExpression = trueSelect;
            else
                SqlProvider.Context.Set.SelectExpression = falseSelect;
            SqlProvider.FormatToList<T>();
            return DbCon.Query_1<TReturn>(SqlProvider, DbTransaction);
        }

        public async Task<IEnumerable<T>> ToIEnumerableAsync()
        {
            SqlProvider.FormatToList<T>();
            return await DbCon.Query_1Async<T>(SqlProvider, DbTransaction);
        }

        public async Task<IEnumerable<TSource>> ToIEnumerableAsync<TSource>()
        {
            SqlProvider.FormatToList<T>();
            return await DbCon.Query_1Async<TSource>(SqlProvider, DbTransaction);
        }

        public async Task<IEnumerable<TReturn>> ToIEnumerableAsync<TReturn>(Expression<Func<T, TReturn>> select)
        {
            SqlProvider.Context.Set.SelectExpression = select;
            SqlProvider.FormatToList<T>();
            return await DbCon.Query_1Async<TReturn>(SqlProvider, DbTransaction);
        }

        public async Task<IEnumerable<TReturn>> ToIEnumerableAsync<TReturn>(bool where, Expression<Func<T, TReturn>> trueSelect, Expression<Func<T, TReturn>> falseSelect)
        {
            if (where)
                SqlProvider.Context.Set.SelectExpression = trueSelect;
            else
                SqlProvider.Context.Set.SelectExpression = falseSelect;
            SqlProvider.FormatToList<T>();
            return await DbCon.Query_1Async<TReturn>(SqlProvider, DbTransaction);
        }

        public List<T> ToList()
        {
            SqlProvider.FormatToList<T>();
            return DbCon.Query_1<T>(SqlProvider, DbTransaction).ToList();
        }

        public List<TSource> ToList<TSource>()
        {
            SqlProvider.FormatToList<T>();
            return DbCon.Query_1<TSource>(SqlProvider, DbTransaction).ToList();
        }

        public List<TReturn> ToList<TReturn>(Expression<Func<T, TReturn>> select)
        {
            SqlProvider.Context.Set.SelectExpression = select;
            SqlProvider.FormatToList<T>();
            return DbCon.Query_1<TReturn>(SqlProvider, DbTransaction);
        }

        public List<TReturn> ToList<TReturn>(bool where, Expression<Func<T, TReturn>> trueSelect, Expression<Func<T, TReturn>> falseSelect)
        {
            if (where)
                SqlProvider.Context.Set.SelectExpression = trueSelect;
            else
                SqlProvider.Context.Set.SelectExpression = falseSelect;
            SqlProvider.FormatToList<T>();
            return DbCon.Query_1<TReturn>(SqlProvider, DbTransaction);
        }

        public async Task<List<T>> ToListAsync()
        {
            SqlProvider.FormatToList<T>();
            return (await DbCon.QueryAsyncs<T>(SqlProvider, DbTransaction)).ToList();
        }

        public async Task<List<TSource>> ToListAsync<TSource>()
        {
            SqlProvider.FormatToList<T>();
            return (await DbCon.QueryAsyncs<TSource>(SqlProvider, DbTransaction)).ToList();
        }

        public async Task<List<TReturn>> ToListAsync<TReturn>(Expression<Func<T, TReturn>> select)
        {
            SqlProvider.Context.Set.SelectExpression = select;
            SqlProvider.FormatToList<T>();
            return await DbCon.Query_1Async<TReturn>(SqlProvider, DbTransaction);
        }

        public async Task<List<TReturn>> ToListAsync<TReturn>(bool where, Expression<Func<T, TReturn>> trueSelect, Expression<Func<T, TReturn>> falseSelect)
        {
            if (where)
                SqlProvider.Context.Set.SelectExpression = trueSelect;
            else
                SqlProvider.Context.Set.SelectExpression = falseSelect;
            SqlProvider.FormatToList<T>();
            return await DbCon.Query_1Async<TReturn>(SqlProvider, DbTransaction);
        }

        public List<T> Page(int pageIndex, int pageSize)
        {
            SqlProvider.FormatToPageList<T>(pageIndex, pageSize);
            return DbCon.Query_1<T>(SqlProvider, DbTransaction);
        }

        public List<TSource> Page<TSource>(int pageIndex, int pageSize)
        {
            SqlProvider.FormatToPageList<T>(pageIndex, pageSize);
            return DbCon.Query_1<TSource>(SqlProvider, DbTransaction);
        }

        public List<TReturn> Page<TReturn>(int pageIndex, int pageSize, Expression<Func<T, TReturn>> select)
        {
            SqlProvider.Context.Set.SelectExpression = select;
            SqlProvider.FormatToPageList<T>(pageIndex, pageSize);
            return DbCon.Query_1<TReturn>(SqlProvider, DbTransaction);
        }

        public List<TReturn> Page<TReturn>(int pageIndex, int pageSize, bool where, Expression<Func<T, TReturn>> trueSelect, Expression<Func<T, TReturn>> falseSelect)
        {
            if (where)
                SqlProvider.Context.Set.SelectExpression = trueSelect;
            else
                SqlProvider.Context.Set.SelectExpression = falseSelect;
            SqlProvider.FormatToPageList<T>(pageIndex, pageSize);
            return DbCon.Query_1<TReturn>(SqlProvider, DbTransaction);
        }

        public async Task<List<T>> PageAsync(int pageIndex, int pageSize)
        {
            SqlProvider.FormatToPageList<T>(pageIndex, pageSize);
            return await DbCon.Query_1Async<T>(SqlProvider, DbTransaction);
        }

        public async Task<List<TSource>> PageAsync<TSource>(int pageIndex, int pageSize)
        {
            SqlProvider.FormatToPageList<T>(pageIndex, pageSize);
            return await DbCon.Query_1Async<TSource>(SqlProvider, DbTransaction);
        }

        public async Task<List<TReturn>> PageAsync<TReturn>(int pageIndex, int pageSize, Expression<Func<T, TReturn>> select)
        {
            SqlProvider.Context.Set.SelectExpression = select;
            SqlProvider.FormatToPageList<T>(pageIndex, pageSize);
            return await DbCon.Query_1Async<TReturn>(SqlProvider, DbTransaction);
        }

        public async Task<List<TReturn>> PageAsync<TReturn>(int pageIndex, int pageSize, bool where, Expression<Func<T, TReturn>> trueSelect, Expression<Func<T, TReturn>> falseSelect)
        {
            if (where)
                SqlProvider.Context.Set.SelectExpression = trueSelect;
            else
                SqlProvider.Context.Set.SelectExpression = falseSelect;
            SqlProvider.FormatToPageList<T>(pageIndex, pageSize);
            return await DbCon.Query_1Async<TReturn>(SqlProvider, DbTransaction);
        }

        public PageList<T> PageList(int pageIndex, int pageSize)
        {
            //查询总行数
            SqlProvider.FormatCount();
            var pageTotal = DbCon.QuerySingles<int>(SqlProvider, DbTransaction);
            //查询数据
            List<T> itemList;
            SqlProvider.Clear();
            if (pageTotal != 0)
            {
                SqlProvider.FormatToPageList<T>(pageIndex, pageSize);
                itemList = DbCon.Query_1<T>(SqlProvider, DbTransaction);
            }
            else
            {
                itemList = new List<T>();
            }
            return new PageList<T>(pageIndex, pageSize, pageTotal, itemList);
        }

        public PageList<TSource> PageList<TSource>(int pageIndex, int pageSize)
        {
            //查询总行数
            SqlProvider.FormatCount();
            var pageTotal = DbCon.QuerySingles<int>(SqlProvider, DbTransaction);
            //查询数据
            List<TSource> itemList;
            SqlProvider.Clear();
            if (pageTotal != 0)
            {
                SqlProvider.FormatToPageList<T>(pageIndex, pageSize);
                itemList = DbCon.Query_1<TSource>(SqlProvider, DbTransaction);
            }
            else
            {
                itemList = new List<TSource>();
            }
            return new PageList<TSource>(pageIndex, pageSize, pageTotal, itemList);
        }

        public PageList<TReturn> PageList<TReturn>(int pageIndex, int pageSize, Expression<Func<T, TReturn>> select)
        {
            SqlProvider.Context.Set.SelectExpression = select;
            //查询总行数
            SqlProvider.FormatCount();
            var pageTotal = DbCon.QuerySingles<int>(SqlProvider, DbTransaction);
            //查询数据
            List<TReturn> itemList;
            SqlProvider.Clear();
            if (pageTotal != 0)
            {
                SqlProvider.FormatToPageList<T>(pageIndex, pageSize);
                itemList = DbCon.Query_1<TReturn>(SqlProvider, DbTransaction);
            }
            else
            {
                itemList = new List<TReturn>();
            }
            return new PageList<TReturn>(pageIndex, pageSize, pageTotal, itemList);
        }

        public PageList<TReturn> PageList<TReturn>(int pageIndex, int pageSize, bool where, Expression<Func<T, TReturn>> trueSelect, Expression<Func<T, TReturn>> falseSelect)
        {
            if (where)
                SqlProvider.Context.Set.SelectExpression = trueSelect;
            else
                SqlProvider.Context.Set.SelectExpression = falseSelect;
            //查询总行数
            SqlProvider.FormatCount();
            var pageTotal = DbCon.QuerySingles<int>(SqlProvider, DbTransaction);
            //查询数据
            List<TReturn> itemList;
            SqlProvider.Clear();
            if (pageTotal != 0)
            {
                SqlProvider.FormatToPageList<T>(pageIndex, pageSize);
                itemList = DbCon.Query_1<TReturn>(SqlProvider, DbTransaction);
            }
            else
            {
                itemList = new List<TReturn>();
            }
            return new PageList<TReturn>(pageIndex, pageSize, pageTotal, itemList);
        }

        public async Task<PageList<T>> PageListAsync(int pageIndex, int pageSize)
        {
            //查询总行数
            SqlProvider.FormatCount();
            var pageTotal = await DbCon.QuerySinglesAsync<int>(SqlProvider, DbTransaction);
            //查询数据
            List<T> itemList;
            SqlProvider.Clear();
            if (pageTotal != 0)
            {
                SqlProvider.FormatToPageList<T>(pageIndex, pageSize);
                itemList = await DbCon.Query_1Async<T>(SqlProvider, DbTransaction);
            }
            else
            {
                itemList = new List<T>();
            }
            return new PageList<T>(pageIndex, pageSize, pageTotal, itemList);
        }

        public async Task<PageList<TSource>> PageListAsync<TSource>(int pageIndex, int pageSize)
        {
            //查询总行数
            SqlProvider.FormatCount();
            var pageTotal = await DbCon.QuerySinglesAsync<int>(SqlProvider, DbTransaction);
            //查询数据
            List<TSource> itemList;
            SqlProvider.Clear();
            if (pageTotal != 0)
            {
                SqlProvider.FormatToPageList<T>(pageIndex, pageSize);
                itemList = await DbCon.Query_1Async<TSource>(SqlProvider, DbTransaction);
            }
            else
            {
                itemList = new List<TSource>();
            }
            return new PageList<TSource>(pageIndex, pageSize, pageTotal, itemList);
        }

        public async Task<PageList<TReturn>> PageListAsync<TReturn>(int pageIndex, int pageSize, Expression<Func<T, TReturn>> select)
        {
            SqlProvider.Context.Set.SelectExpression = select;
            //查询总行数
            SqlProvider.FormatCount();
            var pageTotal = await DbCon.QuerySinglesAsync<int>(SqlProvider, DbTransaction);
            //查询数据
            List<TReturn> itemList;
            SqlProvider.Clear();
            if (pageTotal != 0)
            {
                SqlProvider.FormatToPageList<T>(pageIndex, pageSize);
                itemList = await DbCon.Query_1Async<TReturn>(SqlProvider, DbTransaction);
            }
            else
            {
                itemList = new List<TReturn>();
            }
            return new PageList<TReturn>(pageIndex, pageSize, pageTotal, itemList);
        }

        public async Task<PageList<TReturn>> PageListAsync<TReturn>(int pageIndex, int pageSize, bool where, Expression<Func<T, TReturn>> trueSelect, Expression<Func<T, TReturn>> falseSelect)
        {
            if (where)
                SqlProvider.Context.Set.SelectExpression = trueSelect;
            else
                SqlProvider.Context.Set.SelectExpression = falseSelect;
            //查询总行数
            SqlProvider.FormatCount();
            var pageTotal = await DbCon.QuerySinglesAsync<int>(SqlProvider, DbTransaction);
            //查询数据
            List<TReturn> itemList;
            SqlProvider.Clear();
            if (pageTotal != 0)
            {
                SqlProvider.FormatToPageList<T>(pageIndex, pageSize);
                itemList = await DbCon.Query_1Async<TReturn>(SqlProvider, DbTransaction);
            }
            else
            {
                itemList = new List<TReturn>();
            }
            return new PageList<TReturn>(pageIndex, pageSize, pageTotal, itemList);
        }

        public DataSet ToDataSet(IDbDataAdapter dataAdapter = null)
        {
            SqlProvider.FormatToList<T>();
            return DbCon.QueryDataSets(SqlProvider, DbTransaction, dataAdapter);
        }

        public DataSet ToDataSet<TReturn>(Expression<Func<T, TReturn>> select, IDbDataAdapter dataAdapter = null)
        {
            SqlProvider.Context.Set.SelectExpression = select;
            SqlProvider.FormatToList<T>();
            return DbCon.QueryDataSets(SqlProvider, DbTransaction, dataAdapter);
        }

        public DataSet ToDataSet<TReturn>(bool where, Expression<Func<T, TReturn>> trueSelect, Expression<Func<T, TReturn>> falseSelect, IDbDataAdapter dataAdapter = null)
        {
            if (where)
                SqlProvider.Context.Set.SelectExpression = trueSelect;
            else
                SqlProvider.Context.Set.SelectExpression = falseSelect;
            SqlProvider.FormatToList<T>();
            return DbCon.QueryDataSets(SqlProvider, DbTransaction, dataAdapter);
        }

        public async Task<DataSet> ToDataSetAsync(IDbDataAdapter dataAdapter = null)
        {
            SqlProvider.FormatToList<T>();
            return await DbCon.QueryDataSetsAsync(SqlProvider, DbTransaction, dataAdapter);
        }

        public async Task<DataSet> ToDataSetAsync<TReturn>(Expression<Func<T, TReturn>> select, IDbDataAdapter dataAdapter = null)
        {
            SqlProvider.Context.Set.SelectExpression = select;
            SqlProvider.FormatToList<T>();
            return await DbCon.QueryDataSetsAsync(SqlProvider, DbTransaction, dataAdapter);
        }

        public async Task<DataSet> ToDataSetAsync<TReturn>(bool where, Expression<Func<T, TReturn>> trueSelect, Expression<Func<T, TReturn>> falseSelect, IDbDataAdapter dataAdapter = null)
        {
            if (where)
                SqlProvider.Context.Set.SelectExpression = trueSelect;
            else
                SqlProvider.Context.Set.SelectExpression = falseSelect;
            SqlProvider.FormatToList<T>();
            return await DbCon.QueryDataSetsAsync(SqlProvider, DbTransaction, dataAdapter);
        }
    }

    public class Query<T, TReturn> : Query<T>, IQuery<T, TReturn>
    {
        public Query(IDbConnection conn, SqlProvider sqlProvider) : base(conn, sqlProvider)
        {
        }

        public Query(IDbConnection conn, SqlProvider sqlProvider, IDbTransaction dbTransaction) : base(conn, sqlProvider, dbTransaction)
        {
        }


        public new List<TReturn> ToList()
        {
            return base.ToList<TReturn>();
        }
    }
}
