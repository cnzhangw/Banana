using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace System
{
    /// <summary> 
    /// 作者：zhangw 
    /// 时间：2017/3/30 21:38:06 
    /// CLR版本：4.0.30319.42000 
    /// 唯一标识：33d63979-1f32-49d4-98bd-43ba02f2ed0c 
    /// </summary> 
    public static class FieldInfoExtension
    {
        public static DescriptionAttribute[] GetDescriptAttr(this FieldInfo fieldInfo)
        {
            if (fieldInfo != null)
            {
                return (DescriptionAttribute[])fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), false);
            }
            return null;
        }
    }
}
