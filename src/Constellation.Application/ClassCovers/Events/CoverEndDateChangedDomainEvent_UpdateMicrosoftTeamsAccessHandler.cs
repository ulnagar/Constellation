namespace Constellation.Application.ClassCovers.Events;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions;
using Constellation.Core.DomainEvents;
using Constellation.Core.Enums;
using Constellation.Core.Models;
using Constellation.Core.ValueObjects;
using Serilog;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class CoverEndDateChangedDomainEvent_UpdateMicrosoftTeamsAccessHandler
    : IDomainEventHandler<CoverEndDateChangedDomainEvent>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMSTeamOperationsRepository _operationsRepository;
    private readonly IClassCoverRepository _classCoverRepository;
    private readonly ILogger _logger;

    public CoverEndDateChangedDomainEvent_UpdateMicrosoftTeamsAccessHandler(
        IUnitOfWork unitOfWork,
        IMSTeamOperationsRepository operationsRepository,
        IClassCoverRepository classCoverRepository,
        Serilog.ILogger logger)
    {
        _unitOfWork = unitOfWork;
        _operationsRepository = operationsRepository;
        _classCoverRepository = classCoverRepository;
        _logger = logger.ForContext<CoverEndDateChangedDomainEvent>();
    }

    public async Task Handle(CoverEndDateChangedDomainEvent notification, CancellationToken cancellationToken)
    {
        var cover = await _classCoverRepository.GetById(notification.CoverId, cancellationToken);

        if (cover is null)
        {
            _logger.Warning("{action}: Could not find cover with Id {id} in database", nameof(CoverEndDateChangedDomainEvent_UpdateMicrosoftTeamsAccessHandler), notification.CoverId);

            return;
        }

        var existingRequests = await _operationsRepository
            .GetByCoverId(notification.CoverId, cancellationToken);

        if (existingRequests is null)
        {
            _logger.Warning("{action}: Could not find operations for cover with Id {id} in database", nameof(CoverEndDateChangedDomainEvent_UpdateMicrosoftTeamsAccessHandler), notification.CoverId);
        }

        var coveringTeacherRequests = existingRequests;

        if (cover.TeacherType == CoverTeacherType.Casual)
        {
            coveringTeacherRequests = existingRequests
                .OfType<CasualMSTeamOperation>()
                .Where(operation => operation.CasualId == int.Parse(cover.TeacherId))
                .ToList<MSTeamOperation>();
        }
        else
        {
            coveringTeacherRequests = existingRequests
                .OfType<TeacherMSTeamOperation>()
                .Where(operation => operation.StaffId == cover.TeacherId)
                .ToList<MSTeamOperation>();
        }

        var removeRequests = coveringTeacherRequests
            .Where(operation =>
                operation.Action == MSTeamOperationAction.Remove)
            .ToList();

        // Process removal
        var alreadyRemoved = removeRequests.FirstOrDefault(operation => operation.IsCompleted);

        if (alreadyRemoved is null)
        {
            var newActionDate = notification.NewEndDate.ToDateTime(TimeOnly.MinValue).AddDays(1);

            foreach (var request in removeRequests)
            {
                request.DateScheduled = newActionDate;
            }

            var existingCathyRemoveOperation = existingRequests
                .OfType<TeacherMSTeamOperation>()
                .FirstOrDefault(operation =>
                    operation.StaffId == "1030937" &&
                    operation.Action == MSTeamOperationAction.Remove);

            if (existingCathyRemoveOperation is not null)
            {
                existingCathyRemoveOperation.DateScheduled = newActionDate;
            }

            var existingKarenRemoveOperation = existingRequests
                .OfType<TeacherMSTeamOperation>()
                .FirstOrDefault(operation =>
                    operation.StaffId == "1112830" &&
                    operation.Action == MSTeamOperationAction.Remove);

            if (existingKarenRemoveOperation is not null)
            {
                existingKarenRemoveOperation.DateScheduled = newActionDate;
            }
        }
        else
        {
            var previousActionDate = alreadyRemoved.DateScheduled;
            var newActionDate = notification.NewEndDate.ToDateTime(TimeOnly.MinValue).AddDays(1);

            if (newActionDate > previousActionDate)
            {
                alreadyRemoved.Delete();

                // Re-Add access, then create new removal operations
                if (cover.TeacherType == CoverTeacherType.Casual)
                {
                    var reAddOperation = new CasualMSTeamOperation
                    {
                        OfferingId = cover.OfferingId,
                        CasualId = int.Parse(cover.TeacherId),
                        CoverId = Guid.Empty,
                        Action = MSTeamOperationAction.Add,
                        PermissionLevel = MSTeamOperationPermissionLevel.Owner,
                        DateScheduled = DateTime.Now
                    };

                    _operationsRepository.Insert(reAddOperation);

                    var removeTimelyOperation = new CasualMSTeamOperation
                    {
                        OfferingId = cover.OfferingId,
                        CasualId = int.Parse(cover.TeacherId),
                        CoverId = Guid.Empty,
                        Action = MSTeamOperationAction.Remove,
                        PermissionLevel = MSTeamOperationPermissionLevel.Owner,
                        DateScheduled = newActionDate
                    };

                    _operationsRepository.Insert(removeTimelyOperation);

                }
                else
                {
                    var reAddOperation = new TeacherMSTeamOperation
                    {
                        OfferingId = cover.OfferingId,
                        StaffId = cover.TeacherId,
                        CoverId = Guid.Empty,
                        Action = MSTeamOperationAction.Add,
                        PermissionLevel = MSTeamOperationPermissionLevel.Owner,
                        DateScheduled = DateTime.Now
                    };

                    _operationsRepository.Insert(reAddOperation);

                    var removeTimelyOperation = new TeacherMSTeamOperation
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

                var existingCathyRemoveOperation = existingRequests
                    .OfType<TeacherMSTeamOperation>()
                    .FirstOrDefault(operation =>
                        operation.StaffId == "1030937" &&
                        operation.Action == MSTeamOperationAction.Remove &&
                        operation.DateScheduled == alreadyRemoved.DateScheduled);

                if (existingCathyRemoveOperation is not null)
                {
                    existingCathyRemoveOperation.Delete();

                    var cathyAddOperation = new TeacherMSTeamOperation
                    {
                        OfferingId = cover.OfferingId,
                        StaffId = "1030937",
                        Action = MSTeamOperationAction.Add,
                        PermissionLevel = MSTeamOperationPermissionLevel.Owner,
                        DateScheduled = DateTime.Now,
                        CoverId = Guid.Empty
                    };

                    _operationsRepository.Insert(cathyAddOperation);
                }

                var cathyRemoveOperation = new TeacherMSTeamOperation
                {
                    OfferingId = cover.OfferingId,
                    StaffId = "1030937",
                    Action = MSTeamOperationAction.Remove,
                    PermissionLevel = MSTeamOperationPermissionLevel.Owner,
                    DateScheduled = newActionDate,
                    CoverId = cover.Id
                };

                _operationsRepository.Insert(cathyRemoveOperation);

                var existingKarenRemoveOperation = existingRequests
                    .OfType<TeacherMSTeamOperation>()
                    .FirstOrDefault(operation =>
                        operation.StaffId == "1112830" &&
                        operation.Action == MSTeamOperationAction.Remove &&
                        operation.DateScheduled == alreadyRemoved.DateScheduled);

                if (existingKarenRemoveOperation is not null)
                {
                    existingKarenRemoveOperation.Delete();

                    var karenAddOperation = new TeacherMSTeamOperation
                    {
                        OfferingId = cover.OfferingId,
                        StaffId = "1112830",
                        Action = MSTeamOperationAction.Add,
                        PermissionLevel = MSTeamOperationPermissionLevel.Owner,
                        DateScheduled = DateTime.Now,
                        CoverId = Guid.Empty
                    };

                    _operationsRepository.Insert(karenAddOperation);
                }

                var karenRemoveOperation = new TeacherMSTeamOperation
                {
                    OfferingId = cover.OfferingId,
                    StaffId = "1112830",
                    Action = MSTeamOperationAction.Remove,
                    PermissionLevel = MSTeamOperationPermissionLevel.Owner,
                    DateScheduled = newActionDate,
                    CoverId = cover.Id
                };

                _operationsRepository.Insert(karenRemoveOperation);
            }
        }

        await _unitOfWork.CompleteAsync(cancellationToken);
    }
}
