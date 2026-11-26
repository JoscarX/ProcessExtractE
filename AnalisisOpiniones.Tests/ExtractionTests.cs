using System.Net;
using System.Text.Json;
using AnalisisOpiniones.ETL.Models;
using AnalisisOpiniones.ETL.Services;

namespace AnalisisOpiniones.Tests
{
    public sealed class ExtractionTests : IDisposable
    {
        private readonly DirectoryInfo _temporary = Directory.CreateTempSubdirectory("opiniones-tests-");

        [Fact]
        public async Task CsvReadsQuotedCommentAndSkipsInvalidRows()
        {
            var path = Path.Combine(_temporary.FullName, "encuestas.csv");
            string[] lines =
            {
                "Id,ProductId,Comment,Rating,CreatedAt",
                "1,2,\"Cómodo, ligero\",5,2026-10-02T12:00:00-03:00",
                "2,abc,Producto inválido,4,",
                "3,2,Calificación inválida,6,",
                "4,2,Solo texto,,",
                "5,2,comilla\"inválida,4,",
                "6,2,,4,"
            };

            await File.WriteAllLinesAsync(path, lines);

            var opinions = await new CsvService().ExtractAsync(path);

            Assert.Equal(2, opinions.Count);
            Assert.Equal("Cómodo, ligero", opinions[0].Comment);
            Assert.Equal(TimeSpan.FromHours(-3), opinions[0].CreatedAt!.Value.Offset);
            Assert.Null(opinions[1].Rating);
            Assert.Null(opinions[1].CreatedAt);
        }

        [Fact]
        public async Task MissingCsvReportsFailure()
        {
            var path = Path.Combine(_temporary.FullName, "ausente.csv");

            await Assert.ThrowsAsync<FileNotFoundException>(() => new CsvService().ExtractAsync(path));
        }

        [Fact]
        public async Task InvalidCsvHeaderReportsFailure()
        {
            var path = Path.Combine(_temporary.FullName, "encuestas.csv");
            await File.WriteAllTextAsync(path, "Id,Comment");

            await Assert.ThrowsAsync<InvalidDataException>(() => new CsvService().ExtractAsync(path));
        }

        [Fact]
        public async Task ApiDeserializesCommentsWithoutRating()
        {
            using var client = new HttpClient(new StubHandler(HttpStatusCode.OK, """
                [{"id":5,"productId":2,"comment":"Funciona bien","createdAt":"2026-10-02T12:00:00-03:00"}]
                """));

            var opinions = await new ApiService(client).ExtractAsync("http://localhost/api/comments");

            var opinion = Assert.Single(opinions);

            Assert.Equal("5", opinion.Id);
            Assert.Equal("Funciona bien", opinion.Comment);
            Assert.Null(opinion.Rating);
        }

        [Fact]
        public async Task ApiReportsHttpFailure()
        {
            using var client = new HttpClient(new StubHandler(HttpStatusCode.ServiceUnavailable, "[]"));

            var error = await Assert.ThrowsAsync<HttpRequestException>(
                () => new ApiService(client).ExtractAsync("http://localhost/api/comments"));

            Assert.Equal(HttpStatusCode.ServiceUnavailable, error.StatusCode);
        }

        [Fact]
        public async Task ApiReportsInvalidJson()
        {
            using var client = new HttpClient(new StubHandler(HttpStatusCode.OK, "[{"));

            await Assert.ThrowsAnyAsync<JsonException>(
                () => new ApiService(client).ExtractAsync("http://localhost/api/comments"));
        }

        [Fact]
        public async Task ApiAcceptsNoContent()
        {
            using var client = new HttpClient(new StubHandler(HttpStatusCode.NoContent, ""));

            var opinions = await new ApiService(client).ExtractAsync("http://localhost/api/comments");

            Assert.Empty(opinions);
        }

        [Fact]
        public async Task ApiRejectsNullEntries()
        {
            using var client = new HttpClient(new StubHandler(HttpStatusCode.OK, "[null]"));

            await Assert.ThrowsAsync<InvalidDataException>(
                () => new ApiService(client).ExtractAsync("http://localhost/api/comments"));
        }

        [Fact]
        public async Task JsonReplacesPreviousFileWithIndentedContent()
        {
            var directory = Path.Combine(_temporary.FullName, "Staging");
            var staging = new JsonService(directory);
            await staging.SaveAsync([new Opinion("1", 2, "Original", 4, null)], "csv.json");

            var path = await staging.SaveAsync([new Opinion("2", 2, "Nuevo", 5, null)], "csv.json");

            var content = await File.ReadAllTextAsync(path);
            using var document = JsonDocument.Parse(content);
            var opinion = Assert.Single(document.RootElement.EnumerateArray());
            Assert.Equal("Nuevo", opinion.GetProperty("comment").GetString());
            Assert.Contains("\n  {", content);
            Assert.Single(Directory.GetFiles(directory));
        }

        public void Dispose()
        {
            _temporary.Delete(recursive: true);
        }

        private sealed class StubHandler : HttpMessageHandler
        {
            private readonly HttpStatusCode _statusCode;
            private readonly string _body;

            public StubHandler(HttpStatusCode statusCode, string body)
            {
                _statusCode = statusCode;
                _body = body;
            }

            protected override Task<HttpResponseMessage> SendAsync(
                HttpRequestMessage request, CancellationToken cancellationToken)
            {
                var response = new HttpResponseMessage(_statusCode)
                {
                    Content = new StringContent(_body)
                };

                return Task.FromResult(response);
            }
        }
    }
}
