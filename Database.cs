using System;
using System.Data;
using System.Data.SqlClient;

namespace TokoGrosirApp
{
    public class Database
    {
        private readonly string connectionString;

        public Database()
        {
            // Adjust your connection string here
            connectionString = "Server=.;Database=toko_grosir;Integrated Security=true;TrustServerCertificate=True";
        }

        public SqlConnection GetConnection()
        {
            return new SqlConnection(connectionString);
        }

        public SqlDataReader ExecuteStoredProcedure(string procedureName)
        {
            var connection = GetConnection();
            connection.Open();
            var command = new SqlCommand(procedureName, connection);
            command.CommandType = CommandType.StoredProcedure;
            return command.ExecuteReader();
        }

        public SqlDataReader ExecuteStoredProcedureWithParam(string procedureName, string paramName, string paramValue)
        {
            var connection = GetConnection();
            connection.Open();
            var command = new SqlCommand(procedureName, connection);
            command.CommandType = CommandType.StoredProcedure;
            // Check if paramName and paramValue are not empty before adding parameter
            if (!string.IsNullOrEmpty(paramName) && !string.IsNullOrEmpty(paramValue))
            {
                command.Parameters.AddWithValue(paramName, paramValue);
            }
            return command.ExecuteReader();
        }
    }
}
