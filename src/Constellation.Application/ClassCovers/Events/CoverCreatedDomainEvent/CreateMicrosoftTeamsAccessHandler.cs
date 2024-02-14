namespace Constellation.Application.ClassCovers.Events.CoverCreatedDomainEvent;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Models.Auth;
using Constellation.Application.Models.Identity;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.DomainEvents;
using Constellation.Core.Enums;
using Constellation.Core.Models;
using Constellation.Core.ValueObjects;
using Core.Models.Covers;
using Microsoft.AspNetCore.Identity;
using Serilog;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

internal sealed class CreateMicrosoftTeamsAccessHandler
    : IDomainEventHandler<CoverCreatedDomainEvent>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IClassCoverRepository _classCoverRepository;
    private readonly IMSTeamOperationsRepository _operationsRepository;
    private readonly UserManager<AppUser> _userManager;
    private readonly ILogger _logger;

    public CreateMicrosoftTeamsAccessHandler(
        IUnitOfWork unitOfWork,
        IClassCoverRepository classCoverRepository,
        IMSTeamOperationsRepository operationsRepository,
        UserManager<AppUser> userManager,
        ILogger logger)
    {
        _unitOfWork = unitOfWork;
        _classCoverRepository = classCoverRepository;
        _operationsRepository = operationsRepository;
        _userManager = userManager;
        _logger = logger.ForContext<CoverCreatedDomainEvent>();
    }

    public async Task Handle(CoverCreatedDomainEvent notification, CancellationToken cancellationToken)
    {
        ClassCover cover = await _classCoverRepository.GetById(notification.CoverId, cancellationToken);

        if (cover is null)
        {
            _logger.Error("{action}: Could not find cover with Id {id} in database", nameof(CreateMicrosoftTeamsAccessHandler), notification.CoverId);

            return;
        }

        if (cover.TeacherType == CoverTeacherType.Casual)
        {
            CasualMSTeamOperation addOperation = new()
            {
                OfferingId = cover.OfferingId,
                CasualId = Guid.Parse(cover.TeacherId),
                CoverId = cover.Id.Value,
                Action = MSTeamOperationAction.Add,
                PermissionLevel = MSTeamOperationPermissionLevel.Owner,
                DateScheduled = cover.StartDate.ToDateTime(TimeOnly.MinValue).AddDays(-1)
            };

            _operationsRepository.Insert(addOperation);

            CasualMSTeamOperation removeOperation = new()
            {
                OfferingId = cover.OfferingId,
                CasualId = Guid.Parse(cover.TeacherId),
                CoverId = cover.Id.Value,
                Action = MSTeamOperationAction.Remove,
                PermissionLevel = MSTeamOperationPermissionLevel.Owner,
                DateScheduled = cover.EndDate.ToDateTime(TimeOnly.MinValue).AddDays(1)
            };

            _operationsRepository.Insert(removeOperation);
        }
        else
        {
            TeacherMSTeamOperation addOperation = new()
            {
                OfferingId = cover.OfferingId,
                StaffId = cover.TeacherId,
                CoverId = cover.Id.Value,
                Action = MSTeamOperationAction.Add,
                PermissionLevel = MSTeamOperationPermissionLevel.Owner,
                DateScheduled = cover.StartDate.ToDateTime(TimeOnly.MinValue).AddDays(-1)
            };

            _operationsRepository.Insert(addOperation);

            TeacherMSTeamOperation removeOperation = new()
            {
                OfferingId = cover.OfferingId,
                StaffId = cover.TeacherId,
                CoverId = cover.Id.Value,
                Action = MSTeamOperationAction.Remove,
                PermissionLevel = MSTeamOperationPermissionLevel.Owner,
                DateScheduled = cover.EndDate.ToDateTime(TimeOnly.MinValue).AddDays(1)
            };

            _operationsRepository.Insert(removeOperation);
        }

        // Cover administrators
        IList<AppUser> additionalRecipients = await _userManager.GetUsersInRoleAsync(AuthRoles.CoverRecipient);

        foreach (AppUser coverAdmin in additionalRecipients)
        {
            if (!coverAdmin.IsStaffMember)
                continue;

            TeacherMSTeamOperation addOperation = new()
            {
                OfferingId = cover.OfferingId,
                StaffId = coverAdmin.StaffId,
                Action = MSTeamOperationAction.Add,
                PermissionLevel = MSTeamOperationPermissionLevel.Owner,
                DateScheduled = cover.StartDate.ToDateTime(TimeOnly.MinValue).AddDays(-1),
                CoverId = cover.Id.Value
            };

            _operationsRepository.Insert(addOperation);

            TeacherMSTeamOperation removeOperation = new()
            {
                OfferingId = cover.OfferingId,
                StaffId = coverAdmin.StaffId,
                Action = MSTeamOperationAction.Remove,
                PermissionLevel = MSTeamOperationPermissionLevel.Owner,
                DateScheduled = cover.EndDate.ToDateTime(TimeOnly.MinValue).AddDays(1),
                CoverId = cover.Id.Value
            };

            _operationsRepository.Insert(removeOperation);
        }

        await _unitOfWork.CompleteAsync(cancellationToken);
    }
}
