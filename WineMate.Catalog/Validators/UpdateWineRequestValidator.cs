using FluentValidation;

using WineMate.Catalog.Contracts;

namespace WineMate.Catalog.Validators;

public class UpdateWineRequestValidator : AbstractValidator<UpdateWineRequest>
{
    public UpdateWineRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty();

        RuleFor(x => x.Description)
            .NotEqual(string.Empty)
            .When(x => x.Description != null);

        RuleFor(x => x.Year)
            .NotEmpty()
            .InclusiveBetween(1800, DateTime.UtcNow.Year);

        RuleFor(x => x.Type)
            .NotEmpty()
            .IsInEnum();
    }
}
