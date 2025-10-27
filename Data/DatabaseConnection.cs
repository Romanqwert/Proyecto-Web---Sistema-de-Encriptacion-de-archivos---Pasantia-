using MySql.Data.MySqlClient;
using DotNetEnv;

namespace EncriptacionApi.Data
{
    public sealed class DatabaseConnection
    {
        private static readonly Lazy<DatabaseConnection> _instance = new(() => new DatabaseConnection());
        private readonly string _connectionString;
        private MySqlConnection? _connection;

        private DatabaseConnection()
        {
            Env.Load();

            string server = Environment.GetEnvironmentVariable("DB_SERVER") ?? "localhost";
            string database = Environment.GetEnvironmentVariable("DB_NAME") ?? "test";
            string user = Environment.GetEnvironmentVariable("DB_USER") ?? "root";
            string password = Environment.GetEnvironmentVariable("DB_PASSWORD") ?? "";

            _connectionString = $"Server={server};Database={database};User Id={user};Password={password};";
        }

        public MySqlConnection GetConnection()
        {
            if (_connection == null) _connection = new MySqlConnection(_connectionString);

            if (_connection.State != System.Data.ConnectionState.Open) _connection.Open();

            return _connection;
        }

        public void CloseConnection() { 
            if (_connection != null && _connection.State != System.Data.ConnectionState.Open)
            {
                _connection.Close();
            }
        }
    }
}
