namespace Constellation.Application.GroupTutorials.EditGroupTutorial;

using FluentValidation;
using System;

public sealed class EditGroupTutorialCommandValidator : AbstractValidator<EditGroupTutorialCommand>
{
	public EditGroupTutorialCommandValidator()
	{
        RuleFor(command => command.Id)
            .NotEmpty();

		RuleFor(command => command.Name)
			.NotEmpty();

        RuleFor(command => command.StartDate)
                    .NotEmpty()
                    .LessThanOrEqualTo(command => command.EndDate)
                        .WithMessage("Start Date must be before End Date!");

        RuleFor(command => command.EndDate)
            .NotEmpty()
            .GreaterThanOrEqualTo(command => command.StartDate)
                .WithMessage("End Date must be after Start Date!")
            .GreaterThanOrEqualTo(DateOnly.FromDateTime(DateTime.Today))
                .WithMessage("Cannot set a tutorial End Date to be in the past!");
    }
}
