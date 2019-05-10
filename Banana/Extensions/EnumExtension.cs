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
    /// 时间：2017/3/30 21:35:55 
    /// CLR版本：4.0.30319.42000 
    /// 唯一标识：0fcf1eeb-ee2c-4743-863b-e483bf036e96 
    /// </summary> 
    public static class EnumExtension
    {
        public static string GetDescription(this System.Enum enumName)
        {
            string _description = string.Empty;
            FieldInfo _fieldInfo = enumName.GetType().GetField(enumName.ToString());
            DescriptionAttribute[] _attributes = _fieldInfo.GetDescriptAttr();
            if (_attributes != null && _attributes.Length > 0)
                _description = _attributes[0].Description;
            else
                _description = enumName.ToString();
            return _description;
        }
        public static string GetDescription(this System.Enum enumName, int number)
        {
            return enumName.GetDescription().Split('_')[number];
        }
    }
}
