using Carter;

using MediatR;

using Microsoft.EntityFrameworkCore;

using WineMate.Contracts.Api;
using WineMate.Reviews.Database;

namespace WineMate.Reviews.Features.WineReviews;

public static class ListWineReviews
{
    public class Query : IRequest<IList<WineReviewInfoResponse>> { }

    internal sealed class Handler : IRequestHandler<Query, IList<WineReviewInfoResponse>>
    {
        private readonly ApplicationDbContext _dbContext;

        public Handler(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IList<WineReviewInfoResponse>> Handle(Query request, CancellationToken cancellationToken)
        {
            var reviews = await _dbContext.WineReviews
                .Select(review => new WineReviewInfoResponse
                {
                    Id = review.Id
                })
                .ToListAsync(cancellationToken);

            return reviews;
        }
    }
}

public class ListWineReviewsEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/reviews", async (ISender sender) =>
            {
                var query = new ListWineReviews.Query();

                var result = await sender.Send(query);

                return TypedResults.Ok(result);
            })
            .WithOpenApi()
            .WithName("ListWineReviews")
            .WithSummary("List wine reviews")
            .WithDescription("List all wine reviews")
            .WithTags("WineReviews");
    }
}
