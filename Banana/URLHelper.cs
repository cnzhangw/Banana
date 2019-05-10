using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace Banana
{
    public sealed class URLHelper
    {
        public static NameValueCollection QueryString(string url)
        {
            if (url.IsNullOrWhiteSpace())
            {
                return null;
            }

            url = WebUtility.UrlDecode(url);
            var m = Regex.Matches(url, @"(?<=\?|&)[\w\={}\\\\,-:'\s'""]*(?=[^#\s]|)", RegexOptions.None);
            if (m.Count <= 0)
            {
                return null;
            }

            NameValueCollection nvcs = new NameValueCollection();
            string[] itemvalues = null;
            for (int i = 0; i < m.Count; i++)
            {
                itemvalues = m[i].Value.Split(new char[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
                if (itemvalues == null || itemvalues.Length == 0) continue;
                nvcs.Add(itemvalues[0], itemvalues.Length <= 1 ? string.Empty : itemvalues[1]);
            }
            return nvcs;
        }

        public static string QueryString(string name, NameValueCollection values)
        {
            return values.Get(name);
        }

        public static string QueryString(string name, string url)
        {
            return QueryString(name, QueryString(url));
        }

    }
}
