using FluentValidation;

using WineMate.Catalog.Database.Entities;

namespace WineMate.Catalog.Validators;

public class AddressValidator : AbstractValidator<Address>
{
    public AddressValidator()
    {
        RuleFor(address => address.Number)
            .NotEmpty()
            .GreaterThan(0);

        RuleFor(address => address.City)
            .NotEmpty();

        RuleFor(address => address.Country)
            .NotEmpty();
    }
}
