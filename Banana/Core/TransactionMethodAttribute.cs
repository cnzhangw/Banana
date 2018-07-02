using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Banana.Core
{
    /// <summary> 
    /// 作者：zhangw 
    /// 时间：2017/3/26 20:33:02 
    /// CLR版本：4.0.30319.42000 
    /// 唯一标识：5c13e441-37a3-4f1c-beaa-8ba170f1e028 
    /// </summary> 
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    internal sealed class TransactionMethodAttribute : Attribute
    {
        public object error = null;
        public TransactionMethodAttribute() { }
        public TransactionMethodAttribute(object message)
        {
            this.error = message;
        }
    }
}
