using System;
using System.Collections.Generic;
using System.Text;

namespace Banana.Core
{
    /// <summary>
    /// 拦截器
    /// </summary>
    public interface IInterceptor
    {
        void OnExecutingCommand(string sql,object[] args);
    }
}
