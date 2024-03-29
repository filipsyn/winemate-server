using FluentValidation;

using WineMate.Contracts.Api;

namespace WineMate.Catalog.Validators;

public class UpdateWineMakerRequestValidator : AbstractValidator<UpdateWineMakerRequest>
{
    public UpdateWineMakerRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty();

        RuleFor(x => x.Address)
            .NotEmpty()
            .SetValidator(new AddressValidator());
    }
}
