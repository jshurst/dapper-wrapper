using System.Data;
using System.Data.SqlClient;
using DapperWrapper.Helpers;

namespace DapperWrapper.Tests.Support
{
    public class SqlConnectionFactory : IConnectionFactory
    {
        public IDbConnection GetConnection()
        {
            return new SqlConnection("Data Source=localhost;Initial Catalog=DapperWrapper;Integrated Security=SSPI;");
        }
    }
}
