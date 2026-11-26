using System.Globalization;
using AnalisisOpiniones.ETL.Models;
using CsvHelper;
using CsvHelper.Configuration;

namespace AnalisisOpiniones.ETL.Services
{
    public sealed class CsvService
    {
        public async Task<List<Opinion>> ExtractAsync(string path)
        {
            var malformed = false;
            var configuration = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                ExceptionMessagesContainRawData = false,
                BadDataFound = _ => malformed = true
            };

            using var reader = new StreamReader(path);
            using var csv = new CsvReader(reader, configuration);

            if (!await csv.ReadAsync())
            {
                throw new InvalidDataException("El CSV está vacío y no contiene encabezados.");
            }

            csv.ReadHeader();
            string[] expected =
            {
                "Id", "ProductId", "Comment", "Rating", "CreatedAt"
            };

            if (malformed || !expected.SequenceEqual(csv.HeaderRecord!))
            {
                throw new InvalidDataException("Los encabezados del CSV no coinciden con el formato esperado.");
            }

            var opinions = new List<Opinion>();
            var rejected = 0;

            while (await csv.ReadAsync())
            {
                var opinion = ReadOpinion(csv, expected.Length);
                if (malformed || opinion is null)
                {
                    rejected++;
                }
                else
                {
                    opinions.Add(opinion);
                }

                malformed = false;
            }

            if (rejected > 0)
            {
                Console.WriteLine($"Filas CSV omitidas por datos inválidos: {rejected}.");
            }

            return opinions;
        }

        private static Opinion? ReadOpinion(CsvReader csv, int columnCount)
        {
            if (csv.Parser.Count != columnCount
                || !csv.TryGetField<int>(1, out var productId)
                || !csv.TryGetField<int?>(3, out var rating)
                || !csv.TryGetField<DateTimeOffset?>(4, out var date))
            {
                return null;
            }

            var id = csv.GetField(0);
            var comment = csv.GetField(2);

            if (productId <= 0 || string.IsNullOrWhiteSpace(comment) || comment.Length > 2000
                || rating is < 1 or > 5)
            {
                return null;
            }

            return new Opinion(string.IsNullOrWhiteSpace(id) ? null : id, productId, comment, rating, date);
        }
    }
}
