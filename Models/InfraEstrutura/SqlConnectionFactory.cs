using Microsoft.Data.SqlClient;

public class SqlConnectionFactory: ISqlConnectionFactory {
    private readonly string _connectionString;

    public SqlConnectionFactory(string connectionString)
    {
        _connectionString = connectionString ?? throw new ArgumentException(nameof(connectionString));

    }

    public SqlConnection CreateConnection()
    {
        return new SqlConnection(_connectionString);
    }
}