using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Banana.Utility
{
    /// <summary>
    /// C# 用于Byte转KB/MB/GB
    /// </summary>
    public static class ByteConverter
    {
        const int GB = 1024 * 1024 * 1024;//定义GB的计算常量
        const int MB = 1024 * 1024;//定义MB的计算常量
        const int KB = 1024;//定义KB的计算常量

        /// <summary>
        /// 用于Byte转KB/MB/GB
        /// </summary>
        /// <param name="KSize">Byte</param>
        /// <returns></returns>
        public static string ByteConversion(Int64 KSize)
        {
            if (KSize / GB >= 1)//如果当前Byte的值大于等于1GB
                return (Math.Round(KSize / (float)GB, 2)).ToString() + "GB";

            else if (KSize / MB >= 1)//如果当前Byte的值大于等于1MB
                return (Math.Round(KSize / (float)MB, 2)).ToString() + "MB";

            else if (KSize / KB >= 1)//如果当前Byte的值大于等于1KB
                return (Math.Round(KSize / (float)KB, 2)).ToString() + "KB";

            else
                return KSize.ToString() + "B";//显示Byte值
        }
    }
}
