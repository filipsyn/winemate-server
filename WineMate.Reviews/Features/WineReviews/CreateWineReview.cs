using Carter;

using ErrorOr;

using FluentValidation;

using Mapster;

using MassTransit;

using MediatR;

using WineMate.Common.Extensions;
using WineMate.Contracts.Api;
using WineMate.Contracts.Messages;
using WineMate.Reviews.Configuration;
using WineMate.Reviews.Database;
using WineMate.Reviews.Database.Entities;

namespace WineMate.Reviews.Features.WineReviews;

public static class CreateWineReview
{
    public class Command : IRequest<ErrorOr<Guid>>
    {
        public Guid WineId { get; set; }
        public string? Title { get; set; }
        public string? Body { get; set; }
        public int Rating { get; set; } = Constants.MinimumRating;
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

    internal sealed class Handler : IRequestHandler<Command, ErrorOr<Guid>>
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ILogger<Handler> _logger;
        private readonly IRequestClient<GetWineStatusRequest> _requestClient;
        private readonly IValidator<Command> _validator;

        public Handler(ApplicationDbContext dbContext, IValidator<Command> validator, ILogger<Handler> logger,
            IRequestClient<GetWineStatusRequest> requestClient)
        {
            _dbContext = dbContext;
            _validator = validator;
            _logger = logger;
            _requestClient = requestClient;
        }

        public async Task<ErrorOr<Guid>> Handle(Command request, CancellationToken cancellationToken)
        {
            var validationResult = await _validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                _logger.LogWarning("Can't log review; Validation Failed: {ValidationResult}",
                    validationResult.ToString());
                return Error.Validation(nameof(CreateWineReview), validationResult.ToString() ?? "Validation failed");
            }

            var wineStatusResponse = await _requestClient.GetResponse<GetWineStatusResponse>(
                new GetWineStatusRequest { WineId = request.WineId },
                cancellationToken);

            _logger.LogDebug("Wine status response: {WineStatusResponse}", wineStatusResponse.Message);

            if (!wineStatusResponse.Message.Exists)
            {
                _logger.LogWarning("Can't create wine review; Wine with id {WineId} not found", request.WineId);
                return Error.NotFound(nameof(CreateWineReview), $"Wine with id {request.WineId} not found");
            }

            var wineReview = new WineReview
            {
                WineId = request.WineId,
                Title = request.Title,
                Body = request.Body,
                Rating = request.Rating
            };

            await _dbContext.WineReviews.AddAsync(wineReview, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return wineReview.Id;
        }
    }
}

public class CreateWineReviewEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/reviews", async (
                CreateWineReviewRequest request,
                IValidator<CreateWineReviewRequest> validator,
                ISender sender
            ) =>
            {
                var validationResult = await validator.ValidateAsync(request);
                if (!validationResult.IsValid)
                {
                    return Results.ValidationProblem(validationResult.ToDictionary());
                }

                var command = request.Adapt<CreateWineReview.Command>();

                var result = await sender.Send(command);

                return result.Match(
                    reviewId => Results.Created($"/reviews/{reviewId}",
                        new CreateWineReviewResponse { WineReviewId = reviewId }),
                    error => error.ToResponse()
                );
            })
            .WithOpenApi()
            .WithName("CreateWineReview")
            .WithSummary("Create wine review")
            .WithDescription("Creates a new wine review")
            .WithTags("WineReviews");
    }
}
