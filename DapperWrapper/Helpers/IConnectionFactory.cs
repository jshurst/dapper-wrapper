using System.Data;

namespace DapperWrapper.Helpers
{
    public interface IConnectionFactory
    {
        IDbConnection GetConnection();
    }
}
