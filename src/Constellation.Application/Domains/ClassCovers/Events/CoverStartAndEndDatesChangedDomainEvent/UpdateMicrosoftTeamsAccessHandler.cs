namespace Constellation.Application.Domains.ClassCovers.Events.CoverStartAndEndDatesChangedDomainEvent;

using Abstractions.Messaging;
using Application.Models.Auth;
using Application.Models.Identity;
using Core.Abstractions.Repositories;
using Core.DomainEvents;
using Core.Enums;
using Core.Models;
using Core.Models.Covers;
using Core.ValueObjects;
using Interfaces.Repositories;
using Microsoft.AspNetCore.Identity;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class UpdateMicrosoftTeamsAccessHandler
    : IDomainEventHandler<CoverStartAndEndDatesChangedDomainEvent>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMSTeamOperationsRepository _operationsRepository;
    private readonly IClassCoverRepository _classCoverRepository;
    private readonly UserManager<AppUser> _userManager;
    private readonly ILogger _logger;

    public UpdateMicrosoftTeamsAccessHandler(
        IUnitOfWork unitOfWork,
        IMSTeamOperationsRepository operationsRepository,
        IClassCoverRepository classCoverRepository,
        UserManager<AppUser> userManager,
        ILogger logger)
    {
        _unitOfWork = unitOfWork;
        _operationsRepository = operationsRepository;
        _classCoverRepository = classCoverRepository;
        _userManager = userManager;
        _logger = logger.ForContext<CoverStartAndEndDatesChangedDomainEvent>();
    }

    public async Task Handle(CoverStartAndEndDatesChangedDomainEvent notification, CancellationToken cancellationToken)
    {
        ClassCover cover = await _classCoverRepository.GetById(notification.CoverId, cancellationToken);

        if (cover is null)
        {
            _logger.Warning("{action}: Could not find cover with Id {id} in database", nameof(Events.CoverEndDateChangedDomainEvent.UpdateMicrosoftTeamsAccessHandler), notification.CoverId);

            return;
        }

        // Cover administrators
        IList<AppUser> additionalRecipients = await _userManager.GetUsersInRoleAsync(AuthRoles.CoverRecipient);

        List<MSTeamOperation> existingRequests = await _operationsRepository
            .GetByCoverId(notification.CoverId, cancellationToken);

        if (existingRequests is null)
        {
            _logger.Warning("{action}: Could not find operations for cover with Id {id} in database", nameof(Events.CoverEndDateChangedDomainEvent.UpdateMicrosoftTeamsAccessHandler), notification.CoverId);

            return;
        }

        List<MSTeamOperation> coveringTeacherRequests;

        if (cover.TeacherType == CoverTeacherType.Casual)
        {
            coveringTeacherRequests = existingRequests
                .OfType<CasualMSTeamOperation>()
                .Where(operation => operation.CasualId == Guid.Parse(cover.TeacherId))
                .ToList<MSTeamOperation>();
        }
        else
        {
            coveringTeacherRequests = existingRequests
                .OfType<TeacherMSTeamOperation>()
                .Where(operation => operation.StaffId == cover.TeacherId)
                .ToList<MSTeamOperation>();
        }

        List<MSTeamOperation> addRequests = coveringTeacherRequests
            .Where(operation =>
                operation.Action == MSTeamOperationAction.Add)
            .ToList();

        List<MSTeamOperation> removeRequests = coveringTeacherRequests
            .Where(operation =>
                operation.Action == MSTeamOperationAction.Remove)
            .ToList();

        // If access has not been granted, change the operation
        // If it has, determine whether there is a big enough difference to remove access and then grant later or just leave it
        MSTeamOperation alreadyGranted = addRequests.FirstOrDefault(operation => operation.IsCompleted);

        if (alreadyGranted is null)
        {
            DateTime newActionDate = notification.NewStartDate.ToDateTime(TimeOnly.MinValue).AddDays(-1);

            foreach (MSTeamOperation request in addRequests)
            {
                request.DateScheduled = newActionDate;
            }

            foreach (AppUser coverAdmin in additionalRecipients)
            {
                if (!coverAdmin.IsStaffMember)
                    continue;

                TeacherMSTeamOperation existingAddOperation = existingRequests
                    .OfType<TeacherMSTeamOperation>()
                    .FirstOrDefault(operation =>
                        operation.StaffId == coverAdmin.StaffId &&
                        operation.Action == MSTeamOperationAction.Add);

                if (existingAddOperation is not null)
                    existingAddOperation.DateScheduled = newActionDate;
            }
        }
        else
        {
            DateTime previousActionDate = alreadyGranted.DateScheduled;
            DateTime newActionDate = notification.NewStartDate.ToDateTime(TimeOnly.MinValue).AddDays(-1);

            if (newActionDate.Subtract(previousActionDate) > TimeSpan.FromDays(2))
            {
                alreadyGranted.Delete();

                // Remove access, then create new add operation
                if (cover.TeacherType == CoverTeacherType.Casual)
                {
                    CasualMSTeamOperation removeEarlyOperation = new()
                    {
                        OfferingId = cover.OfferingId,
                        CasualId = Guid.Parse(cover.TeacherId),
                        CoverId = Guid.Empty,
                        Action = MSTeamOperationAction.Remove,
                        PermissionLevel = MSTeamOperationPermissionLevel.Owner,
                        DateScheduled = DateTime.Today
                    };

                    _operationsRepository.Insert(removeEarlyOperation);

                    CasualMSTeamOperation addTimelyOperation = new()
                    {
                        OfferingId = cover.OfferingId,
                        CasualId = Guid.Parse(cover.TeacherId),
                        CoverId = cover.Id.Value,
                        Action = MSTeamOperationAction.Add,
                        PermissionLevel = MSTeamOperationPermissionLevel.Owner,
                        DateScheduled = newActionDate
                    };

                    _operationsRepository.Insert(addTimelyOperation);
                }
                else
                {
                    TeacherMSTeamOperation removeEarlyOperation = new()
                    {
                        OfferingId = cover.OfferingId,
                        StaffId = cover.TeacherId,
                        CoverId = Guid.Empty,
                        Action = MSTeamOperationAction.Remove,
                        PermissionLevel = MSTeamOperationPermissionLevel.Owner,
                        DateScheduled = DateTime.Today
                    };

                    _operationsRepository.Insert(removeEarlyOperation);

                    TeacherMSTeamOperation addTimelyOperation = new()
                    {
                        OfferingId = cover.OfferingId,
                        StaffId = cover.TeacherId,
                        CoverId = cover.Id.Value,
                        Action = MSTeamOperationAction.Add,
                        PermissionLevel = MSTeamOperationPermissionLevel.Owner,
                        DateScheduled = newActionDate
                    };

                    _operationsRepository.Insert(addTimelyOperation);
                }

                foreach (AppUser coverAdmin in additionalRecipients)
                {
                    if (!coverAdmin.IsStaffMember)
                        continue;

                    TeacherMSTeamOperation existingAddOperation = existingRequests
                        .OfType<TeacherMSTeamOperation>()
                        .FirstOrDefault(operation =>
                            operation.StaffId == coverAdmin.StaffId &&
                            operation.Action == MSTeamOperationAction.Add &&
                            operation.DateScheduled == alreadyGranted.DateScheduled);

                    if (existingAddOperation is not null)
                    {
                        existingAddOperation.Delete();

                        TeacherMSTeamOperation removeOperation = new()
                        {
                            OfferingId = cover.OfferingId,
                            StaffId = coverAdmin.StaffId,
                            Action = MSTeamOperationAction.Remove,
                            PermissionLevel = MSTeamOperationPermissionLevel.Owner,
                            DateScheduled = DateTime.Today,
                            CoverId = Guid.Empty
                        };

                        _operationsRepository.Insert(removeOperation);
                    }

                    TeacherMSTeamOperation addOperation = new()
                    {
                        OfferingId = cover.OfferingId,
                        StaffId = coverAdmin.StaffId,
                        Action = MSTeamOperationAction.Add,
                        PermissionLevel = MSTeamOperationPermissionLevel.Owner,
                        DateScheduled = newActionDate,
                        CoverId = cover.Id.Value
                    };

                    _operationsRepository.Insert(addOperation);
                }
            }
        }

        // Process removal
        MSTeamOperation alreadyRemoved = removeRequests.FirstOrDefault(operation => operation.IsCompleted);

        if (alreadyRemoved is null)
        {
            DateTime newActionDate = notification.NewEndDate.ToDateTime(TimeOnly.MinValue).AddDays(1);

            foreach (MSTeamOperation request in removeRequests)
            {
                request.DateScheduled = newActionDate;
            }

            foreach (AppUser coverAdmin in additionalRecipients)
            {
                if (!coverAdmin.IsStaffMember)
                    continue;

                TeacherMSTeamOperation existingRemoveOperation = existingRequests
                    .OfType<TeacherMSTeamOperation>()
                    .FirstOrDefault(operation =>
                        operation.StaffId == coverAdmin.StaffId &&
                        operation.Action == MSTeamOperationAction.Remove);

                if (existingRemoveOperation is not null)
                    existingRemoveOperation.DateScheduled = newActionDate;
            }
        }
        else
        {
            DateTime previousActionDate = alreadyRemoved.DateScheduled;
            DateTime newActionDate = notification.NewEndDate.ToDateTime(TimeOnly.MinValue).AddDays(1);

            if (newActionDate > previousActionDate)
            {
                alreadyRemoved.Delete();

                // Re-Add access, then create new removal operations
                if (cover.TeacherType == CoverTeacherType.Casual)
                {
                    CasualMSTeamOperation reAddOperation = new()
                    {
                        OfferingId = cover.OfferingId,
                        CasualId = Guid.Parse(cover.TeacherId),
                        CoverId = Guid.Empty,
                        Action = MSTeamOperationAction.Add,
                        PermissionLevel = MSTeamOperationPermissionLevel.Owner,
                        DateScheduled = DateTime.Now
                    };

                    _operationsRepository.Insert(reAddOperation);

                    CasualMSTeamOperation removeTimelyOperation = new()
                    {
                        OfferingId = cover.OfferingId,
                        CasualId = Guid.Parse(cover.TeacherId),
                        CoverId = Guid.Empty,
                        Action = MSTeamOperationAction.Remove,
                        PermissionLevel = MSTeamOperationPermissionLevel.Owner,
                        DateScheduled = newActionDate
                    };

                    _operationsRepository.Insert(removeTimelyOperation);

                }
                else
                {
                    TeacherMSTeamOperation reAddOperation = new()
                    {
                        OfferingId = cover.OfferingId,
                        StaffId = cover.TeacherId,
                        CoverId = Guid.Empty,
                        Action = MSTeamOperationAction.Add,
                        PermissionLevel = MSTeamOperationPermissionLevel.Owner,
                        DateScheduled = DateTime.Now
                    };

                    _operationsRepository.Insert(reAddOperation);

                    TeacherMSTeamOperation removeTimelyOperation = new()
                    {
                        OfferingId = cover.OfferingId,
                        StaffId = cover.TeacherId,
                        CoverId = Guid.Empty,
                        Action = MSTeamOperationAction.Remove,
                        PermissionLevel = MSTeamOperationPermissionLevel.Owner,
                        DateScheduled = newActionDate
                    };

                    _operationsRepository.Insert(removeTimelyOperation);
                }

                foreach (AppUser coverAdmin in additionalRecipients)
                {
                    if (!coverAdmin.IsStaffMember)
                        continue;

                    TeacherMSTeamOperation existingRemoveOperation = existingRequests
                        .OfType<TeacherMSTeamOperation>()
                        .FirstOrDefault(operation =>
                            operation.StaffId == coverAdmin.StaffId &&
                            operation.Action == MSTeamOperationAction.Remove &&
                            operation.DateScheduled == alreadyRemoved.DateScheduled);

                    if (existingRemoveOperation is not null)
                    {
                        existingRemoveOperation.Delete();

                        TeacherMSTeamOperation addOperation = new()
                        {
                            OfferingId = cover.OfferingId,
                            StaffId = coverAdmin.StaffId,
                            Action = MSTeamOperationAction.Add,
                            PermissionLevel = MSTeamOperationPermissionLevel.Owner,
                            DateScheduled = DateTime.Now,
                            CoverId = Guid.Empty
                        };

                        _operationsRepository.Insert(addOperation);
                    }

                    TeacherMSTeamOperation removeOperation = new()
                    {
                        OfferingId = cover.OfferingId,
                        StaffId = coverAdmin.StaffId,
                        Action = MSTeamOperationAction.Remove,
                        PermissionLevel = MSTeamOperationPermissionLevel.Owner,
                        DateScheduled = newActionDate,
                        CoverId = cover.Id.Value
                    };

                    _operationsRepository.Insert(removeOperation);
                }
            }
        }

        await _unitOfWork.CompleteAsync(cancellationToken);
    }
}
