using System.Text.Json;
using AnalisisOpiniones.ETL.Models;

namespace AnalisisOpiniones.ETL.Services
{
    public sealed class JsonService
    {
        private static readonly JsonSerializerOptions _settings = new(JsonSerializerDefaults.Web)
        {
            WriteIndented = true
        };

        private readonly string _stagingPath;

        public JsonService(string stagingPath)
        {
            _stagingPath = stagingPath;
        }

        public async Task<string> SaveAsync(List<Opinion> opinions, string fileName)
        {
            Directory.CreateDirectory(_stagingPath);

            var path = Path.Combine(_stagingPath, fileName);
            var temporaryPath = Path.Combine(_stagingPath, $"{Guid.NewGuid():N}.tmp");

            try
            {
                await using (var stream = File.Create(temporaryPath))
                {
                    await JsonSerializer.SerializeAsync(stream, opinions, _settings);
                }

                File.Move(temporaryPath, path, overwrite: true);

                return path;
            }
            finally
            {
                File.Delete(temporaryPath);
            }
        }

        public bool HasPreviousFile(string fileName)
        {
            return File.Exists(Path.Combine(_stagingPath, fileName));
        }
    }
}
