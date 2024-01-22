using Carter;

using ErrorOr;

using FluentValidation;

using Mapster;

using MediatR;

using WineMate.Catalog.Configuration;
using WineMate.Catalog.Contracts;
using WineMate.Catalog.Database;
using WineMate.Catalog.Database.Entities;
using WineMate.Catalog.Extensions;

namespace WineMate.Catalog.Features.Wines;

public static class CreateWine
{
    public class Command : IRequest<ErrorOr<Guid>>
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int Year { get; set; }
        public WineType Type { get; set; } = WineType.Other;
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Name).NotEmpty();

            RuleFor(x => x.Description)
                .NotEqual(string.Empty)
                .When(x => x.Description != null);

            RuleFor(x => x.Year)
                .NotEmpty()
                .InclusiveBetween(Constants.MinimalAllowedYear, DateTime.UtcNow.Year);

            RuleFor(x => x.Type)
                .NotEmpty()
                .IsInEnum();
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
        app.MapPost("/wines", async (
                CreateWineRequest request,
                IValidator<CreateWineRequest> validator,
                ISender sender) =>
            {
                var validationResult = await validator.ValidateAsync(request);
                if (!validationResult.IsValid)
                {
                    return Results.ValidationProblem(validationResult.ToDictionary());
                }


                var command = request.Adapt<CreateWine.Command>();

                var result = await sender.Send(command);

                return result.MatchFirst(
                    wineId => Results.Created($"/wines/{wineId}", new CreateWineResponse { WineId = wineId }),
                    error => error.ToResponse()
                );
            })
            .WithOpenApi()
            .WithName("CreateWine")
            .WithSummary("Create Wine")
            .WithDescription("Creates a new wine")
            .WithTags("Wines");
    }
}
