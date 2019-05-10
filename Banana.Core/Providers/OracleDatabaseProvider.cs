// <copyright file="OracleDatabaseType.cs" company="PetaPoco - CollaboratingPlatypus">
//      Apache License, Version 2.0 https://github.com/CollaboratingPlatypus/PetaPoco/blob/master/LICENSE.txt
// </copyright>
// <author>PetaPoco - CollaboratingPlatypus</author>
// <date>2015/12/05</date>

using Banana.Internal;
using System;
using System.Data;
using System.Data.Common;
using System.Linq;

namespace Banana.Core.Providers
{
    public class OracleDatabaseProvider : DatabaseProvider
    {
        public override string GetParameterPrefix(string connectionString)
        {
            return ":";
        }

        public override void PreExecute(IDbCommand cmd)
        {
            cmd.GetType().GetProperty("BindByName").SetValue(cmd, true, null);
            cmd.GetType().GetProperty("InitialLONGFetchSize").SetValue(cmd, -1, null);
        }

        public override string BuildPageQuery(long skip, long take, SQLParts parts, ref object[] args)
        {
            if (parts.SqlSelectRemoved.StartsWith("*"))
                throw new Exception("Query must alias '*' when performing a paged query.\neg. select t.* from table t order by t.id");

            // Same deal as SQL Server
            //return Singleton<SqlServerDatabaseProvider>.Instance.BuildPageQuery(skip, take, parts, ref args);


            #region edit by zhangw on 2018-9-25 12:15:23

            var helper = (PagingHelper)PagingUtility;
            // when the query does not contain an "order by", it is very slow
            if (helper.SimpleRegexOrderBy.IsMatch(parts.SqlSelectRemoved))
            {
                var m = helper.SimpleRegexOrderBy.Match(parts.SqlSelectRemoved);
                if (m.Success)
                {
                    var g = m.Groups[0];
                    parts.SqlSelectRemoved = parts.SqlSelectRemoved.Substring(0, g.Index);
                }
            }
            if (helper.RegexDistinct.IsMatch(parts.SqlSelectRemoved))
            {
                parts.SqlSelectRemoved = "peta_inner.* FROM (SELECT " + parts.SqlSelectRemoved + ") peta_inner";
            }
            //var sqlPage = string.Format("SELECT * FROM (SELECT ROWNUM peta_rn, {0} {1}) peta_paged WHERE peta_rn > @{2} AND peta_rn <= @{3}", parts.SqlSelectRemoved, (parts.SqlOrderBy ?? ""), args.Length, args.Length + 1);
            // 存在order by排序时，使用rownum会导致分页数据错误，此处改为row_number() over () 代替 by zhangw 
            var sqlPage = string.Format("SELECT * FROM (SELECT ROW_NUMBER() OVER ({1}) peta_rn, {0}) peta_paged WHERE peta_rn > @{2} AND peta_rn <= @{3}", parts.SqlSelectRemoved, (parts.SqlOrderBy ?? ""), args.Length, args.Length + 1);
            args = args.Concat(new object[] { skip, skip + take }).ToArray();
            return sqlPage;

            #endregion


        }

        public override DbProviderFactory GetFactory()
        {
            // "Oracle.ManagedDataAccess.Client.OracleClientFactory, Oracle.ManagedDataAccess" is for Oracle.ManagedDataAccess.dll
            // "Oracle.DataAccess.Client.OracleClientFactory, Oracle.DataAccess" is for Oracle.DataAccess.dll

            string netcore_dll_path = string.Empty;
#if NETSTANDARD2_0
            netcore_dll_path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Oracle.ManagedDataAccess.dll");
            if (!System.IO.File.Exists(netcore_dll_path))
            {
                netcore_dll_path = string.Empty;
            }
#endif

            return GetFactory(netcore_dll_path, "Oracle.ManagedDataAccess.Client.OracleClientFactory, Oracle.ManagedDataAccess, Culture=neutral, PublicKeyToken=89b483f429c47342",
                              "Oracle.DataAccess.Client.OracleClientFactory, Oracle.DataAccess");
        }

        public override string EscapeSqlIdentifier(string sqlIdentifier)
        {
            return string.Format("\"{0}\"", sqlIdentifier.ToUpperInvariant());
        }

        public override string GetAutoIncrementExpression(TableInfo ti)
        {
            if (!string.IsNullOrEmpty(ti.SequenceName))
                return string.Format("{0}.nextval", ti.SequenceName);

            return null;
        }

        public override object ExecuteInsert(Database db, IDbCommand cmd, string primaryKeyName)
        {
            if (primaryKeyName != null)
            {
                cmd.CommandText += string.Format(" returning {0} into :newid", EscapeSqlIdentifier(primaryKeyName));
                var param = cmd.CreateParameter();
                param.ParameterName = ":newid";
                param.Value = DBNull.Value;
                param.Direction = ParameterDirection.ReturnValue;
                param.DbType = DbType.Int64;
                cmd.Parameters.Add(param);
                ExecuteNonQueryHelper(db, cmd);
                return param.Value;
            }
            else
            {
                ExecuteNonQueryHelper(db, cmd);
                return -1;
            }
        }
    }
}