namespace InfraEstrutura;
using System.Data;

public interface ISqlConnectionFactory
{
    IDbConnection CreateConnection();
}