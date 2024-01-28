using Carter;

using ErrorOr;

using MediatR;

using Microsoft.EntityFrameworkCore;

using WineMate.Common.Extensions;
using WineMate.Reviews.Database;

namespace WineMate.Reviews.Features.WineReviews;

public static class DeleteWineReview
{
    public class Command : IRequest<ErrorOr<Deleted>>
    {
        public Guid Id { get; set; }
    }

    internal sealed class Handler : IRequestHandler<Command, ErrorOr<Deleted>>
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ILogger<Handler> _logger;

        public Handler(ApplicationDbContext dbContext, ILogger<Handler> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<ErrorOr<Deleted>> Handle(Command request, CancellationToken cancellationToken)
        {
            var review = await _dbContext.WineReviews
                .FirstOrDefaultAsync(review => review.Id == request.Id, cancellationToken);

            if (review is null)
            {
                _logger.LogWarning("Wine review with id {Id} not found", request.Id);
                return Error.NotFound(nameof(DeleteWineReview), $"Wine review with id {request.Id} not found.");
            }

            _dbContext.WineReviews.Remove(review);
            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Wine review with id {Id} deleted", request.Id);
            return Result.Deleted;
        }
    }
}

public class DeleteWineReviewEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("/reviews/{id}", async (Guid id, ISender sender) =>
            {
                var command = new DeleteWineReview.Command { Id = id };

                var result = await sender.Send(command);

                return result.Match(
                    _ => TypedResults.NoContent(),
                    errors => errors.ToResponse()
                );
            })
            .WithOpenApi()
            .WithName("DeleteWineReview")
            .WithSummary("Delete wine review")
            .WithDescription("Delete a wine review by id.")
            .WithTags("WineReviews");
    }
}
