namespace AnalisisOpiniones.ETL.Models
{
    public sealed record Opinion(
        string? Id,
        int ProductId,
        string Comment,
        int? Rating,
        DateTimeOffset? CreatedAt);
}
