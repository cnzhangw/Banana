using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Banana.Core.Provider
{
    class OracleContext<T, TId> : DataContext<T, TId>
        where T : DataContextModel, new()
        //where TId : object
    {
    }

    class OracleContext<T> : OracleContext<T, int> where T : DataContextModel, new()
    {
    }
}
