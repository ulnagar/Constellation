namespace Constellation.Application.Absences.SetAbsenceConfigurationForStudent;

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
            .When(command => string.IsNullOrWhiteSpace(command.StudentId))
            .WithMessage("You must specify either a Student or a School");
    }
}
