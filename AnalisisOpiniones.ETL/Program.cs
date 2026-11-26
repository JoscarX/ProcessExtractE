using System.Text.Json;
using AnalisisOpiniones.ETL.Models;
using AnalisisOpiniones.ETL.Services;
using CsvHelper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace AnalisisOpiniones.ETL
{
    internal static class Program
    {
        private static async Task<int> Main()
        {
            try
            {
                return await ExecuteAsync();
            }
            catch (Exception error) when (error is IOException or UnauthorizedAccessException
                or ArgumentException or InvalidDataException or FormatException)
            {
                Console.WriteLine($"Error de configuración: {error.Message}");
                return 1;
            }
        }

        private static async Task<int> ExecuteAsync()
        {
            var folder = AppContext.BaseDirectory;
            var configuration = new ConfigurationBuilder()
                .SetBasePath(folder)
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables()
                .Build();

            var settings = configuration.GetSection("Extraction");
            var stagingPath = Path.GetFullPath(settings["StagingPath"] ?? "Staging", folder);

            using var httpClient = new HttpClient();
            var csv = new CsvService();
            var database = new DatabaseService(configuration.GetConnectionString("Opiniones") ?? "");
            var api = new ApiService(httpClient);
            var json = new JsonService(stagingPath);

            Console.WriteLine("EXTRACCIÓN DE OPINIONES");

            var csvCount = await ExtractAsync("CSV", "csv.json", json,
                () => csv.ExtractAsync(Path.GetFullPath(settings["CsvPath"] ?? "Samples/encuestas.csv", folder)));

            var databaseCount = await ExtractAsync("SQL Server", "database.json", json, database.ExtractAsync);

            var apiCount = await ExtractAsync("API", "api.json", json,
                () => api.ExtractAsync(settings["ApiUrl"] ?? ""));

            Console.WriteLine("\nRESUMEN FINAL");
            PrintResult("CSV", csvCount);
            PrintResult("SQL Server", databaseCount);
            PrintResult("API", apiCount);

            var total = (csvCount ?? 0) + (databaseCount ?? 0) + (apiCount ?? 0);
            Console.WriteLine($"Total de registros guardados: {total}");

            var success = csvCount.HasValue && databaseCount.HasValue && apiCount.HasValue;
            Console.WriteLine(success ? "Proceso finalizado correctamente." : "Proceso finalizado con errores.");

            return success ? 0 : 1;
        }

        private static async Task<int?> ExtractAsync(
            string source,
            string fileName,
            JsonService json,
            Func<Task<List<Opinion>>> extract)
        {
            Console.WriteLine($"\n{source}");

            try
            {
                var opinions = await extract();
                Console.WriteLine($"Registros extraídos: {opinions.Count}");

                var path = await json.SaveAsync(opinions, fileName);
                Console.WriteLine($"Archivo: {path}");

                return opinions.Count;
            }
            catch (Exception error) when (error is IOException or UnauthorizedAccessException
                or InvalidDataException or CsvHelperException or SqlException or HttpRequestException
                or JsonException or OperationCanceledException or ArgumentException or InvalidOperationException)
            {
                Console.WriteLine($"Error en {source}: {error.Message}");

                if (json.HasPreviousFile(fileName))
                {
                    Console.WriteLine($"{fileName} se conserva de una ejecución anterior.");
                }

                return null;
            }
        }

        private static void PrintResult(string source, int? count)
        {
            Console.WriteLine(count.HasValue ? $"{source}: {count} registros" : $"{source}: error, sin salida nueva");
        }
    }
}
