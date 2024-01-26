using Carter;

using MediatR;

using Microsoft.EntityFrameworkCore;

using WineMate.Catalog.Contracts;
using WineMate.Catalog.Database;

namespace WineMate.Catalog.Features.WineMakers;

public static class ListWineMakers
{
    public class Query : IRequest<IList<WineMakerInfoResponse>> { }

    internal sealed class Handler : IRequestHandler<Query, IList<WineMakerInfoResponse>>
    {
        private readonly ApplicationDbContext _dbContext;

        public Handler(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IList<WineMakerInfoResponse>> Handle(Query request, CancellationToken cancellationToken)
        {
            var winemakers = await _dbContext.WineMakers
                .Select(wineMaker => new WineMakerInfoResponse
                {
                    Id = wineMaker.Id,
                    Name = wineMaker.Name
                })
                .ToListAsync(cancellationToken);

            return winemakers;
        }
    }
}

public class ListWineMakersEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/winemakers", async (ISender sender) =>
            {
                var query = new ListWineMakers.Query();

                var result = await sender.Send(query);

                return TypedResults.Ok(result);
            })
            .WithOpenApi()
            .WithName("ListWineMakers")
            .WithSummary("List wine makers")
            .WithDescription("List all wine makers")
            .WithTags("WineMakers");
    }
}
