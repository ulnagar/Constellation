﻿namespace Constellation.Application.GroupTutorials.CreateRoll;

using FluentValidation;
using System;

internal class CreateRollCommandValidator : AbstractValidator<CreateRollCommand>
{
	public CreateRollCommandValidator()
	{
		RuleFor(x => x.RollDate).Must(x => x >= DateOnly.FromDateTime(DateTime.Today));
	}
}
