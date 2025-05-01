namespace Constellation.Application.Domains.GroupTutorials.Commands.CreateGroupTutorial;

using FluentValidation;
using System;

public class CreateGroupTutorialCommandValidator : AbstractValidator<CreateGroupTutorialCommand>
{
	public CreateGroupTutorialCommandValidator()
	{
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
				.WithMessage("Cannot create a tutorial that has already ended!");
	}
}
