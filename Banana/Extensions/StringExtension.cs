using Banana;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace System
{
    /// <summary> 
    /// 作者：zhangw 
    /// 时间：2017/3/30 21:16:58 
    /// CLR版本：4.0.30319.42000 
    /// 唯一标识：76ef04aa-b4bf-4b0d-925a-3a7059606f13 
    /// </summary> 
    public static class StringExtension
    {
        /// <summary>
        /// 反序列化对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="that"></param>
        /// <returns></returns>
        public static T Deserialize<T>(this string that)// where T : class
        {
            if (that.HasValue())
            {
                try
                {
                    JsonSerializer serializer = new JsonSerializer();
                    StringReader sr = new StringReader(that);
                    object o = serializer.Deserialize(new JsonTextReader(sr), typeof(T));
                    T t = (T)o;
                    return t;
                    //return JsonConvert.DeserializeObject<T>(that);
                }
                catch (Exception) { }
            }
            return default(T);
        }

        /// <summary>
        /// 反序列化匿名类型
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="that"></param>
        /// <param name="anonymousTypeObject"></param>
        /// <returns></returns>
        public static T DeserializeAnonymousType<T>(this string that, T anonymousTypeObject)
        {
            if (that.HasValue())
            {
                try
                {
                    return JsonConvert.DeserializeAnonymousType(that, anonymousTypeObject);
                }
                catch (Exception) { }
            }
            return default(T);
        }

        /// <summary>
        /// 解析JSON数组生成对象实体集合
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="that">json数组字符串(eg.[{"ID":"1","Name":"zhangw"}])</param>
        /// <returns>对象实体集合</returns>
        public static List<T> DeserializeList<T>(this string that) //where T : class
        {
            if (that.HasValue())
            {
                try
                {
                    JsonSerializer serializer = new JsonSerializer();
                    StringReader sr = new StringReader(that);
                    object o = serializer.Deserialize(new JsonTextReader(sr), typeof(List<T>));
                    List<T> list = o as List<T>;
                    return list;
                }
                catch (Exception) { }
            }
            return default(List<T>);
        }

        /// <summary>
        /// 格式化字符串
        /// </summary>
        /// <param name="that"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static string Format(this string that, params object[] args)
        {
            return string.Format(that, args);
        }

        /// <summary>
        /// 判断字符串是否为空
        /// </summary>
        /// <param name="that"></param>
        /// <returns></returns>
        public static bool IsNullOrWhiteSpace(this string that)
        {
            return string.IsNullOrWhiteSpace(that);
        }

        /// <summary>
        /// 是否有值，等同于IsNullOrWhiteSpace函数
        /// </summary>
        /// <param name="that"></param>
        /// <returns></returns>
        public static bool HasValue(this string that)
        {
            return !that.IsNullOrWhiteSpace();
        }

        /// <summary>
        /// 转换成Camel命名法
        /// </summary>
        /// <param name="that"></param>
        /// <returns></returns>
        public static string ToCamel(this string that)
        {
            if (!that.HasValue()) return that;
            return that[0].ToString().ToLower() + that.Substring(1);
        }

        /// <summary>
        /// 转换成Pascal命名法
        /// </summary>
        /// <param name="that"></param>
        /// <returns></returns>
        public static string ToPascal(this string that)
        {
            if (!that.HasValue()) return that;

            var arr = that.Split("_".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            if (arr.Length == 1)
            {
                return that[0].ToString().ToUpper() + that.Substring(1).ToLower();
            }

            StringBuilder sb = new StringBuilder();
            foreach (var item in arr)
            {
                sb.Append(item.ToPascal());
            }
            return sb.ToString();
        }

        /// <summary>
        /// 转全角(SBC case)
        /// </summary>
        /// <param name="that"></param>
        /// <returns></returns>
        public static string ToSBC(this string that)
        {
            ///任意字符串
            ///全角字符串
            ///全角空格为12288，半角空格为32
            ///其他字符半角(33-126)与全角(65281-65374)的对应关系是：均相差65248

            char[] c = that.ToCharArray();
            for (int i = 0; i < c.Length; i++)
            {
                if (c[i] == 32)
                {
                    c[i] = (char)12288;
                    continue;
                }
                if (c[i] < 127)
                    c[i] = (char)(c[i] + 65248);
            }
            return new String(c);
        }

        /// <summary>
        /// 转半角(DBC case)
        /// </summary>
        /// <param name="that"></param>
        /// <returns></returns>
        public static String ToDBC(this string that)
        {
            // /任意字符串
            // /半角字符串
            // /全角空格为12288，半角空格为32
            // /其他字符半角(33-126)与全角(65281-65374)的对应关系是：均相差65248

            char[] c = that.ToCharArray();
            for (int i = 0; i < c.Length; i++)
            {
                if (c[i] == 12288)
                {
                    c[i] = (char)32;
                    continue;
                }
                if (c[i] > 65280 && c[i] < 65375)
                    c[i] = (char)(c[i] - 65248);
            }
            return new String(c);
        }

        /// <summary>
        /// 去除字符串中的html字符
        /// </summary>
        /// <param name="that"></param>
        /// <returns></returns>
        public static string StripHTML(this string that)
        {
            if (!that.HasValue()) { return ""; }
            //return Text.RegularExpressions.Regex.Replace(that, @"<[^>]*>", "").Trim();

            that = Regex.Replace(that, "</p(?:\\s*)>(?:\\s*)<p(?:\\s*)>", "\n\n", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            that = Regex.Replace(that, "", "\n", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            that = Regex.Replace(that, "\"", "''", RegexOptions.IgnoreCase | RegexOptions.Compiled);

            that = Regex.Replace(that, "<[^>]+>", "", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            return that;
        }

        /// <summary>
        /// 转JObject（Newtonsoft.Json.Linq）
        /// </summary>
        /// <param name="that"></param>
        /// <returns></returns>
        public static Result<JObject> ToJObject(this string that)
        {
            Result<JObject> result = Result.Create<JObject>();
            try
            {
                JObject obj = JObject.Parse(that);
                result.Success = true;
                result.Data = obj;
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
            }
            return result;
        }

    }
}
