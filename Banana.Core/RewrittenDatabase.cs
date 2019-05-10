using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;
using System.Linq;

namespace Banana.Core
{
    /// <summary>
    /// implement of petapoco's database
    /// zhangw 2019-4-1 14:08:04
    /// </summary>
    internal class RewrittenDatabase
        : Database
    {
        public event Action<string, object[]> OnExecutingEvent;

        public RewrittenDatabase(string connectionString, string providerName)
            : base(connectionString, providerName) { }

        public RewrittenDatabase(string connectionString, DbProviderFactory factory)
            : base(connectionString, factory) { }


        public override void OnExecutingCommand(IDbCommand cmd)
        {
            if (OnExecutingEvent.GetInvocationList().Length > 0)
            {
                try
                {
                    object[] args = (from IDataParameter parameter in cmd.Parameters select parameter.Value).ToArray();
                    OnExecutingEvent.Invoke(cmd.CommandText, args);
                }
                catch (Exception)
                {

                }
            }
        }
    }
}
