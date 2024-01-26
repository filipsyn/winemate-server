using Carter;

using ErrorOr;

using FluentValidation;

using Mapster;

using MediatR;

using Microsoft.EntityFrameworkCore;

using WineMate.Catalog.Contracts;
using WineMate.Catalog.Database;
using WineMate.Catalog.Database.Entities;
using WineMate.Catalog.Extensions;
using WineMate.Catalog.Validators;

namespace WineMate.Catalog.Features.WineMakers;

public static class UpdateWineMaker
{
    public class Command : IRequest<ErrorOr<Guid>>
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public Address Address { get; set; } = new();
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(command => command.Name)
                .NotEmpty();

            RuleFor(command => command.Address)
                .NotEmpty()
                .SetValidator(new AddressValidator());
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
            var winemaker = await _dbContext.WineMakers
                .Include(wineMaker => wineMaker.Address)
                .FirstOrDefaultAsync(winemaker => winemaker.Id == request.Id, cancellationToken);
            if (winemaker is null)
            {
                _logger.LogWarning("WineMaker with id {Id} not found", request.Id);
                return Error.NotFound("UpdateWineMaker", $"WineMaker with id {request.Id} not found.");
            }

            winemaker.Name = request.Name;
            winemaker.Address.Number = request.Address.Number;
            winemaker.Address.Street = request.Address.Street;
            winemaker.Address.City = request.Address.City;
            winemaker.Address.Country = request.Address.Country;

            await _dbContext.SaveChangesAsync(cancellationToken);

            return winemaker.Id;
        }
    }
}

public class UpdateWineMakerEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("/winemakers/{id}", async (
                Guid id,
                UpdateWineMakerRequest request,
                IValidator<UpdateWineMakerRequest> validator,
                ISender sender
            ) =>
            {
                var validationResult = validator.Validate(request);
                if (!validationResult.IsValid)
                {
                    return Results.ValidationProblem(validationResult.ToDictionary());
                }

                var command = request.Adapt<UpdateWineMaker.Command>();
                command.Id = id;

                var result = await sender.Send(command);

                return result.Match(
                    wineMakerId => TypedResults.Accepted($"/winemakers/{wineMakerId}", wineMakerId),
                    errors => errors.ToResponse()
                );
            })
            .WithOpenApi()
            .WithName("UpdateWineMaker")
            .WithSummary("Update wine maker")
            .WithDescription("Updates a wine maker.")
            .WithTags("WineMakers");
    }
}
