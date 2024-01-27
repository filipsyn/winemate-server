using Carter;

using ErrorOr;

using FluentValidation;

using Mapster;

using MediatR;

using Microsoft.EntityFrameworkCore;

using WineMate.Catalog.Configuration;
using WineMate.Catalog.Contracts;
using WineMate.Catalog.Database;
using WineMate.Catalog.Database.Entities;
using WineMate.Catalog.Extensions;

namespace WineMate.Catalog.Features.Wines;

public static class UpdateWine
{
    public class Command : IRequest<ErrorOr<Guid>>
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; } = null;
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

        public Handler(ApplicationDbContext dbContext, ILogger<Handler> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<ErrorOr<Guid>> Handle(Command request, CancellationToken cancellationToken)
        {
            var wine = await _dbContext.Wines.FindAsync(request.Id);

            if (wine is null)
            {
                _logger.LogWarning("Wine with id {Id} not found", request.Id);
                return Error.Failure(nameof(UpdateWine), $"Wine with id {request.Id} not found.");
            }

            var winemaker = await _dbContext.WineMakers
                .FirstOrDefaultAsync(maker => maker.Id == request.WineMakerId, cancellationToken);

            if (winemaker is null)
            {
                _logger.LogWarning("Can't update wine {Id}; Wine maker with id {WineMakerId} not found",
                    request.Id,
                    request.WineMakerId);
                return Error.Failure(nameof(UpdateWine), $"Wine maker with id {request.WineMakerId} not found.");
            }

            wine.Name = request.Name;
            wine.Description = request.Description;
            wine.Year = request.Year;
            wine.Type = request.Type;
            wine.WineMakerId = request.WineMakerId;
            wine.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync(cancellationToken);

            return wine.Id;
        }
    }
}

public class UpdateWineEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("/wines/{id}",
                async (
                    Guid id,
                    UpdateWineRequest request,
                    IValidator<UpdateWineRequest> validator,
                    ISender sender
                ) =>
                {
                    var validationResult = await validator.ValidateAsync(request);
                    if (!validationResult.IsValid)
                    {
                        return Results.ValidationProblem(validationResult.ToDictionary());
                    }

                    var command = request.Adapt<UpdateWine.Command>();
                    command.Id = id;

                    var result = await sender.Send(command);

                    return result.MatchFirst(
                        wineId => TypedResults.Accepted($"/wines/{wineId}"),
                        error => error.ToResponse());
                }
            )
            .WithOpenApi()
            .WithName("UpdateWine")
            .WithSummary("Update Wine")
            .WithDescription("Updates a wine.")
            .WithTags("Wines");
    }
}
