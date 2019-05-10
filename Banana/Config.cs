using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Banana
{
    /// <summary>
    /// 配置读取（配置文件路径：/config/banana.json）
    /// </summary>
    public class Config
    {

        internal class CoreConfig
        {
            private readonly bool noCache = true;
            private JObject BuildItems(string filename)
            {
                FileInfo fileInfo = null;
                if (File.Exists(filename))
                {
                    fileInfo = new FileInfo(filename);
                }
                else
                {
                    string fullname = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, filename);
                    if (File.Exists(fullname))
                    {
                        fileInfo = new FileInfo(fullname);
                    }
                    else
                    {
                        fullname = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config", filename);
                        if (File.Exists(fullname))
                        {
                            fileInfo = new FileInfo(fullname);
                        }
                        else
                        {
                            throw new FileNotFoundException("配置文件找不到", filename);
                        }
                    }
                }

                return JObject.Parse(File.ReadAllText(fileInfo.FullName, Encoding.UTF8));
            }

            public CoreConfig()
            {
                BuildItems("banana.json");
            }
            public CoreConfig(string filename)
            {
                BuildItems(filename);
            }

            private JObject Items
            {
                get
                {
                    if (noCache || _Items == null)
                    {
                        _Items = BuildItems("banana.json");
                    }
                    return _Items;
                }
            }
            private JObject _Items;

        }





        //public static Config GG(string filename)
        //{
        //    var cc = new CoreConfig(filename);



        //    return this;
        //}




        private static bool noCache = true;
        private static JObject BuildItems()
        {
            var fileInfo = new FileInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config", "banana.json"));
            if (!fileInfo.Exists)
            {
                throw new FileNotFoundException("未找到配置文件：/config/banana.json");
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

        private static T LoopLookup<T>(List<string> fragments, JToken jo, T defaultValue)
        {
            if (fragments.Count > 0)
            {
                var o = jo[fragments[0]];
                if (o != null)
                {
                    fragments.RemoveAt(0);
                    return LoopLookup<T>(fragments, o, defaultValue);
                }
            }
            else
            {
                if (jo != null)
                {
                    return Convert<T>(jo);
                }
            }
            return defaultValue;// default(T);
        }
        private static T LoopLookup<T>(List<string> fragments, JToken jo, Func<T> whenNull)
        {
            if (fragments.Count > 0)
            {
                var o = jo[fragments[0]];
                if (o != null)
                {
                    fragments.RemoveAt(0);
                    return LoopLookup<T>(fragments, o, whenNull);
                }
            }
            else
            {
                if (jo != null)
                {
                    return Convert<T>(jo);
                }
            }
            return whenNull == null ? default(T) : whenNull.Invoke();
        }

        private static T Convert<T>(JToken jToken)
        {
            Type type = typeof(T);
            if (!type.IsPrimitive)//1.非基础类型
            {
                //2.非string
                if (!(type == typeof(string) || type == typeof(JObject) || type == typeof(JToken) || type == typeof(JArray) || type == typeof(JProperty) || type == typeof(JValue)))
                {
                    return JsonConvert.DeserializeObject<T>(jToken.ToString());
                }
                // typeof(T).GetInterfaces().Any(x => typeof(string) == (x.IsGenericType ? x.GetGenericTypeDefinition() : x));
            }
            return jToken.Value<T>();
        }

        public static T GetValue<T>(string key, Func<T> whenNull)
        {
            var jt = Items[key];
            if (jt != null)
            {
                return Convert<T>(jt);
            }

            if (key.Contains("."))
            {
                List<string> fragments = key.Split(".".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).ToList();
                var jo = Items[fragments[0]];
                if (jo != null)
                {
                    fragments.RemoveAt(0);
                    return LoopLookup<T>(fragments, jo, whenNull);
                }
            }

            return whenNull == null ? default(T) : whenNull.Invoke();
        }

        public static T GetValue<T>(string key, T valueDefault)
        {
            T value = default(T);
            //try
            //{
            JToken jt1 = Items[key];
            if (jt1 != null)
            {
                value = Convert<T>(jt1);
            }
            else
            {
                if (key.Contains("."))
                {
                    List<string> fragments = key.Split(".".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).ToList();
                    JToken jt2 = Items[fragments[0]];
                    if (jt2 != null)
                    {
                        fragments.RemoveAt(0);
                        return LoopLookup<T>(fragments, jt2, valueDefault);
                    }
                }
            }
            //}
            //catch
            //{
            //    value = valueDefault;
            //}
            return value;
        }

        public static T GetValue<T>(string key)
        {
            return GetValue<T>(key: key, whenNull: null);
        }

        public static T GetValue<T>(string key, JObject obj)
        {
            if (obj == null)
            {
                return default(T);
            }

            return Convert<T>(obj[key]);
            //return obj[key].Value<T>();
        }

        public static string[] GetStringList(string key)
        {
            return GetList<string>(key);
        }
        public static T[] GetList<T>(string key)
        {
            var token = GetValue<JToken>(key);
            if (token == null)
            {
                return null;
            }
            return token.Select(x => Convert<T>(x)).ToArray();
        }

        public static string GetString(string key)
        {
            return GetValue<string>(key);
        }
        public static string GetString(string key, string defaults)
        {
            return GetValue<string>(key, defaults);
        }

        public static int GetInt(string key)
        {
            return GetValue<int>(key);
        }
        public static int GetInt(string key, int defaults)
        {
            return GetValue<int>(key, defaults);
        }

        /// <summary>
        /// 是否是debug模式
        /// </summary>
        public static bool Debug
        {
            get
            {
                return GetValue("debug", false);
            }
        }

    }
}