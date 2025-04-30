namespace Constellation.Application.Domains.Attendance.Absences.Commands.CreateAbsenceResponseFromSchool;

using FluentValidation;

internal sealed class CreateAbsenceResponseFromSchoolCommandValidator : AbstractValidator<CreateAbsenceResponseFromSchoolCommand>
{
    public CreateAbsenceResponseFromSchoolCommandValidator()
    {
        RuleFor(command => command.AbsenceId)
            .NotNull();

        RuleFor(command => command.Comment)
            .NotNull()
            .MinimumLength(5)
            .WithMessage("You must provide a longer explanation for this absence.");

        RuleFor(command => command.UserEmail)
            .NotNull();
    }
}