using Carter;

using ErrorOr;

using Mapster;

using MediatR;

using Microsoft.EntityFrameworkCore;

using WineMate.Catalog.Database;
using WineMate.Common.Extensions;
using WineMate.Contracts.Api;

namespace WineMate.Catalog.Features.WineMakers;

public static class GetWineMaker
{
    public class Query : IRequest<ErrorOr<WineMakerDetailResponse>>
    {
        public Guid Id { get; set; }
    }

    internal sealed class Handler : IRequestHandler<Query, ErrorOr<WineMakerDetailResponse>>
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ILogger<Handler> _logger;

        public Handler(ApplicationDbContext dbContext, ILogger<Handler> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<ErrorOr<WineMakerDetailResponse>> Handle(Query request, CancellationToken cancellationToken)
        {
            var wineMaker = await _dbContext.WineMakers
                .AsNoTracking()
                .FirstOrDefaultAsync(wineMaker => wineMaker.Id == request.Id, cancellationToken);

            if (wineMaker is null)
            {
                _logger.LogWarning("Wine maker with id {Id} not found", request.Id);
                return Error.NotFound("GetWineMaker", $"Wine maker with id {request.Id} not found.");
            }

            return wineMaker.Adapt<WineMakerDetailResponse>();
        }
    }
}

public class GetWineMakerEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/winemakers/{id}", async (Guid id, ISender sender) =>
            {
                var query = new GetWineMaker.Query { Id = id };

                var result = await sender.Send(query);

                return result.MatchFirst(
                    TypedResults.Ok,
                    error => error.ToResponse());
            })
            .WithOpenApi()
            .WithName("GetWineMaker")
            .WithSummary("Get wine maker")
            .WithDescription("Get a wine maker by id.")
            .WithTags("WineMakers");
    }
}
