namespace Constellation.Application.Absences.SetAbsenceConfigurationForStudent;

using Core.Models.Students.Identifiers;
using FluentValidation;

internal sealed class SetAbsenceConfigurationForStudentCommandValidator : AbstractValidator<SetAbsenceConfigurationForStudentCommand>
{
    public SetAbsenceConfigurationForStudentCommandValidator()
    {
        RuleFor(command => command.StudentId)
            .NotEmpty()
            .When(command => string.IsNullOrWhiteSpace(command.SchoolCode))
            .WithMessage("You must specify either a Student or a School");

        RuleFor(command => command.SchoolCode)
            .NotEmpty()
            .When(command => command.StudentId == StudentId.Empty)
            .WithMessage("You must specify either a Student or a School");

        RuleFor(command => command.StartDate)
            .NotEmpty()
            .LessThanOrEqualTo(command => command.EndDate.Value)
            .When(command => command.EndDate.HasValue)
            .WithMessage("The Start Date must be before the End Date");
    }
}
