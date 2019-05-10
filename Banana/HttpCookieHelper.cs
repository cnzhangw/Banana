using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;

namespace Banana
{
    /// <summary>
    /// Cookie操作帮助类
    /// </summary>
    public class HttpCookieHelper
    {
        public class CookieItem
        {
            /// <summary>
            /// 键
            /// </summary>
            public string Key { get; set; }
            /// <summary>
            /// 值
            /// </summary>
            public string Value { get; set; }
        }

        /// <summary>
        /// 根据字符生成Cookie列表
        /// </summary>
        /// <param name="cookie">Cookie字符串</param>
        /// <returns></returns>
        public static List<CookieItem> GetCookieList(string cookie)
        {
            List<CookieItem> cookielist = new List<CookieItem>();
            foreach (string item in cookie.Split(new string[] { ";", "," }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (Regex.IsMatch(item, @"([\s\S]*?)=([\s\S]*?)$"))
                {
                    Match m = Regex.Match(item, @"([\s\S]*?)=([\s\S]*?)$");
                    cookielist.Add(new CookieItem() { Key = m.Groups[1].Value, Value = m.Groups[2].Value });
                }
            }
            return cookielist;
        }

        /// <summary>
        /// 根据Key值得到Cookie值,Key不区分大小写
        /// </summary>
        /// <param name="Key">key</param>
        /// <param name="cookie">字符串Cookie</param>
        /// <returns></returns>
        public static string GetCookieValue(string Key, string cookie)
        {
            foreach (CookieItem item in GetCookieList(cookie))
            {
                if (item.Key == Key)
                    return item.Value;
            }
            return "";
        }

        /// <summary>
        /// 格式化Cookie为标准格式
        /// </summary>
        /// <param name="key">Key值</param>
        /// <param name="value">Value值</param>
        /// <returns></returns>
        public static string CookieFormat(string key, string value)
        {
            return string.Format("{0}={1};", key, value);
        }

        
        public static CookieCollection GetAllCookies(CookieContainer cookie)
        {
            var cookieCollection = new CookieCollection();

            var table = (Hashtable)cookie.GetType().InvokeMember("m_domainTable",
                                                                BindingFlags.NonPublic |
                                                                BindingFlags.GetField |
                                                                BindingFlags.Instance,
                                                                null,
                                                                cookie,
                                                                new object[] { });

            foreach (var tableKey in table.Keys)
            {
                var strTableKey = (string)tableKey;

                if (strTableKey[0] == '.')
                {
                    strTableKey = strTableKey.Substring(1);
                }

                var list = (SortedList)table[tableKey].GetType().InvokeMember("m_list",
                                                                            BindingFlags.NonPublic |
                                                                            BindingFlags.GetField |
                                                                            BindingFlags.Instance,
                                                                            null,
                                                                            table[tableKey],
                                                                            new object[] { });
                foreach (var listKey in list.Keys)
                {
                    var url = "https://" + strTableKey + (string)listKey;
                    cookieCollection.Add(cookie.GetCookies(new Uri(url)));
                }
            }
            return cookieCollection;
        }

        public static string GetCookie(CookieContainer cookie, string key)
        {
            CookieCollection cc = GetAllCookies(cookie);

            Cookie c = cc[key];//如果不存在key会返回null
            return c == null ? null : c.Value;
        }

        
        [DllImport("wininet.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern bool InternetGetCookieEx(string pchURL, string pchCookieName, StringBuilder pchCookieData, ref int pcchCookieData, int dwFlags, object lpReserved);

        /// <summary>
        /// 取当前webBrowser登录后的Cookie值
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static string GetCookieString(string url)
        {
            // Determine the size of the cookie      
            int size = 256;
            StringBuilder builder = new StringBuilder(size);
            if (!InternetGetCookieEx(url, null, builder, ref size, 0x00002000, null))
            {
                if (size < 0)
                    return null;
                // Allocate stringbuilder large enough to hold the cookie    
                builder = new StringBuilder(size);
                if (!InternetGetCookieEx(url, null, builder, ref size, 0x00002000, null))
                    return null;
            }
            return builder.ToString();
        }

    }
}