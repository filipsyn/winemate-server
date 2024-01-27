using FluentValidation;

using WineMate.Contracts.Api;

namespace WineMate.Catalog.Validators;

public class CreateWineMakerRequestValidator : AbstractValidator<CreateWineMakerRequest>
{
    public CreateWineMakerRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty();

        RuleFor(x => x.Address)
            .NotEmpty()
            .SetValidator(new AddressValidator());
    }
}
