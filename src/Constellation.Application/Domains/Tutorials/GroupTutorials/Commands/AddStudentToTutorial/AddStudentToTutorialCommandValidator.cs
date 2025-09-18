namespace Constellation.Application.Domains.Tutorials.GroupTutorials.Commands.AddStudentToTutorial;

using FluentValidation;

public sealed class AddStudentToTutorialCommandValidator : AbstractValidator<AddStudentToTutorialCommand>
{
	public AddStudentToTutorialCommandValidator()
	{
		RuleFor(command => command.StudentId).NotEmpty();
	}
}
