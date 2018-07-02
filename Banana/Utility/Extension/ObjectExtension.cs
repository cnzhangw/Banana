using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace System
{


    /// <summary> 
    /// 作者：zhangw 
    /// 时间：2017/3/30 20:51:29 
    /// CLR版本：4.0.30319.42000 
    /// 唯一标识：fdca0254-68c0-4681-add9-7436d4bce513 
    /// </summary> 
    public static class ObjectExtension
    {
        /// <summary>
        /// object to json string
        /// </summary>
        /// <param name="that"></param>
        /// <returns></returns>
        public static string ToJson(this object that)
        {
            // 1.jss
            //return new JavaScriptSerializer()
            //{
            //    MaxJsonLength = int.MaxValue,
            //}.Serialize(that);

            // 2.newtonsoft
            var jsonSerializerSettings = new JsonSerializerSettings()
            {
                MaxDepth = 10,
                Formatting = Formatting.Indented,
                ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
                //DateFormatString = "yyyy-MM-dd HH:mm:ss",
                DateFormatHandling = DateFormatHandling.MicrosoftDateFormat
            };
            return JsonConvert.SerializeObject(that, jsonSerializerSettings);
        }

        /// <summary>
        /// 获取指定属性值
        /// </summary>
        /// <param name="that"></param>
        /// <param name="propertyName">属性名称</param>
        /// <returns></returns>
        public static object GetPropertyValue(this object that, string propertyName)
        {
            if (that == null) return null;
            Type type = that.GetType();
            IEnumerable<PropertyInfo> properties = from pi in type.GetProperties() where pi.Name.ToLower() == propertyName.ToLower() select pi;
            var pif = properties.FirstOrDefault();
            if (pif == null)
            {
                return null;
            }
            return pif.GetValue(that, null);
        }

        /// <summary>
        /// 获取指定属性值
        /// </summary>
        /// <param name="that"></param>
        /// <param name="propertyName">属性名称</param>
        /// <param name="action"></param>
        /// <returns></returns>
        public static T GetPropertyValue<T>(this object that, string propertyName, Func<object, T> action = null)
        {
            try
            {
                object obj = that.GetPropertyValue(propertyName);
                if (obj != null)
                {
                    if (action != null)
                        return action(obj);
                    else
                        return (T)obj;
                }
            }
            catch (Exception ex) { throw ex; }
            return default(T);
        }

        /// <summary>
        /// 获取指定属性值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="that"></param>
        /// <param name="propertyName"></param>
        /// <param name="whenHave"></param>
        /// <param name="valueDefault"></param>
        /// <returns></returns>
        public static object GetPropertyValue<T>(this object that, string propertyName, Action<T> whenHave, T valueDefault)
        {
            try
            {
                object property = that.GetPropertyValue(propertyName);
                if (property != null)
                {
                    if (whenHave != null)
                        whenHave((T)property);
                }
                else
                {
                    if (whenHave != null && valueDefault != null)
                        whenHave(valueDefault);
                }
            }
            catch (Exception ex) { throw ex; }
            return that;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="that"></param>
        /// <param name="propertyName"></param>
        /// <param name="whenHave"></param>
        /// <returns></returns>
        public static object GetPropertyValue<T>(this object that, string propertyName, Action<T> whenHave)
        {
            try
            {
                object property = that.GetPropertyValue(propertyName);
                //bool nullable = typeof(T).GetGenericTypeDefinition() == typeof(Nullable<>);
                if (property != null)
                {
                    if (whenHave != null)
                    {
                        Type type = typeof(T);
                        switch (type.Name)
                        {
                            case "Int32":
                                whenHave((dynamic)Convert.ToInt32(property));
                                break;
                            case "Int64":
                                whenHave((dynamic)Convert.ToInt64(property));
                                break;
                            case "DateTime":
                                if (property.ToString().HasValue())
                                {
                                    DateTime value;
                                    if (DateTime.TryParse(property.ToString(),out value))
                                    {
                                        whenHave((dynamic)value);
                                    }
                                }
                                break;
                            case "Boolean":
                                whenHave((dynamic)Convert.ToBoolean(property));
                                break;
                            default:
                                whenHave((T)property);
                                break;
                        }
                    }
                }
                //else
                //{
                //    if (nullable)
                //    {
                //        //new Nullable<T>((T)property);
                //        //if (whenHave != null)
                //        //    whenHave(new Nullable<T>().Value);
                //    }
                //}
            }
            catch (Exception ex) { throw ex; }
            return that;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="that"></param>
        /// <param name="propertyName"></param>
        /// <param name="whenHave"></param>
        /// <returns></returns>
        public static object GetPropertyValue<T>(this object that, string propertyName, Action<T> whenHave, Action whenNull)
        {
            try
            {
                object property = that.GetPropertyValue(propertyName);
                //bool nullable = typeof(T).GetGenericTypeDefinition() == typeof(Nullable<>);
                if (property != null)
                {
                    if (whenHave != null)
                        whenHave.Invoke((T)property);
                }
                else
                {
                    if (whenNull != null)
                        whenNull.Invoke();
                }
            }
            catch (Exception ex) { throw ex; }
            return that;
        }

        #region JObject扩展

        public static T GetValue<T>(this JObject that, string propertyName)
        {
            JToken jtoken = that[propertyName];
            return jtoken == null ? default(T) : jtoken.Value<T>();
        }
        public static string GetString(this JObject that, string propertyName, string defaults = null)
        {
            string result = that.GetValue<string>(propertyName);
            return result.IsNullOrWhiteSpace() ? defaults : result;
        }
        public static int GetInt(this JObject that, string propertyName)
        {
            return that.GetValue<int>(propertyName);
        }


        public static T GetValue<T>(this JToken that, string propertyName)
        {
            JToken jtoken = that[propertyName];
            return jtoken == null ? default(T) : jtoken.Value<T>();
        }
        public static string GetString(this JToken that, string propertyName, string defaults = null)
        {
            string result = that.GetValue<string>(propertyName);
            return result.IsNullOrWhiteSpace() ? defaults : result;
        }
        public static int GetInt(this JToken that, string propertyName)
        {
            return that.GetValue<int>(propertyName);
        }

        #endregion
    }
}
