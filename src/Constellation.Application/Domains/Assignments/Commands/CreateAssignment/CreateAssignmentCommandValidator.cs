namespace Constellation.Application.Domains.Assignments.Commands.CreateAssignment;

using Core.Abstractions.Clock;
using FluentValidation;
using System;

public class CreateAssignmentCommandValidator : AbstractValidator<CreateAssignmentCommand>
{
    public CreateAssignmentCommandValidator(IDateTimeProvider dateTime)
    {
        RuleFor(command => command.DueDate)
            .GreaterThanOrEqualTo(DateTime.Today)
            .WithMessage($"Due Date must be in the future!");

        RuleFor(command => command.UnlockDate)
            .LessThanOrEqualTo(command => command.DueDate)
            .LessThanOrEqualTo(command => command.LockDate)
            .WithMessage($"Unlock Date must be before the Due Date and the Lock Date!");

        RuleFor(command => command.LockDate)
            .GreaterThanOrEqualTo(command => command.UnlockDate)
            .When(command => command.UnlockDate.HasValue)
            .WithMessage($"Lock Date must be after the Unlock Date!");

        RuleFor(command => command.ForwardDate)
            .GreaterThanOrEqualTo(command => dateTime.Today)
            .When(command => command.DelayForwarding == true)
            .WithMessage($"Forward Date must be in the future");

        RuleFor(command => command.ForwardDate)
            .LessThan(command => DateOnly.FromDateTime(command.LockDate.Value))
            .When(command =>
                command.DelayForwarding == true &&
                command.LockDate.HasValue)
            .WithMessage($"Forwarding Date must be before the Lock Date");
    }
}