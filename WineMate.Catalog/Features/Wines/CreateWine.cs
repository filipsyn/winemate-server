using Carter;
using Mapster;
using MediatR;
using WineMate.Catalog.Contracts;
using WineMate.Catalog.Database;
using WineMate.Catalog.Database.Entities;

namespace WineMate.Catalog.Features.Wines;

public static class CreateWine
{
    public class Command : IRequest<Guid>
    {
        public required string Name { get; set; }
        public string? Description { get; set; }
        public required int Year { get; set; }
        public WineType Type { get; set; } = WineType.Other;
    }

    internal sealed class Handler : IRequestHandler<Command, Guid>
    {
        private readonly ApplicationDbContext _dbContext;

        public Handler(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Guid> Handle(Command request, CancellationToken cancellationToken)
        {
            var wine = new Wine
            {
                Name = request.Name,
                Description = request.Description,
                Year = request.Year,
                Type = request.Type,
                CreatedAt = DateTime.UtcNow
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
        app.MapPost("/wines", async (CreateWineRequest request, ISender sender) =>
            {
                var command = request.Adapt<CreateWine.Command>();

                var wineId = await sender.Send(command);

                var response = new CreateWineResponse
                {
                    WineId = wineId
                };

                return TypedResults.Created($"/wines/{wineId}", response);
            })
            .WithOpenApi()
            .WithName("CreateWine")
            .WithSummary("Create Wine")
            .WithDescription("Creates a new wine")
            .WithTags("Wines");
    }
}
