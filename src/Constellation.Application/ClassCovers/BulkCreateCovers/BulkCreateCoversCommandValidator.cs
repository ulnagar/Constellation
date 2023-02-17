namespace Constellation.Application.ClassCovers.BulkCreateCovers;

using Constellation.Application.Extensions;
using Constellation.Application.Interfaces.Repositories;
using FluentValidation;
using System;
using System.Threading;
using System.Threading.Tasks;

public class BulkCreateCoversCommandValidator : AbstractValidator<BulkCreateCoversCommand>
{
    private readonly ICourseOfferingRepository _offeringRepository;

    public BulkCreateCoversCommandValidator(ICourseOfferingRepository offeringRepository)
    {
        _offeringRepository = offeringRepository;

        RuleForEach(command => command.OfferingId)
            .NotEmpty()
            .MustAsync(BeCurrentOffering)
                .WithMessage("Cannot create a cover for an expired class!");

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

    private async Task<bool> BeCurrentOffering(int OfferingId, CancellationToken cancellationToken)
    {
        var offering = await _offeringRepository.GetForExistCheck(OfferingId);

        if (offering is null)
            return false;

        if (offering.IsCurrent())
            return true;

        return false;
    }
}
