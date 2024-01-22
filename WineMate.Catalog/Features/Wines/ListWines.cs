using Carter;
using MediatR;
using Microsoft.EntityFrameworkCore;
using WineMate.Catalog.Contracts;
using WineMate.Catalog.Database;

namespace WineMate.Catalog.Features.Wines;

public static class ListWines
{
    public class Query : IRequest<IList<WineInfoResponse>> { }

    internal sealed class Handler : IRequestHandler<Query, IList<WineInfoResponse>>
    {
        private readonly ApplicationDbContext _dbContext;

        public Handler(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IList<WineInfoResponse>> Handle(Query request, CancellationToken cancellationToken)
        {
            var wines = await _dbContext.Wines
                .Select(wine => new WineInfoResponse
                {
                    Id = wine.Id,
                    Name = wine.Name
                })
                .ToListAsync(cancellationToken);

            return wines;
        }
    }
}

public class ListWinesEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/wines", async (ISender sender) =>
            {
                var query = new ListWines.Query();

                var result = await sender.Send(query);

                return TypedResults.Ok(result);
            })
            .WithOpenApi()
            .WithName("ListWines")
            .WithSummary("List wines")
            .WithDescription("List all wines")
            .WithTags("Wines");
    }
}
