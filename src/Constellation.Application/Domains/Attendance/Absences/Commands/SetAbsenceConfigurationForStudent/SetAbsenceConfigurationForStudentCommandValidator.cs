namespace Constellation.Application.Domains.Attendance.Absences.Commands.SetAbsenceConfigurationForStudent;

using Constellation.Core.Models.Students.Identifiers;
using Core.Models.Identifiers;
using FluentValidation;

internal sealed class SetAbsenceConfigurationForStudentCommandValidator : AbstractValidator<SetAbsenceConfigurationForStudentCommand>
{
    public SetAbsenceConfigurationForStudentCommandValidator()
    {
        RuleFor(command => command.StudentId)
            .NotEmpty()
            .When(command => 
                command.SchoolCode == SchoolCode.Empty &&
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
                command.SchoolCode == SchoolCode.Empty &&
                command.StudentId == StudentId.Empty)
            .WithMessage("You must specify a Student, a Grade, or a School");

        RuleFor(command => command.StartDate)
            .NotEmpty()
            .LessThanOrEqualTo(command => command.EndDate.Value)
            .When(command => command.EndDate.HasValue)
            .WithMessage("The Start Date must be before the End Date");
    }
}
