﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Dynamic;

namespace Banana.Core
{
    public static partial class Banana
    {
        /*
        internal delegate object MyDelegate(dynamic Sender, params object[] PMs);

        internal class DelegateObj
        {
            private MyDelegate _delegate;

            public MyDelegate CallMethod
            {
                get { return _delegate; }
            }
            private DelegateObj(MyDelegate D)
            {
                _delegate = D;
            }
            /// <summary>  
            /// 构造委托对象，让它看起来有点javascript定义的味道.  
            /// </summary>  
            /// <param name="D"></param>  
            /// <returns></returns>  
            public static DelegateObj Function(MyDelegate D)
            {
                return new DelegateObj(D);
            }
        }
        
        internal class DObject : DynamicObject
        {
            //保存对象动态定义的属性值
            private Dictionary<string, object> _values;

            public DObject()
            {
                _values = new Dictionary<string, object>();
            }

            /// <summary>
            /// 获取属性值
            /// </summary>
            /// <param name="propertyName"></param>
            /// <returns></returns>
            public object GetPropertyValue(string propertyName)
            {
                if (_values.ContainsKey(propertyName) == true)
                {
                    return _values[propertyName];
                }
                return null;
            }

            /// <summary>
            /// 设置属性值
            /// </summary>
            /// <param name="propertyName"></param>
            /// <param name="value"></param>
            public void SetPropertyValue(string propertyName, object value)
            {
                if (_values.ContainsKey(propertyName) == true)
                {
                    _values[propertyName] = value;
                }
                else
                {
                    _values.Add(propertyName, value);
                }
            }

            /// <summary>
            /// 实现动态对象属性成员访问的方法，得到返回指定属性的值
            /// </summary>
            /// <param name="binder"></param>
            /// <param name="result"></param>
            /// <returns></returns>
            public override bool TryGetMember(GetMemberBinder binder, out object result)
            {
                result = GetPropertyValue(binder.Name);
                return result == null ? false : true;
            }

            /// <summary>
            /// 实现动态对象属性值设置的方法。
            /// </summary>
            /// <param name="binder"></param>
            /// <param name="value"></param>
            /// <returns></returns>
            public override bool TrySetMember(SetMemberBinder binder, object value)
            {
                SetPropertyValue(binder.Name, value);
                return true;
            }

            /// <summary>
            /// 动态对象动态方法调用时执行的实际代码
            /// </summary>
            /// <param name="binder"></param>
            /// <param name="args"></param>
            /// <param name="result"></param>
            /// <returns></returns>
            public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
            {
                var theDelegateObj = GetPropertyValue(binder.Name) as DelegateObj;
                if (theDelegateObj == null || theDelegateObj.CallMethod == null)
                {
                    result = null;
                    return false;
                }
                result = theDelegateObj.CallMethod(this, args);
                return true;
            }
            public override bool TryInvoke(InvokeBinder binder, object[] args, out object result)
            {
                return base.TryInvoke(binder, args, out result);
            }

        }
        */
    }
}
