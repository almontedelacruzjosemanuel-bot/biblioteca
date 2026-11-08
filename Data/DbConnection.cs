using Microsoft.Data.SqlClient;

namespace Biblioteca.Api.Data
{
    public class DbConnection
    {
        private readonly string _connectionString;

        public DbConnection(IConfiguration configuration)
        {
            _connectionString = configuration
                .GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException(
                    "No se encontró DefaultConnection en appsettings.json");
        }

        public SqlConnection CreateConnection()
        {
            return new SqlConnection(_connectionString);
        }
    }
}