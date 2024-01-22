using Carter;

using ErrorOr;

using FluentValidation;

using Mapster;

using MediatR;

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
        public required string Name { get; set; }
        public string? Description { get; set; } = null;
        public required int Year { get; set; }
        public WineType Type { get; set; } = WineType.Other;
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Name).NotEmpty();
            RuleFor(x => x.Year).InclusiveBetween(1800, DateTime.UtcNow.Year);
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

            wine.Name = request.Name;
            wine.Description = request.Description;
            wine.Year = request.Year;
            wine.Type = request.Type;

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
                async (Guid id, UpdateWineRequest request, ISender sender) =>
                {
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
