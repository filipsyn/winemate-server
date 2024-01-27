using Carter;

using ErrorOr;

using FluentValidation;

using Mapster;

using MediatR;

using Microsoft.EntityFrameworkCore;

using WineMate.Catalog.Configuration;
using WineMate.Catalog.Database;
using WineMate.Catalog.Database.Entities;
using WineMate.Catalog.Extensions;
using WineMate.Contracts.Api;
using WineMate.Contracts.Common;

namespace WineMate.Catalog.Features.Wines;

public static class CreateWine
{
    public class Command : IRequest<ErrorOr<Guid>>
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int Year { get; set; }
        public WineType Type { get; set; } = WineType.Other;
        public Guid WineMakerId { get; set; }
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Name).NotEmpty();

            RuleFor(x => x.Description)
                .NotEqual(string.Empty)
                .When(x => x.Description != null);

            RuleFor(x => x.Year)
                .NotEmpty()
                .InclusiveBetween(Constants.MinimalAllowedYear, DateTime.UtcNow.Year);

            RuleFor(x => x.Type)
                .NotEmpty()
                .IsInEnum();

            RuleFor(x => x.WineMakerId)
                .NotEmpty();
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
                _logger.LogWarning("Can't create wine, validation failed: {ValidationResult}",
                    validationResult.ToString() ?? "Validation failed.");
                return Error.Validation(nameof(CreateWine), validationResult.ToString() ?? "Validation failed.");
            }

            var winemaker = await _dbContext.WineMakers
                .FirstOrDefaultAsync(maker => maker.Id == request.WineMakerId, cancellationToken);

            if (winemaker is null)
            {
                _logger.LogWarning("Can't create wine, wine maker with id {Id} not found", request.WineMakerId);
                return Error.Failure(nameof(CreateWine), $"Wine maker with id {request.WineMakerId} not found.");
            }

            var wine = new Wine
            {
                Name = request.Name,
                Description = request.Description,
                Year = request.Year,
                Type = request.Type,
                CreatedAt = DateTime.UtcNow,
                WineMakerId = request.WineMakerId
            };

            _dbContext.Wines.Add(wine);

            await _dbContext.SaveChangesAsync(cancellationToken);

            return wine.Id;
        }
    }
}

public class CreateWineEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/wines", async (
                CreateWineRequest request,
                IValidator<CreateWineRequest> validator,
                ISender sender) =>
            {
                var validationResult = await validator.ValidateAsync(request);
                if (!validationResult.IsValid)
                {
                    return Results.ValidationProblem(validationResult.ToDictionary());
                }

                var command = request.Adapt<CreateWine.Command>();

                var result = await sender.Send(command);

                return result.MatchFirst(
                    wineId => Results.Created($"/wines/{wineId}", new CreateWineResponse { WineId = wineId }),
                    error => error.ToResponse()
                );
            })
            .WithOpenApi()
            .WithName("CreateWine")
            .WithSummary("Create Wine")
            .WithDescription("Creates a new wine")
            .WithTags("Wines");
    }
}
