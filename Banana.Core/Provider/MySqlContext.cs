using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Banana.Core.Provider
{
    sealed class MySqlContext<T, TId> : DataContext<T, TId>
        where T : DataContextModel, new()
        //where TId : Object
    {
        public override int ExecuteProcedure(string procedureName, Action<List<object>> action)
        {
            return base.ExecuteProcedure(procedureName, action);
        }
    }
}
