namespace Constellation.Application.Absences.SetAbsenceConfigurationForStudent;

using Core.Models.Students.Identifiers;
using FluentValidation;

internal sealed class SetAbsenceConfigurationForStudentCommandValidator : AbstractValidator<SetAbsenceConfigurationForStudentCommand>
{
    public SetAbsenceConfigurationForStudentCommandValidator()
    {
        RuleFor(command => command.StudentId)
            .NotEmpty()
            .When(command => 
                string.IsNullOrWhiteSpace(command.SchoolCode) &&
                !command.GradeFilter.HasValue)
            .WithMessage("You must specify a Student, a Grade, or a School");

        RuleFor(command => command.SchoolCode)
            .NotEmpty()
            .When(command => 
                command.StudentId == StudentId.Empty &&
                !command.GradeFilter.HasValue)
            .WithMessage("You must specify a Student, a Grade, or a School");

        RuleFor(command => command.GradeFilter)
            .NotNull()
            .When(command =>
                string.IsNullOrWhiteSpace(command.SchoolCode) &&
                command.StudentId == StudentId.Empty)
            .WithMessage("You must specify a Student, a Grade, or a School");

        RuleFor(command => command.StartDate)
            .NotEmpty()
            .LessThanOrEqualTo(command => command.EndDate.Value)
            .When(command => command.EndDate.HasValue)
            .WithMessage("The Start Date must be before the End Date");
    }
}
