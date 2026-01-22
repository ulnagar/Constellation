namespace Constellation.Application.Domains.Covers.Events.CoverEndDateChangedDomainEvent;

using Abstractions.Messaging;
using Constellation.Core.Models.Covers.Events;
using Constellation.Core.Models.Covers.Repositories;
using Constellation.Core.Models.StaffMembers;
using Constellation.Core.Models.StaffMembers.Errors;
using Constellation.Core.Models.StaffMembers.Repositories;
using Constellation.Core.Models.StaffMembers.ValueObjects;
using Constellation.Core.Shared;
using Core.Enums;
using Core.Models;
using Core.Models.Covers;
using Core.Models.Covers.Enums;
using Core.Models.StaffMembers.Identifiers;
using Interfaces.Configuration;
using Interfaces.Repositories;
using Microsoft.Extensions.Options;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class UpdateMicrosoftTeamsAccessHandler
    : IDomainEventHandler<CoverEndDateChangedDomainEvent>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMSTeamOperationsRepository _operationsRepository;
    private readonly ICoverRepository _coverRepository;
    private readonly AppConfiguration _configuration;
    private readonly IStaffRepository _staffRepository;
    private readonly ILogger _logger;

    public UpdateMicrosoftTeamsAccessHandler(
        IUnitOfWork unitOfWork,
        IMSTeamOperationsRepository operationsRepository,
        ICoverRepository coverRepository,
        IOptions<AppConfiguration> configuration,
        IStaffRepository staffRepository,
        ILogger logger)
    {
        _unitOfWork = unitOfWork;
        _operationsRepository = operationsRepository;
        _coverRepository = coverRepository;
        _configuration = configuration.Value;
        _staffRepository = staffRepository;
        _logger = logger.ForContext<CoverEndDateChangedDomainEvent>();
    }

    public async Task Handle(CoverEndDateChangedDomainEvent notification, CancellationToken cancellationToken)
    {
        Cover? cover = await _coverRepository.GetById(notification.CoverId, cancellationToken);

        if (cover is null)
        {
            _logger.Warning("{action}: Could not find cover with Id {id} in database", nameof(UpdateMicrosoftTeamsAccessHandler), notification.CoverId);

            return;
        }

        List<MSTeamOperation> existingRequests = await _operationsRepository
            .GetByCoverId(notification.CoverId, cancellationToken);

        if (existingRequests.Count == 0)
        {
            _logger.Warning("{action}: Could not find operations for cover with Id {id} in database", nameof(UpdateMicrosoftTeamsAccessHandler), notification.CoverId);

            return;
        }

        List<MSTeamOperation> coveringTeacherRequests;

        if (cover.TeacherType == CoverTeacherType.Casual)
        {
            coveringTeacherRequests = existingRequests
                .OfType<CasualMSTeamOperation>()
                .Where(operation => operation.CasualId == Guid.Parse(cover.TeacherId))
                .Where(operation => operation.Action == MSTeamOperationAction.Remove)
                .ToList<MSTeamOperation>();
        }
        else
        {
            coveringTeacherRequests = existingRequests
                .OfType<TeacherMSTeamOperation>()
                .Where(operation => operation.StaffId.ToString() == cover.TeacherId)
                .Where(operation => operation.Action == MSTeamOperationAction.Remove)
                .ToList<MSTeamOperation>();
        }
        
        // Process removal
        MSTeamOperation? alreadyRemoved = coveringTeacherRequests.FirstOrDefault(operation => operation.IsCompleted);

        if (alreadyRemoved is null)
        {
            DateTime newActionDate = notification.NewEndDate.ToDateTime(TimeOnly.MinValue).AddDays(1);

            foreach (MSTeamOperation request in coveringTeacherRequests)
            {
                request.DateScheduled = newActionDate;
            }

            foreach (EmployeeId employeeId in _configuration.Covers.CoverContacts)
            {
                StaffMember? teacher = await _staffRepository.GetByEmployeeId(employeeId, cancellationToken);

                if (teacher is null)
                {
                    _logger
                        .ForContext(nameof(CoverEndDateChangedDomainEvent), notification, true)
                        .ForContext(nameof(Error), StaffMemberErrors.NotFoundByEmployeeId(employeeId), true)
                        .ForContext(nameof(EmployeeId), employeeId)
                        .Warning("Failed to update Cover Teams Access");

                    continue;
                }

                TeacherMSTeamOperation? existingAdminOperation = existingRequests
                    .OfType<TeacherMSTeamOperation>()
                    .FirstOrDefault(operation =>
                        operation.StaffId == teacher.Id &&
                        operation.Action == MSTeamOperationAction.Remove);

                if (existingAdminOperation is not null)
                    existingAdminOperation.DateScheduled = newActionDate;
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
                    bool success = Guid.TryParse(cover.TeacherId, out Guid staffIdGuid);
                    StaffId staffId = success
                        ? StaffId.FromValue(staffIdGuid)
                        : StaffId.Empty;

                    TeacherMSTeamOperation reAddOperation = new()
                    {
                        OfferingId = cover.OfferingId,
                        StaffId = staffId,
                        CoverId = Guid.Empty,
                        Action = MSTeamOperationAction.Add,
                        PermissionLevel = MSTeamOperationPermissionLevel.Owner,
                        DateScheduled = DateTime.Now
                    };

                    _operationsRepository.Insert(reAddOperation);

                    TeacherMSTeamOperation removeTimelyOperation = new()
                    {
                        OfferingId = cover.OfferingId,
                        StaffId = staffId,
                        CoverId = Guid.Empty,
                        Action = MSTeamOperationAction.Remove,
                        PermissionLevel = MSTeamOperationPermissionLevel.Owner,
                        DateScheduled = newActionDate
                    };

                    _operationsRepository.Insert(removeTimelyOperation);
                }

                foreach (EmployeeId employeeId in _configuration.Covers.CoverContacts)
                {
                    StaffMember? teacher = await _staffRepository.GetByEmployeeId(employeeId, cancellationToken);

                    if (teacher is null)
                    {
                        _logger
                            .ForContext(nameof(CoverEndDateChangedDomainEvent), notification, true)
                            .ForContext(nameof(Error), StaffMemberErrors.NotFoundByEmployeeId(employeeId), true)
                            .ForContext(nameof(EmployeeId), employeeId)
                            .Warning("Failed to update Cover Teams Access");

                        continue;
                    }

                    TeacherMSTeamOperation? existingAdminRemoveOperation = existingRequests
                        .OfType<TeacherMSTeamOperation>()
                        .FirstOrDefault(operation =>
                            operation.StaffId == teacher.Id &&
                            operation.Action == MSTeamOperationAction.Remove &&
                            operation.DateScheduled == alreadyRemoved.DateScheduled);

                    if (existingAdminRemoveOperation is not null)
                    {
                        existingAdminRemoveOperation.Delete();

                        TeacherMSTeamOperation adminAddOperation = new()
                        {
                            OfferingId = cover.OfferingId,
                            StaffId = teacher.Id,
                            Action = MSTeamOperationAction.Add,
                            PermissionLevel = MSTeamOperationPermissionLevel.Owner,
                            DateScheduled = DateTime.Now,
                            CoverId = Guid.Empty
                        };

                        _operationsRepository.Insert(adminAddOperation);
                    }

                    TeacherMSTeamOperation adminRemoveOperation = new()
                    {
                        OfferingId = cover.OfferingId,
                        StaffId = teacher.Id,
                        Action = MSTeamOperationAction.Remove,
                        PermissionLevel = MSTeamOperationPermissionLevel.Owner,
                        DateScheduled = newActionDate,
                        CoverId = cover.Id.Value
                    };

                    _operationsRepository.Insert(adminRemoveOperation);
                }
            }
        }

        await _unitOfWork.CompleteAsync(cancellationToken);
    }
}
