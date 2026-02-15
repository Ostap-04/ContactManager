using ContactManager.Common.Constants;
using ContactManager.Models.Dtos;
using FluentValidation;

namespace ContactManager.Validation;

public sealed class DataTableRequestValidator : AbstractValidator<DataTableRequest>
{
    public DataTableRequestValidator()
    {
        RuleFor(x => x.Draw)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.Start)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.Length)
            .GreaterThan(0)
            .LessThanOrEqualTo(200);

        RuleForEach(x => x.Order)
            .SetValidator(new DataTableOrderValidator());

        RuleForEach(x => x.Columns)
            .SetValidator(new DataTableColumnValidator());

        RuleFor(x => x)
            .Must(req => req.Order.All(o => o.Column >= 0 && o.Column < req.Columns.Count))
            .WithMessage("Order column index is out of range.");
    }
}

public sealed class DataTableOrderValidator : AbstractValidator<DataTableOrder>
{
    public DataTableOrderValidator()
    {
        RuleFor(x => x.Column)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.Direction)
            .NotEmpty()
            .Must(dir => dir is "asc" or "desc")
            .WithMessage("Sort direction must be 'asc' or 'desc'.");
    }
}

public sealed class DataTableColumnValidator : AbstractValidator<DataTableColumn>
{
    public DataTableColumnValidator()
    {
        RuleFor(x => x.Data)
            .MaximumLength(64)
            .When(x => x.Data is not null);

        RuleFor(x => x.Name)
            .MaximumLength(64)
            .When(x => x.Name is not null);
    }
}