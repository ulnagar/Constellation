namespace Constellation.Application.Absences.CreateAbsenceResponseFromSchool;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models.Identifiers;
using FluentValidation;

public sealed record CreateAbsenceResponseFromSchoolCommand(
    AbsenceId AbsenceId,
    string Comment,
    string UserEmail)
    : ICommand;

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