namespace Constellation.Application.Absences.ProvideParentWholeAbsenceExplanation;

using FluentValidation;

public sealed class ProvideParentWholeAbsenceExplanationCommandValidator : AbstractValidator<ProvideParentWholeAbsenceExplanationCommand>
{
    public ProvideParentWholeAbsenceExplanationCommandValidator()
    {
        RuleFor(command => command.Comment)
            .NotEmpty()
            .MinimumLength(5)
            .WithMessage("You must include a longer comment");
    }
}