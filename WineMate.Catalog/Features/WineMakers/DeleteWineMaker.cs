using Carter;

using ErrorOr;

using MediatR;

using Microsoft.EntityFrameworkCore;

using WineMate.Catalog.Database;
using WineMate.Catalog.Extensions;

namespace WineMate.Catalog.Features.WineMakers;

public static class DeleteWineMaker
{
    public class Command : IRequest<ErrorOr<Deleted>>
    {
        public Guid Id { get; init; }
    }

    internal sealed class Handler : IRequestHandler<Command, ErrorOr<Deleted>>
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ILogger<Handler> _handler;

        public Handler(ApplicationDbContext dbContext, ILogger<Handler> handler)
        {
            _dbContext = dbContext;
            _handler = handler;
        }

        public async Task<ErrorOr<Deleted>> Handle(Command request, CancellationToken cancellationToken)
        {
            var winemaker = await _dbContext.WineMakers
                .FirstOrDefaultAsync(maker => maker.Id == request.Id, cancellationToken);

            if (winemaker is null)
            {
                _handler.LogWarning("Wine maker with id {Id} not found", request.Id);
                return Error.NotFound(nameof(DeleteWineMaker), $"WineMaker with id {request.Id} not found.");
            }

            _dbContext.WineMakers.Remove(winemaker);
            await _dbContext.SaveChangesAsync(cancellationToken);

            _handler.LogInformation("Wine maker with id {Id} deleted", request.Id);

            return Result.Deleted;
        }
    }
}

public class DeleteWineMakerEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("/winemakers/{id}", async (
                Guid id,
                ISender sender
            ) =>
            {
                var command = new DeleteWineMaker.Command { Id = id };

                var result = await sender.Send(command);

                return result.Match(
                    _ => TypedResults.NoContent(),
                    errors => errors.ToResponse()
                );
            })
            .WithOpenApi()
            .WithName("DeleteWineMaker")
            .WithSummary("Delete wine maker")
            .WithDescription("Deletes a wine maker")
            .WithTags("WineMakers");
    }
}
