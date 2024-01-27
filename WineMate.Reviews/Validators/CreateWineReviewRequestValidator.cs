using FluentValidation;

using WineMate.Contracts.Api;

namespace WineMate.Reviews.Validators;

public class CreateWineReviewRequestValidator : AbstractValidator<CreateWineReviewRequest>
{
    public CreateWineReviewRequestValidator()
    {
        RuleFor(x => x.WineId)
            .NotEmpty();

        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(128);

        RuleFor(x => x.Body)
            .NotEmpty();

        RuleFor(x => x.Rating)
            .NotEmpty()
            .InclusiveBetween(0, 10);
    }
}
