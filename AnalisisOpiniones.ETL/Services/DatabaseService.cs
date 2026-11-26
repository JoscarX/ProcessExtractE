using System.Globalization;
using AnalisisOpiniones.ETL.Models;
using Microsoft.Data.SqlClient;

namespace AnalisisOpiniones.ETL.Services
{
    public sealed class DatabaseService
    {
        private const string Query = """
            SELECT Id, ProductId, Comment, Rating, CreatedAt
            FROM dbo.Opiniones
            ORDER BY Id;
            """;

        private readonly string _connectionString;

        public DatabaseService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<List<Opinion>> ExtractAsync()
        {
            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            await using var command = new SqlCommand(Query, connection);
            await using var reader = await command.ExecuteReaderAsync();

            var opinions = new List<Opinion>();

            while (await reader.ReadAsync())
            {
                opinions.Add(new Opinion(
                    reader.GetInt32(0).ToString(CultureInfo.InvariantCulture),
                    reader.GetInt32(1),
                    reader.GetString(2),
                    reader.IsDBNull(3) ? null : reader.GetInt32(3),
                    reader.GetDateTimeOffset(4)));
            }

            return opinions;
        }
    }
}
