using System.Data.Common;

namespace Banana.PetaPoco.Providers
{
    public class SqlServerMsDataDatabaseProvider : SqlServerDatabaseProvider
    {
        public override DbProviderFactory GetFactory()
            => GetFactory("Microsoft.Data.SqlClient.SqlClientFactory, Microsoft.Data.SqlClient");
    }
}