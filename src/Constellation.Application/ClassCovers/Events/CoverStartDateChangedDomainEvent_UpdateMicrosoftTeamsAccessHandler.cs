namespace Constellation.Application.ClassCovers.Events;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions;
using Constellation.Core.DomainEvents;
using Constellation.Core.Enums;
using Constellation.Core.Models;
using Constellation.Core.ValueObjects;
using Serilog;
using System.Linq;
using System;
using System.Threading;
using System.Threading.Tasks;

internal sealed class CoverStartDateChangedDomainEvent_UpdateMicrosoftTeamsAccessHandler
    : IDomainEventHandler<CoverStartDateChangedDomainEvent>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMSTeamOperationsRepository _operationsRepository;
    private readonly IClassCoverRepository _classCoverRepository;
    private readonly ILogger _logger;

    public CoverStartDateChangedDomainEvent_UpdateMicrosoftTeamsAccessHandler(
        IUnitOfWork unitOfWork,
        IMSTeamOperationsRepository operationsRepository,
        IClassCoverRepository classCoverRepository,
        Serilog.ILogger logger)
    {
        _unitOfWork = unitOfWork;
        _operationsRepository = operationsRepository;
        _classCoverRepository = classCoverRepository;
        _logger = logger.ForContext<CoverStartDateChangedDomainEvent>();
    }

    public async Task Handle(CoverStartDateChangedDomainEvent notification, CancellationToken cancellationToken)
    {
        var cover = await _classCoverRepository.GetById(notification.CoverId, cancellationToken);

        if (cover is null)
        {
            _logger.Warning("{action}: Could not find cover with Id {id} in database", nameof(CoverStartDateChangedDomainEvent_UpdateMicrosoftTeamsAccessHandler), notification.CoverId);

            return;
        }

        var existingRequests = await _operationsRepository
            .GetByCoverId(notification.CoverId, cancellationToken);

        if (existingRequests is null)
        {
            _logger.Warning("{action}: Could not find operations for cover with Id {id} in database", nameof(CoverStartDateChangedDomainEvent_UpdateMicrosoftTeamsAccessHandler), notification.CoverId);
        }

        var coveringTeacherRequests = existingRequests;

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

        var addRequests = coveringTeacherRequests
            .Where(operation =>
                operation.Action == MSTeamOperationAction.Add)
            .ToList();

        // If access has not been granted, change the operation
        // If it has, determine whether there is a big enough difference to remove access and then grant later or just leave it
        var alreadyGranted = addRequests.FirstOrDefault(operation => operation.IsCompleted);

        if (alreadyGranted is null)
        {
            var newActionDate = notification.NewStartDate.ToDateTime(TimeOnly.MinValue).AddDays(-1);

            foreach (var request in addRequests)
            {
                request.DateScheduled = newActionDate;
            }

            var existingCathyAddOperation = existingRequests
                    .OfType<TeacherMSTeamOperation>()
                    .FirstOrDefault(operation =>
                        operation.StaffId == "1030937" &&
                        operation.Action == MSTeamOperationAction.Add);

            if (existingCathyAddOperation is not null)
            {
                existingCathyAddOperation.DateScheduled = newActionDate;
            }

            var existingKarenAddOperation = existingRequests
                .OfType<TeacherMSTeamOperation>()
                .FirstOrDefault(operation =>
                    operation.StaffId == "1112830" &&
                    operation.Action == MSTeamOperationAction.Add);

            if (existingKarenAddOperation is not null)
            {
                existingKarenAddOperation.DateScheduled = newActionDate;
            }
        }
        else
        {
            var previousActionDate = alreadyGranted.DateScheduled;
            var newActionDate = notification.NewStartDate.ToDateTime(TimeOnly.MinValue).AddDays(-1);

            if (newActionDate.Subtract(previousActionDate) > TimeSpan.FromDays(2))
            {
                alreadyGranted.Delete();

                // Remove access, then create new add operation
                if (cover.TeacherType == CoverTeacherType.Casual)
                {
                    var removeEarlyOperation = new CasualMSTeamOperation
                    {
                        OfferingId = cover.OfferingId,
                        CasualId = Guid.Parse(cover.TeacherId),
                        CoverId = Guid.Empty,
                        Action = MSTeamOperationAction.Remove,
                        PermissionLevel = MSTeamOperationPermissionLevel.Owner,
                        DateScheduled = DateTime.Today
                    };

                    _operationsRepository.Insert(removeEarlyOperation);

                    var addTimelyOperation = new CasualMSTeamOperation
                    {
                        OfferingId = cover.OfferingId,
                        CasualId = Guid.Parse(cover.TeacherId),
                        CoverId = cover.Id,
                        Action = MSTeamOperationAction.Add,
                        PermissionLevel = MSTeamOperationPermissionLevel.Owner,
                        DateScheduled = newActionDate
                    };

                    _operationsRepository.Insert(addTimelyOperation);
                }
                else
                {
                    var removeEarlyOperation = new TeacherMSTeamOperation
                    {
                        OfferingId = cover.OfferingId,
                        StaffId = cover.TeacherId,
                        CoverId = Guid.Empty,
                        Action = MSTeamOperationAction.Remove,
                        PermissionLevel = MSTeamOperationPermissionLevel.Owner,
                        DateScheduled = DateTime.Today
                    };

                    _operationsRepository.Insert(removeEarlyOperation);

                    var addTimelyOperation = new TeacherMSTeamOperation
                    {
                        OfferingId = cover.OfferingId,
                        StaffId = cover.TeacherId,
                        CoverId = cover.Id,
                        Action = MSTeamOperationAction.Add,
                        PermissionLevel = MSTeamOperationPermissionLevel.Owner,
                        DateScheduled = newActionDate
                    };

                    _operationsRepository.Insert(addTimelyOperation);
                }

                var existingCathyAddOperation = existingRequests
                    .OfType<TeacherMSTeamOperation>()
                    .FirstOrDefault(operation =>
                        operation.StaffId == "1030937" &&
                        operation.Action == MSTeamOperationAction.Add &&
                        operation.DateScheduled == alreadyGranted.DateScheduled);

                if (existingCathyAddOperation is not null)
                {
                    existingCathyAddOperation.Delete();

                    var cathyRemoveOperation = new TeacherMSTeamOperation
                    {
                        OfferingId = cover.OfferingId,
                        StaffId = "1030937",
                        Action = MSTeamOperationAction.Remove,
                        PermissionLevel = MSTeamOperationPermissionLevel.Owner,
                        DateScheduled = DateTime.Today,
                        CoverId = Guid.Empty
                    };

                    _operationsRepository.Insert(cathyRemoveOperation);
                }

                var cathyAddOperation = new TeacherMSTeamOperation
                {
                    OfferingId = cover.OfferingId,
                    StaffId = "1030937",
                    Action = MSTeamOperationAction.Add,
                    PermissionLevel = MSTeamOperationPermissionLevel.Owner,
                    DateScheduled = newActionDate,
                    CoverId = cover.Id
                };

                _operationsRepository.Insert(cathyAddOperation);

                var existingKarenAddOperation = existingRequests
                    .OfType<TeacherMSTeamOperation>()
                    .FirstOrDefault(operation =>
                        operation.StaffId == "1112830" &&
                        operation.Action == MSTeamOperationAction.Add &&
                        operation.DateScheduled == alreadyGranted.DateScheduled);

                if (existingKarenAddOperation is not null)
                {
                    existingKarenAddOperation.Delete();

                    var karenRemoveOperation = new TeacherMSTeamOperation
                    {
                        OfferingId = cover.OfferingId,
                        StaffId = "1112830",
                        Action = MSTeamOperationAction.Remove,
                        PermissionLevel = MSTeamOperationPermissionLevel.Owner,
                        DateScheduled = DateTime.Today,
                        CoverId = Guid.Empty
                    };

                    _operationsRepository.Insert(karenRemoveOperation);

                    var karenAddOperation = new TeacherMSTeamOperation
                    {
                        OfferingId = cover.OfferingId,
                        StaffId = "1112830",
                        Action = MSTeamOperationAction.Add,
                        PermissionLevel = MSTeamOperationPermissionLevel.Owner,
                        DateScheduled = newActionDate,
                        CoverId = cover.Id
                    };

                    _operationsRepository.Insert(karenAddOperation);
                }
            }
        }

        await _unitOfWork.CompleteAsync(cancellationToken);
    }
}
