namespace AnalisisOpiniones.Api.Models
{
    public sealed record Opinion(int Id, int ProductId, string Comment, int Rating, DateTimeOffset CreatedAt);
}
