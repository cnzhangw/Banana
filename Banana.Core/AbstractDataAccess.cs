using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Banana.Core
{
    using Banana.Core.Poco;
    using System.Configuration;
    using System.Linq.Expressions;

    class AbstractDataAccess : Database
    {
        private static ConnectionStringSettings mainConnection = ConfigurationManager.ConnectionStrings["mainConnection"];

        public AbstractDataAccess()
            : base("Data Source=121.52.220.139;port=8103;User ID=net;Password=W0X4RhB04Wz0VDUit98l9a;database=net;Persist Security Info=false;Min Pool Size=2;Max Pool Size=2;Connect Timeout=30;", "MySql.Data.MySqlClient")
        //: base(mainConnection.ConnectionString, mainConnection.ProviderName)
        {

        }
    }
}
