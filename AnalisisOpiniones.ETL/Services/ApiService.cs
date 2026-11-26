using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using AnalisisOpiniones.ETL.Models;

namespace AnalisisOpiniones.ETL.Services
{
    public sealed class ApiService
    {
        private readonly HttpClient _client;

        public ApiService(HttpClient client)
        {
            _client = client;
        }

        public async Task<List<Opinion>> ExtractAsync(string url)
        {
            if (!Uri.TryCreate(url, UriKind.Absolute, out var address) || address.Scheme is not ("http" or "https"))
            {
                throw new ArgumentException("Configura una URL HTTP o HTTPS en Extraction:ApiUrl.");
            }

            using var response = await _client.GetAsync(address);
            response.EnsureSuccessStatusCode();

            if (response.StatusCode == HttpStatusCode.NoContent)
            {
                return new List<Opinion>();
            }

            var entries = await response.Content.ReadFromJsonAsync<List<ApiOpinion?>>()
                ?? throw new InvalidDataException("La API debe devolver un arreglo de opiniones.");

            var opinions = new List<Opinion>();

            foreach (var entry in entries)
            {
                if (entry is null || entry.ProductId <= 0 || string.IsNullOrWhiteSpace(entry.Comment)
                    || entry.Comment.Length > 2000 || entry.Rating is < 1 or > 5)
                {
                    throw new InvalidDataException("La API devolvió una opinión con datos inválidos.");
                }

                var id = entry.Id?.ToString(CultureInfo.InvariantCulture);
                var opinion = new Opinion(id, entry.ProductId, entry.Comment, entry.Rating, entry.CreatedAt);

                opinions.Add(opinion);
            }

            return opinions;
        }

        private sealed record ApiOpinion(
            int? Id,
            int ProductId,
            string? Comment,
            int? Rating,
            DateTimeOffset? CreatedAt);
    }
}
