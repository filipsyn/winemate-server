using Carter;

using ErrorOr;

using FluentValidation;

using Mapster;

using MediatR;

using WineMate.Catalog.Contracts;
using WineMate.Catalog.Database;
using WineMate.Catalog.Database.Entities;

namespace WineMate.Catalog.Features.Wines;

public static class CreateWine
{
    public class Command : IRequest<ErrorOr<Guid>>
    {
        public required string Name { get; set; }
        public string? Description { get; set; }
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
        private readonly IValidator<Command> _validator;

        public Handler(ApplicationDbContext dbContext, IValidator<Command> validator)
        {
            _dbContext = dbContext;
            _validator = validator;
        }

        public async Task<ErrorOr<Guid>> Handle(Command request, CancellationToken cancellationToken)
        {
            var validationResult = await _validator.ValidateAsync(request, cancellationToken);

            if (!validationResult.IsValid)
            {
                return Error.Failure(nameof(CreateWine), validationResult.ToString() ?? "Validation failed.");
            }

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

                var result = await sender.Send(command);
                if (result.IsError)
                {
                    return Results.BadRequest(result.Errors);
                }

                var response = new CreateWineResponse
                {
                    WineId = result.Value
                };

                return Results.Created($"/wines/{response.WineId}", response);
            })
            .WithOpenApi()
            .WithName("CreateWine")
            .WithSummary("Create Wine")
            .WithDescription("Creates a new wine")
            .WithTags("Wines");
    }
}
