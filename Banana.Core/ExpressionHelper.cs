using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Banana.Core
{
    using System.Linq.Expressions;
    internal class ExpressionHelper
    {

        internal static Interface.IProvider Provider
        {
            get; set;
        }

        #region 解析表达式

        /// <summary>
        /// 解析表达式为sql（注：前面不含where）
        /// </summary>
        /// <param name="where">expression表达式</param>
        /// <param name="recursion">是否递归，默认true</param>
        /// <returns></returns>
        public static string AnalyticExpression(Expression where, bool recursion = true)
        {
            if (where == null) { return ""; }

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
                        if (!field.HasValue())
                        {
                            if (exp.Arguments.Count > 1)
                            {
                                field = AnalyticExpression(exp.Arguments[1]);
                            }
                            sb.Append(string.Concat(field, " in(", value, ") "));
                        }
                        else if (value.StartsWith("substr"))
                        {
                            sb.Append($"{field} like concat('%',{value},'%')");
                        }
                        else
                        {
                            sb.Append(string.Concat(field, " like '%", value.Replace("'", ""), "%' "));
                        }
                        break;
                    case "StartsWith":
                        sb.Append(string.Concat(field, " like '", value.Replace("'", ""), "%' "));
                        break;
                    case "EndsWith":
                        sb.Append(string.Concat(field, " like '%", value.Replace("'", ""), "' "));
                        break;
                    case "Equals":
                        sb.Append(string.Concat(field, " = ", value));
                        break;
                    case "IsNullOrEmpty":
                    case "IsNullOrWhiteSpace":
                        sb.Append(string.Concat(value, " = ''"));
                        break;
                    case "Trim":
                        sb.Append(string.Concat("trim(", field, ")"));
                        break;
                    case "TrimStart":
                        sb.Append(string.Concat("ltrim(", field, ")"));
                        break;
                    case "TrimEnd":
                        sb.Append(string.Concat("rtrim(", field, ")"));
                        break;
                    case "Substring":
                        string a = (int.Parse(exp.Arguments[0].ToString()) + 1).ToString();
                        string b = string.Empty;
                        if (exp.Arguments.Count > 1)
                        {
                            b = exp.Arguments[1].ToString();
                        }
                        if (b.HasValue())
                        {
                            sb.Append($"substr({field},{a},{b})");
                        }
                        else
                        {
                            sb.Append($"substr({field},{a})");
                        }
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
                    sqlText = sqlText.Replace("=", "<>").Replace("like", "not like");
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
        public static string ExpressionTypeCast(ExpressionType type)
        {
            switch (type)
            {
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                    return " and ";
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
                    return " or ";
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
        public static string BinaryExpressionConvert(BinaryExpression expression)
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
        public static string MemberExpressionConvert(MemberExpression expression)
        {
            if (expression.Expression is ParameterExpression)
            {
                var t = ((System.Reflection.PropertyInfo)expression.Member).PropertyType;
                object[] attrs = expression.Member.GetCustomAttributes(typeof(ColumnAttribute), true);
                if (attrs.Length > 0)
                {
                    return ColumnFormat(t, ((ColumnAttribute)attrs[0]).Name);
                }

                return ColumnFormat(t, expression.Member.Name);
            }

            object result = Expression.Lambda(expression).Compile().DynamicInvoke();
            if (result == null)
            {
                result = string.Empty;
            }

            // 枚举类型需获取对应的value值，非toString()的字符串
            if (expression.Type.IsEnum)
            {
                result = (int)Enum.Parse(expression.Type, result.ToString(), true);
            }

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
            else if (result is DateTime || result is DateTimeOffset)
            {
                return ColumnFormat(result.GetType(), $"'{(Convert.ToDateTime(result)).ToString("yyyy-MM-dd HH:mm:ss")}'");
            }
            else if (result is long || result is int)
            {
                return result.ToString();
            }
            return string.Format("'{0}'", result);
        }

        private static string ColumnFormat(Type type, string input)
        {
            if (type == typeof(DateTime) || type == typeof(DateTimeOffset))
            {
                if (Provider is Providers.SQLiteDatabaseProvider)
                {
                    return $"strftime('%Y-%m-%d %H:%M:%S',{input})";
                }
            }

            if (Provider is Providers.MySqlDatabaseProvider)
            {
                return $"`{input}`";
            }

            return input;
        }

        #endregion

        public static string ReplaceSpecialWords(string input)
        {
            return new System.Text.RegularExpressions.Regex("@").Replace(input, "@@");
            //return input.Replace("@", "@@");
        }

    }
}
