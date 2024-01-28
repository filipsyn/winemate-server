using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

using Carter;

using ErrorOr;

using FluentValidation;

using Mapster;

using MediatR;

using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

using WineMate.Common.Extensions;
using WineMate.Contracts.Api;
using WineMate.Identity.Database;
using WineMate.Identity.Database.Entities;

namespace WineMate.Identity.Features.Authentication;

public static class Login
{
    public class Command : IRequest<ErrorOr<UserLoginResponse>>
    {
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Email).NotEmpty().EmailAddress();
            RuleFor(x => x.Password).NotEmpty();
        }
    }

    internal sealed class Handler : IRequestHandler<Command, ErrorOr<UserLoginResponse>>
    {
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _dbContext;
        private readonly ILogger<Handler> _logger;
        private readonly IValidator<Command> _validator;

        public Handler(
            ApplicationDbContext dbContext,
            ILogger<Handler> logger,
            IValidator<Command> validator,
            IConfiguration configuration)
        {
            _dbContext = dbContext;
            _logger = logger;
            _validator = validator;
            _configuration = configuration;
        }

        public async Task<ErrorOr<UserLoginResponse>> Handle(Command request, CancellationToken cancellationToken)
        {
            var validationResult = await _validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                _logger.LogWarning("Can't create {Request}; validation failed {ValidationResult}",
                    request,
                    validationResult);
                return Error.Validation(nameof(Handler), validationResult.ToString() ?? "Validation failed");
            }

            var user = await _dbContext.Users.FirstOrDefaultAsync(user => user.Email == request.Email,
                cancellationToken);

            if (user is null)
            {
                _logger.LogWarning("User with email {Email} not found; Login failed", request.Email);
                return Error.Failure(nameof(Handler), "Incorrect credentials");
            }

            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.Password))
            {
                _logger.LogWarning("User with email {Email} provided incorrect password; Login failed",
                    request.Email);
                return Error.Failure(nameof(Handler), "Incorrect credentials");
            }

            user.LastLoginAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync(cancellationToken);

            var token = GenerateJwtToken(user);

            return new UserLoginResponse { Token = token };
        }

        private string GenerateJwtToken(User user)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id.ToString())
            };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration.GetSection("JwtSettings:Key").Value
                                       ?? throw new InvalidOperationException("Key is not configured"))
            );

            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddHours(1),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}

public class LoginEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/login", async (
                UserLoginRequest request,
                IValidator<UserLoginRequest> validator,
                ISender sender) =>
            {
                var validationResult = await validator.ValidateAsync(request);
                if (!validationResult.IsValid)
                {
                    return Results.ValidationProblem(validationResult.ToDictionary());
                }

                var command = request.Adapt<Login.Command>();
                var result = await sender.Send(command);

                return result.Match(
                    Results.Ok,
                    errors => errors.ToResponse()
                );
            })
            .WithOpenApi()
            .WithName("Login")
            .WithSummary("Login user")
            .WithDescription("Logs in user with given email and password.")
            .WithTags("Authentication");
    }
}
