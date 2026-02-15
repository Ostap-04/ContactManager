using ContactManager.Models.Dtos;
using FluentValidation;

namespace ContactManager.Validation;

public sealed class UpdateContactRequestValidator : AbstractValidator<UpdateContactRequest>
{
    public UpdateContactRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MinimumLength(2)
            .MaximumLength(200);

        RuleFor(x => x.DateOfBirth)
            .NotEmpty()
            .Must(d => d <= DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("Date of birth cannot be in the future.")
            .Must(d => d >= new DateOnly(1900, 1, 1))
            .WithMessage("Date of birth is too old.");

        RuleFor(x => x.Phone)
            .NotEmpty()
            .MaximumLength(50)
            .Matches(@"^\+?[0-9\s\-\(\)]{7,50}$")
            .WithMessage("Phone number format is invalid.");

        RuleFor(x => x.Salary)
            .GreaterThanOrEqualTo(0)
            .LessThanOrEqualTo(1_000_000_000);
    }
}