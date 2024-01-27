using Carter;

using ErrorOr;

using FluentValidation;

using Mapster;

using MediatR;

using WineMate.Contracts.Api;
using WineMate.Reviews.Configuration;
using WineMate.Reviews.Database;
using WineMate.Reviews.Database.Entities;
using WineMate.Reviews.Extensions;

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
                .NotEmpty()
                .MaximumLength(128);

            RuleFor(x => x.Body)
                .NotEmpty();

            RuleFor(x => x.Rating)
                .NotEmpty()
                .InclusiveBetween(Constants.MinimumRating, Constants.MaximumRating);
        }
    }

    internal sealed class Handler : IRequestHandler<Command, ErrorOr<Guid>>
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ILogger<Handler> _logger;
        private readonly IValidator<Command> _validator;

        public Handler(ApplicationDbContext dbContext, IValidator<Command> validator, ILogger<Handler> logger)
        {
            _dbContext = dbContext;
            _validator = validator;
            _logger = logger;
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
