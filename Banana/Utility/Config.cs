using System;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;

namespace Banana.Utility
{
    public class Config
    {
        private static bool noCache = true;
        private static JObject BuildItems()
        {
            var fileInfo = new FileInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config", "banana.json"));
            if (!fileInfo.Exists)
            {
                throw new FileNotFoundException("未能找到配置文件：/config/banana.json");
            }
            var json = File.ReadAllText(fileInfo.FullName, Encoding.UTF8);
            return JObject.Parse(json);
        }

        public Config()
        {
            BuildItems();
        }

        private static JObject Items
        {
            get
            {
                if (noCache || _Items == null)
                {
                    _Items = BuildItems();
                }
                return _Items;
            }
        }
        private static JObject _Items;

        public static T GetValue<T>(string key)
        {
            //if (key.Contains("."))
            //{
            //    string[] arr = key.Split(".".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

            //    var action = new Func<string, JObject>((k) =>
            //     {
            //         JObject obj = GetValue<JObject>(item);
            //     });

            //    int length = arr.Length;
            //    foreach (var item in arr)
            //    {
            //        length--;
            //        JObject obj = GetValue<JObject>(item);
            //        if (obj != null)
            //        {

            //        }
            //    }

            //}
            return Items[key].Value<T>();
        }
        public static T GetValue<T>(string key, JObject obj)
        {
            if (obj == null) return default(T);
            return obj[key].Value<T>();
        }

        public static String[] GetStringList(string key)
        {
            return Items[key].Select(x => x.Value<String>()).ToArray();
        }

        public static String GetString(string key)
        {
            return GetValue<String>(key);
        }

        public static int GetInt(string key)
        {
            return GetValue<int>(key);
        }


    }
}