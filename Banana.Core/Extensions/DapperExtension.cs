using System;
using System.Collections.Generic;
using System.Linq;

namespace Banana.Core
{
    /// <summary>
    /// 说明：DapperExtension
    /// 作者：张炜 
    /// 时间：2018/5/19 20:21:05
    /// Email:cnzhangw@sina.com
    /// CLR版本：4.0.30319.42000
    /// 唯一标识：360044bf-29a8-4d45-872d-73e32efd964a
    /// </summary>
    public static class DapperExtension
    {

        internal static IEnumerable<TFirst> Map<TFirst, TSecond, TKey>(this Dapper.SqlMapper.GridReader reader,
        Func<TFirst, TKey> firstKey, Func<TSecond, TKey> secondKey, Action<TFirst, IEnumerable<TSecond>> addChildren)
        {
            var first = reader.Read<TFirst>().ToList();
            var childMap = reader.Read<TSecond>().GroupBy(s => secondKey(s)).ToDictionary(g => g.Key, g => g.AsEnumerable());
            foreach (var item in first)
            {
                IEnumerable<TSecond> children;
                if (childMap.TryGetValue(firstKey(item), out children))
                {
                    addChildren(item, children);
                }
            }
            return first;
        }

    }
}
