using Carter;

using ErrorOr;

using FluentValidation;

using Mapster;

using MediatR;

using WineMate.Catalog.Configuration.Policies;
using WineMate.Catalog.Database;
using WineMate.Catalog.Database.Entities;
using WineMate.Catalog.Validators;
using WineMate.Common.Extensions;
using WineMate.Contracts.Api;
using WineMate.Contracts.Common;

namespace WineMate.Catalog.Features.WineMakers;

public class CreateWineMaker
{
    public class Command : IRequest<ErrorOr<Guid>>
    {
        public string Name { get; set; }
        public Address Address { get; set; }
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(command => command.Name)
                .NotEmpty();

            RuleFor(command => command.Address)
                .SetValidator(new AddressValidator());
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
                return Error.Validation(nameof(CreateWineMaker), validationResult.ToString() ?? "Validation failed.");
            }

            var wineMaker = new WineMaker
            {
                Name = request.Name,
                Address = request.Address
            };

            await _dbContext.WineMakers.AddAsync(wineMaker, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return wineMaker.Id;
        }
    }
}

public class CreateWineMakerEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/winemakers", async (
                CreateWineMakerRequest request,
                IValidator<CreateWineMakerRequest> validator,
                ISender sender
            ) =>
            {
                var validationResult = await validator.ValidateAsync(request);
                if (!validationResult.IsValid)
                {
                    return Results.ValidationProblem(validationResult.ToDictionary());
                }

                var command = request.Adapt<CreateWineMaker.Command>();
                var result = await sender.Send(command);

                return result.Match(
                    wineMakerId => Results.Created($"/winemakers/{wineMakerId}", wineMakerId),
                    error => error.ToResponse());
            })
            .RequireAuthorization(Policies.IsAdmin)
            .WithOpenApi()
            .WithName("CreateWineMaker")
            .WithSummary("Create wine maker")
            .WithDescription("Creates a new wine maker.")
            .WithTags("WineMakers");
    }
}
