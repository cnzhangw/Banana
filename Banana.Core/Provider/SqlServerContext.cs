using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Banana.Core.Provider
{
    class SqlServerContext<T, TId> : DataContext<T, TId>
        where T : DataContextModel, new()
        //where TId : Object
    {
    }
    class SqlServerContext<T> : SqlServerContext<T,int> where T : DataContextModel, new()
    {
    }
}
