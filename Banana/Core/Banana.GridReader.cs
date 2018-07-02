using System;
using System.Collections.Generic;
using System.Linq;
using Banana.Core.Dapper;

namespace Banana.Core
{
    public static partial class Banana
    {
        public class GridReader
        {
            private SqlMapper.GridReader gridReader = null;

            internal GridReader(SqlMapper.GridReader gridReader)
            {
                this.gridReader = gridReader;
            }

            public T ReadSingle<T>()
            {
                return gridReader.ReadSingle<T>();
            }
            public T ReadSingleOrDefault<T>()
            {
                return gridReader.ReadSingleOrDefault<T>();
            }

            public T ReadFirst<T>()
            {
                return gridReader.ReadFirst<T>();
            }
            public T ReadFirstOrDefault<T>()
            {
                return gridReader.ReadFirstOrDefault<T>();
            }

            public dynamic ReadFirst()
            {
                return gridReader.ReadFirst();
            }
            public dynamic ReadFirst(Type type)
            {
                return gridReader.ReadFirst(type);
            }

            public dynamic ReadFirstOrDefault()
            {
                return gridReader.ReadFirstOrDefault();
            }
            public dynamic ReadFirstOrDefault(Type type)
            {
                return gridReader.ReadFirstOrDefault(type);
            }

            public dynamic ReadSingle()
            {
                return gridReader.ReadSingle();
            }
            public dynamic ReadSingleOrDefault(Type type)
            {
                return gridReader.ReadSingleOrDefault(type);
            }

            public IEnumerable<dynamic> Read(bool buffered = true)
            {
                return gridReader.Read(buffered);
            }
            public IEnumerable<T> Read<T>(bool buffered = true)
            {
                return gridReader.Read<T>(buffered);
            }


            ///// <summary>
            ///// Read multiple objects from a single record set on the grid.
            ///// </summary>
            ///// <typeparam name="TFirst">The first type in the record set.</typeparam>
            ///// <typeparam name="TSecond">The second type in the record set.</typeparam>
            ///// <typeparam name="TReturn">The type to return from the record set.</typeparam>
            ///// <param name="func">The mapping function from the read types to the return type.</param>
            ///// <param name="splitOn">The field(s) we should split and read the second object from (defaults to "id")</param>
            ///// <param name="buffered">Whether to buffer results in memory.</param>
            //public IEnumerable<TReturn> Read<TFirst, TSecond, TReturn>(Func<TFirst, TSecond, TReturn> func, string splitOn = "id", bool buffered = true)
            //{
            //    return gridReader.Read(func, splitOn, buffered);
            //}
            
            /// <summary>
            /// 一对多聚合查询
            /// </summary>
            /// <typeparam name="TFirst">父对象</typeparam>
            /// <typeparam name="TSecond">子对象</typeparam>
            /// <typeparam name="TKey">关联键类型，如：string</typeparam>
            /// <param name="firstKey">主对象键值</param>
            /// <param name="secondKey">子对象关联外键值</param>
            /// <param name="addChildren">子对象赋值函数</param>
            /// <returns></returns>
            public List<TFirst> Map<TFirst, TSecond, TKey>(Func<TFirst, TKey> firstKey,
                Func<TSecond, TKey> secondKey, Action<TFirst, List<TSecond>> addChildren)
            {
                var first = gridReader.Read<TFirst>().AsList();
                var childMap = gridReader.Read<TSecond>().GroupBy(s => secondKey(s)).ToDictionary(g => g.Key, g => g.AsEnumerable());
                foreach (var item in first)
                {
                    IEnumerable<TSecond> children;
                    if (childMap.TryGetValue(firstKey(item), out children))
                    {
                        addChildren(item, children.AsList());
                    }
                }
                return first;
            }

            /// <summary>
            /// 一对多聚合查询
            /// </summary>
            /// <typeparam name="TFirst">父对象</typeparam>
            /// <typeparam name="TSecond">子对象</typeparam>
            /// <param name="firstKey">主对象键值</param>
            /// <param name="secondKey">子对象关联外键值</param>
            /// <param name="addChildren">子对象赋值函数</param>
            /// <returns></returns>
            public List<TFirst> Map<TFirst, TSecond>(Func<TFirst, string> firstKey,
                Func<TSecond, string> secondKey, Action<TFirst, List<TSecond>> addChildren)
            {
                return this.Map<TFirst, TSecond, string>(firstKey, secondKey, addChildren);
            }

            /// <summary>
            /// 一对多聚合查询
            /// </summary>
            /// <typeparam name="TFirst">父对象</typeparam>
            /// <typeparam name="TSecond">子对象</typeparam>
            /// <param name="secondKey">子对象关联外键值</param>
            /// <param name="addChildren">子对象赋值函数</param>
            /// <returns></returns>
            public List<TFirst> Map<TFirst, TSecond>(Func<TSecond, string> secondKey, Action<TFirst, List<TSecond>> addChildren)
            {
                return this.Map<TFirst, TSecond, string>((first) =>
                {
                    string id = first.GetPropertyValue<string>("Id");
                    if (!id.HasValue())
                    {
                        throw new KeyNotFoundException($"{typeof(TFirst).FullName} 对象中未找到 Id 属性");
                    }
                    return id;
                }, secondKey, addChildren);
            }

        }
    }
}
