namespace Constellation.Application.GroupTutorials.AddStudentToTutorial;

using FluentValidation;

public sealed class AddStudentToTutorialCommandValidator : AbstractValidator<AddStudentToTutorialCommand>
{
	public AddStudentToTutorialCommandValidator()
	{
		RuleFor(command => command.StudentId).NotEmpty();
	}
}
