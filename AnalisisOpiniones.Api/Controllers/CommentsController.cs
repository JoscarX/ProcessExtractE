using AnalisisOpiniones.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace AnalisisOpiniones.Api.Controllers
{
    [ApiController]
    [Route("api")]
    public sealed class CommentsController : ControllerBase
    {
        private readonly OpinionService _service;

        public CommentsController(OpinionService service)
        {
            _service = service;
        }

        [HttpGet("comments")]
        public List<CommentDto> GetComments()
        {
            return _service.GetOpinions()
                .Select(opinion => new CommentDto(opinion.Id, opinion.ProductId, opinion.Comment, opinion.CreatedAt))
                .ToList();
        }

        [HttpGet("reviews")]
        public List<ReviewDto> GetReviews()
        {
            return _service.GetOpinions()
                .Select(opinion => new ReviewDto(
                    opinion.Id, opinion.ProductId, opinion.Comment, opinion.Rating, opinion.CreatedAt))
                .ToList();
        }

        public sealed record CommentDto(int Id, int ProductId, string Comment, DateTimeOffset CreatedAt);
        public sealed record ReviewDto(int Id, int ProductId, string Comment, int? Rating, DateTimeOffset CreatedAt);
    }
}
