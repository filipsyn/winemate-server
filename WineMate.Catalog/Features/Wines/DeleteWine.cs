using Carter;

using ErrorOr;

using MediatR;

using Microsoft.EntityFrameworkCore;

using WineMate.Catalog.Database;
using WineMate.Common.Extensions;

namespace WineMate.Catalog.Features.Wines;

public static class DeleteWine
{
    public class Command : IRequest<ErrorOr<Deleted>>
    {
        public Guid Id { get; set; }
    }

    internal sealed class Handler : IRequestHandler<Command, ErrorOr<Deleted>>
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ILogger<Handler> _logger;

        public Handler(ApplicationDbContext dbContext, ILogger<Handler> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<ErrorOr<Deleted>> Handle(Command request, CancellationToken cancellationToken)
        {
            var wine = await _dbContext.Wines
                .FirstOrDefaultAsync(wine => wine.Id == request.Id, cancellationToken);

            if (wine is null)
            {
                _logger.LogWarning("Wine with id {Id} not found", request.Id);
                return Error.NotFound(nameof(DeleteWine), $"Wine with id {request.Id} not found.");
            }

            _dbContext.Wines.Remove(wine);

            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Wine with id {Id} deleted", request.Id);

            return Result.Deleted;
        }
    }
}

public class DeleteWineEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("/wines/{id}",
                async (Guid id, ISender sender) =>
                {
                    var command = new DeleteWine.Command { Id = id };

                    var result = await sender.Send(command);


                    return result.MatchFirst(
                        _ => Results.NoContent(),
                        error => error.ToResponse()
                    );
                })
            .WithOpenApi()
            .WithName("DeleteWine")
            .WithSummary("Delete wine")
            .WithDescription("Deletes a wine")
            .WithTags("Wines");
    }
}
