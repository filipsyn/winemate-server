using Carter;

using ErrorOr;

using Mapster;

using MediatR;

using Microsoft.EntityFrameworkCore;

using WineMate.Catalog.Database;
using WineMate.Common.Extensions;
using WineMate.Contracts.Api;

namespace WineMate.Catalog.Features.Wines;

public static class GetWine
{
    public class Query : IRequest<ErrorOr<WineDetailResponse>>
    {
        public Guid Id { get; set; }
    }

    internal sealed class Handler : IRequestHandler<Query, ErrorOr<WineDetailResponse>>
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ILogger<Handler> _logger;

        public Handler(ApplicationDbContext dbContext, ILogger<Handler> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<ErrorOr<WineDetailResponse>> Handle(Query request, CancellationToken cancellationToken)
        {
            var wine = await _dbContext.Wines
                .AsNoTracking()
                .FirstOrDefaultAsync(wine => wine.Id == request.Id, cancellationToken);

            if (wine is null)
            {
                _logger.LogWarning("Wine with id {Id} not found", request.Id);
                return Error.NotFound("GetWine", $"Wine with id {request.Id} not found.");
            }

            return wine.Adapt<WineDetailResponse>();
        }
    }
}

public class GetWineEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/wines/{id}", async (Guid id, ISender sender) =>
            {
                var query = new GetWine.Query { Id = id };

                var result = await sender.Send(query);

                return result.MatchFirst(
                    TypedResults.Ok,
                    error => Results.NotFound(error.ToResponse())
                );
            })
            .WithOpenApi()
            .WithName("GetWine")
            .WithSummary("Get wine")
            .WithDescription("Get a wine by id")
            .WithTags("Wines");
    }
}
