﻿namespace Constellation.Application.Families.events;

using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions;
using Constellation.Core.DomainEvents;
using Constellation.Core.Models.Families;
using MediatR;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

internal sealed class StudentRemovedFromFamilyDomainEvent_EnsureFamilyHasActiveStudents
    : INotificationHandler<StudentRemovedFromFamilyDomainEvent>
{
    private readonly IFamilyRepository _familyRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public StudentRemovedFromFamilyDomainEvent_EnsureFamilyHasActiveStudents(
        IFamilyRepository familyRepository,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _familyRepository = familyRepository;
        _unitOfWork = unitOfWork;
        _logger = logger.ForContext<StudentRemovedFromFamilyDomainEvent>();
    }

    public async Task Handle(StudentRemovedFromFamilyDomainEvent notification, CancellationToken cancellationToken)
    {
        Family family = await _familyRepository.GetFamilyById(notification.Membership.FamilyId, cancellationToken);

        if (family is null)
        {
            _logger.Warning("Could not find family with Id {familyId} to ensure active students present", notification.Id);

            return;
        }

        if (family.Students.Count == 0)
        {
            _logger.Information("Family with Id {familyId} has no remaining students linked. Set to deleted.", family.Id);

            family.Delete();

            await _unitOfWork.CompleteAsync(cancellationToken);
        }
    }
}