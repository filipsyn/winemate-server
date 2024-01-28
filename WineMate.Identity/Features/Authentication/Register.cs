using Carter;

using ErrorOr;

using FluentValidation;

using Mapster;

using MediatR;

using Microsoft.EntityFrameworkCore;

using WineMate.Common.Extensions;
using WineMate.Contracts.Api;
using WineMate.Identity.Configuration;
using WineMate.Identity.Database;
using WineMate.Identity.Database.Entities;

namespace WineMate.Identity.Features.Authentication;

public static class Register
{
    public class Command : IRequest<ErrorOr<RegisterResponse>>
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Email)
                .NotEmpty()
                .EmailAddress();

            RuleFor(x => x.Password)
                .NotEmpty()
                .MinimumLength(Constants.MinimumPasswordLength);
        }
    }

    internal sealed class Handler : IRequestHandler<Command, ErrorOr<RegisterResponse>>
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ILogger<Handler> _logger;
        private readonly IValidator<Command> _validator;

        public Handler(ApplicationDbContext dbContext, ILogger<Handler> logger, IValidator<Command> validator)
        {
            _dbContext = dbContext;
            _logger = logger;
            _validator = validator;
        }

        public async Task<ErrorOr<RegisterResponse>> Handle(Command request, CancellationToken cancellationToken)
        {
            var validationResult = await _validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                _logger.LogWarning("Cant create request {@Request}; validation failed {ValidationResult}",
                    request,
                    validationResult);
                return Error.Validation(nameof(Register), validationResult.ToString() ?? "Validation failed");
            }

            if (await IsEmailTaken(request.Email, cancellationToken))
            {
                _logger.LogWarning("Cant register user with email {Email}; email already exists", request.Email);
                return Error.Conflict(nameof(Register), "Email is already taken");
            }

            var user = new User
            {
                Email = request.Email,
                Password = BCrypt.Net.BCrypt.HashPassword(request.Password)
            };

            await _dbContext.Users.AddAsync(user, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return new RegisterResponse { Id = user.Id };
        }

        private async Task<bool> IsEmailTaken(string email, CancellationToken cancellationToken)
        {
            return await _dbContext.Users.AnyAsync(user => user.Email == email, cancellationToken);
        }
    }
}

public class RegisterEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/register", async (
                RegisterRequest request,
                IValidator<RegisterRequest> validator,
                ISender sender) =>
            {
                var validationResult = await validator.ValidateAsync(request);
                if (!validationResult.IsValid)
                {
                    return Results.ValidationProblem(validationResult.ToDictionary());
                }

                var command = request.Adapt<Register.Command>();
                var result = await sender.Send(command);

                return result.Match(
                    response => Results.Created(null as string, response),
                    errors => errors.ToResponse());
            })
            .WithOpenApi()
            .WithName("Register")
            .WithSummary("Register new user")
            .WithDescription("Registers new user with given email and password.")
            .WithTags("Authentication");
    }
}
