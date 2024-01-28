using Carter;

using ErrorOr;

using Mapster;

using MediatR;

using Microsoft.EntityFrameworkCore;

using WineMate.Common.Extensions;
using WineMate.Contracts.Api;
using WineMate.Reviews.Database;

namespace WineMate.Reviews.Features.WineReviews;

public static class GetWineReview
{
    public class Query : IRequest<ErrorOr<WineReviewDetailResponse>>
    {
        public Guid Id { get; set; }
    }

    internal sealed class Handler : IRequestHandler<Query, ErrorOr<WineReviewDetailResponse>>
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ILogger<Handler> _logger;

        public Handler(ApplicationDbContext dbContext, ILogger<Handler> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<ErrorOr<WineReviewDetailResponse>> Handle(Query request, CancellationToken cancellationToken)
        {
            var review = await _dbContext.WineReviews
                .FirstOrDefaultAsync(review => review.Id == request.Id, cancellationToken);

            if (review is null)
            {
                _logger.LogWarning("Wine review with id {Id} was not found", request.Id);
                return Error.NotFound(nameof(GetWineReview), $"Wine review with id {request.Id} was not found");
            }

            return review.Adapt<WineReviewDetailResponse>();
        }
    }
}

public class GetWineReviewEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/reviews/{id}", async (Guid id, ISender sender) =>
            {
                var query = new GetWineReview.Query { Id = id };

                var result = await sender.Send(query);

                return result.Match(
                    TypedResults.Ok,
                    errors => errors.ToResponse()
                );
            })
            .WithOpenApi()
            .WithName("GetWineReview")
            .WithSummary("Get wine review")
            .WithDescription("Get a wine review by id")
            .WithTags("WineReviews");
    }
}
