namespace Constellation.Application.Domains.Covers.Events.CoverStartDateChangedDomainEvent;

using Abstractions.Messaging;
using Constellation.Core.Models.Covers.Events;
using Constellation.Core.Models.Covers.Repositories;
using Constellation.Core.Models.StaffMembers;
using Constellation.Core.Models.StaffMembers.Errors;
using Constellation.Core.Models.StaffMembers.Repositories;
using Constellation.Core.Models.StaffMembers.ValueObjects;
using Core.Enums;
using Core.Models;
using Core.Models.Covers;
using Core.Models.Covers.Enums;
using Core.Models.StaffMembers.Identifiers;
using Core.Shared;
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
    : IDomainEventHandler<CoverStartDateChangedDomainEvent>
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
        _logger = logger.ForContext<CoverStartDateChangedDomainEvent>();
    }

    public async Task Handle(CoverStartDateChangedDomainEvent notification, CancellationToken cancellationToken)
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
                .ToList<MSTeamOperation>();
        }
        else
        {
            coveringTeacherRequests = existingRequests
                .OfType<TeacherMSTeamOperation>()
                .Where(operation => operation.StaffId.ToString() == cover.TeacherId)
                .ToList<MSTeamOperation>();
        }

        List<MSTeamOperation> addRequests = coveringTeacherRequests
            .Where(operation =>
                operation.Action == MSTeamOperationAction.Add)
            .ToList();

        // If access has not been granted, change the operation
        // If it has, determine whether there is a big enough difference to remove access and then grant later or just leave it
        MSTeamOperation? alreadyGranted = addRequests.FirstOrDefault(operation => operation.IsCompleted);

        if (alreadyGranted is null)
        {
            DateTime newActionDate = notification.NewStartDate.ToDateTime(TimeOnly.MinValue).AddDays(-1);

            foreach (MSTeamOperation request in addRequests)
            {
                request.DateScheduled = newActionDate;
            }

            foreach (EmployeeId employeeId in _configuration.Covers.CoverContacts)
            {
                StaffMember? teacher = await _staffRepository.GetByEmployeeId(employeeId, cancellationToken);

                if (teacher is null)
                {
                    _logger
                        .ForContext(nameof(CoverStartAndEndDatesChangedDomainEvent), notification, true)
                        .ForContext(nameof(Error), StaffMemberErrors.NotFoundByEmployeeId(employeeId), true)
                        .ForContext(nameof(EmployeeId), employeeId)
                        .Warning("Failed to update Cover Teams Access");

                    continue;
                }

                TeacherMSTeamOperation? existingAddOperation = existingRequests
                    .OfType<TeacherMSTeamOperation>()
                    .FirstOrDefault(operation =>
                        operation.StaffId == teacher.Id &&
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
                    bool success = Guid.TryParse(cover.TeacherId, out Guid staffIdGuid);
                    StaffId staffId = success
                        ? StaffId.FromValue(staffIdGuid)
                        : StaffId.Empty;

                    TeacherMSTeamOperation removeEarlyOperation = new()
                    {
                        OfferingId = cover.OfferingId,
                        StaffId = staffId,
                        CoverId = Guid.Empty,
                        Action = MSTeamOperationAction.Remove,
                        PermissionLevel = MSTeamOperationPermissionLevel.Owner,
                        DateScheduled = DateTime.Today
                    };

                    _operationsRepository.Insert(removeEarlyOperation);

                    TeacherMSTeamOperation addTimelyOperation = new()
                    {
                        OfferingId = cover.OfferingId,
                        StaffId = staffId,
                        CoverId = cover.Id.Value,
                        Action = MSTeamOperationAction.Add,
                        PermissionLevel = MSTeamOperationPermissionLevel.Owner,
                        DateScheduled = newActionDate
                    };

                    _operationsRepository.Insert(addTimelyOperation);
                }

                foreach (EmployeeId employeeId in _configuration.Covers.CoverContacts)
                {
                    StaffMember? teacher = await _staffRepository.GetByEmployeeId(employeeId, cancellationToken);

                    if (teacher is null)
                    {
                        _logger
                            .ForContext(nameof(CoverStartAndEndDatesChangedDomainEvent), notification, true)
                            .ForContext(nameof(Error), StaffMemberErrors.NotFoundByEmployeeId(employeeId), true)
                            .ForContext(nameof(EmployeeId), employeeId)
                            .Warning("Failed to update Cover Teams Access");

                        continue;
                    }

                    TeacherMSTeamOperation? existingAddOperation = existingRequests
                        .OfType<TeacherMSTeamOperation>()
                        .FirstOrDefault(operation =>
                            operation.StaffId == teacher.Id &&
                            operation.Action == MSTeamOperationAction.Add &&
                            operation.DateScheduled == alreadyGranted.DateScheduled);

                    if (existingAddOperation is not null)
                    {
                        existingAddOperation.Delete();

                        TeacherMSTeamOperation removeOperation = new()
                        {
                            OfferingId = cover.OfferingId,
                            StaffId = teacher.Id,
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
                        StaffId = teacher.Id,
                        Action = MSTeamOperationAction.Add,
                        PermissionLevel = MSTeamOperationPermissionLevel.Owner,
                        DateScheduled = newActionDate,
                        CoverId = cover.Id.Value
                    };

                    _operationsRepository.Insert(addOperation);
                }
            }
        }

        await _unitOfWork.CompleteAsync(cancellationToken);
    }
}
