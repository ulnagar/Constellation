namespace Constellation.Application.Domains.Tutorials.GroupTutorials.Commands.AddTeacherToTutorial;

using FluentValidation;

public sealed class AddTeacherToTutorialCommandValidator : AbstractValidator<AddTeacherToTutorialCommand>
{
	public AddTeacherToTutorialCommandValidator()
	{
		RuleFor(command => command.StaffId).NotEmpty();
	}
}
