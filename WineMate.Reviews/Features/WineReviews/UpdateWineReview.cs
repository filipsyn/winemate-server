using Carter;

using ErrorOr;

using FluentValidation;

using Mapster;

using MassTransit;

using MediatR;

using Microsoft.EntityFrameworkCore;

using WineMate.Common.Extensions;
using WineMate.Contracts.Api;
using WineMate.Contracts.Messages;
using WineMate.Reviews.Configuration;
using WineMate.Reviews.Database;

namespace WineMate.Reviews.Features.WineReviews;

public static class UpdateWineReview
{
    public class Command : IRequest<ErrorOr<UpdateWineReviewResponse>>
    {
        public Guid Id { get; set; }
        public Guid WineId { get; set; }
        public string? Title { get; set; }
        public string? Body { get; set; }
        public int Rating { get; set; }
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.WineId)
                .NotEmpty();

            RuleFor(x => x.Title)
                .MaximumLength(128);

            RuleFor(x => x.Rating)
                .NotEmpty()
                .InclusiveBetween(Constants.MinimumRating, Constants.MaximumRating);
        }
    }

    internal sealed class Handler : IRequestHandler<Command, ErrorOr<UpdateWineReviewResponse>>
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ILogger<Handler> _logger;
        private readonly IRequestClient<GetWineStatusRequest> _requestClient;
        private readonly IValidator<Command> _validator;

        public Handler(
            ApplicationDbContext dbContext,
            IValidator<Command> validator,
            ILogger<Handler> logger,
            IRequestClient<GetWineStatusRequest> requestClient
        )
        {
            _dbContext = dbContext;
            _validator = validator;
            _logger = logger;
            _requestClient = requestClient;
        }

        public async Task<ErrorOr<UpdateWineReviewResponse>> Handle(Command request,
            CancellationToken cancellationToken)
        {
            var validationResult = await _validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                _logger.LogWarning("Cannot update review with id {Id}, validation failed: {ValidationResult}",
                    request.Id, validationResult.ToDictionary());
                return Error.Validation(nameof(UpdateWineReview), validationResult.ToString() ?? "Validation failed");
            }

            var review = await _dbContext.WineReviews
                .FirstOrDefaultAsync(review => review.Id == request.Id, cancellationToken);

            if (review is null)
            {
                _logger.LogWarning("Cannot update review with id {Id}, not found", request.Id);
                return Error.NotFound(nameof(UpdateWineReview), $"Review with id {request.Id} not found.");
            }

            var wineStatusResponse = await _requestClient.GetResponse<GetWineStatusResponse>(
                new GetWineStatusRequest { WineId = request.WineId },
                cancellationToken);

            if (!wineStatusResponse.Message.Exists)
            {
                _logger.LogWarning("Can't update wine review; Wine with id {WineId} not found", request.WineId);
                return Error.NotFound(nameof(UpdateWineReview), $"Wine with id {request.WineId} not found");
            }

            review.WineId = request.WineId;
            review.Title = request.Title;
            review.Body = request.Body;
            review.Rating = request.Rating;
            review.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync(cancellationToken);

            return new UpdateWineReviewResponse { Id = review.Id };
        }
    }
}

public class UpdateWineReviewEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("/reviews/{id}", async (
                Guid id,
                UpdateWineReviewRequest request,
                IValidator<UpdateWineReviewRequest> validator,
                ISender sender) =>
            {
                var validationResult = await validator.ValidateAsync(request);
                if (!validationResult.IsValid)
                {
                    return Results.ValidationProblem(validationResult.ToDictionary());
                }

                var command = request.Adapt<UpdateWineReview.Command>();
                command.Id = id;

                var result = await sender.Send(command);

                return result.Match(
                    response => TypedResults.Accepted($"/reviews/{response.Id}", response),
                    errors => errors.ToResponse()
                );
            })
            .WithOpenApi()
            .WithName("UpdateWineReview")
            .WithSummary("Update wine review")
            .WithDescription("Updates a wine review")
            .WithTags("WineReviews");
    }
}
