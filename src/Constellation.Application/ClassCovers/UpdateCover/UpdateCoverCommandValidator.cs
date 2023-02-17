namespace Constellation.Application.ClassCovers.BulkCreateCovers;

using Constellation.Application.ClassCovers.UpdateCover;
using FluentValidation;
using System;

public class UpdateCoverCommandValidator : AbstractValidator<UpdateCoverCommand>
{
    public UpdateCoverCommandValidator()
    {
        RuleFor(command => command.StartDate)
            .NotEmpty()
            .LessThanOrEqualTo(command => command.EndDate)
                .WithMessage("Start Date must be before End Date!");

        RuleFor(command => command.EndDate)
            .NotEmpty()
            .GreaterThanOrEqualTo(command => command.StartDate)
                .WithMessage("End Date must be after Start Date!")
            .GreaterThanOrEqualTo(DateOnly.FromDateTime(DateTime.Today))
                .WithMessage("Cannot create a cover that has already ended!");
    }
}
