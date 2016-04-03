using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Banana.Core
{
    using Banana.Core.Poco;
    using System.Configuration;
    using System.Linq.Expressions;
    public class DataAccess : Database
    {
        private static ConnectionStringSettings mainConnection = ConfigurationManager.ConnectionStrings["mainConnection"];

        public DataAccess()
            : base(mainConnection.ConnectionString, mainConnection.ProviderName)
        {

        }

    }
}
