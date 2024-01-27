using FluentValidation;

using WineMate.Contracts.Api;
using WineMate.Reviews.Configuration;

namespace WineMate.Reviews.Validators;

public class UpdateWineReviewRequestValidator : AbstractValidator<UpdateWineReviewRequest>
{
    public UpdateWineReviewRequestValidator()
    {
        RuleFor(x => x.WineId)
            .NotEmpty();

        RuleFor(x => x.Title)
            .MaximumLength(128);

        RuleFor(x => x.Rating)
            .NotEmpty()
            .InclusiveBetween(Constants.MinimumRating, Constants.MaximumRating);
    }
}
