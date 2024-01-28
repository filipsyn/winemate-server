using FluentValidation;

using WineMate.Contracts.Api;
using WineMate.Identity.Configuration;

namespace WineMate.Identity.Validators;

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty().MinimumLength(Constants.MinimumPasswordLength);
    }
}
