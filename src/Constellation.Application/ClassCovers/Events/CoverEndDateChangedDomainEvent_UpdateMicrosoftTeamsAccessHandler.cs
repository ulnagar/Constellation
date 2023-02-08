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

namespace Constellation.Application.ClassCovers.Events;

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

        var removeRequests = existingRequests
            .Where(operation =>
                operation.Action == MSTeamOperationAction.Remove)
            .ToList();

        // Set the first unactioned remove request to process asap
        var removeOperations = removeRequests.Where(operation => !operation.IsCompleted && !operation.IsDeleted).ToList();

        if (removeOperations.Count == 0)
        {
            // Create new operation to remove access
            if (cover.TeacherType == CoverTeacherType.Casual)
            {
                var removeOperation = new CasualMSTeamOperation
                {
                    OfferingId = cover.OfferingId,
                    CasualId = int.Parse(cover.TeacherId),
                    CoverId = cover.Id,
                    Action = MSTeamOperationAction.Remove,
                    PermissionLevel = MSTeamOperationPermissionLevel.Owner,
                    DateScheduled = notification.NewEndDate.ToDateTime(TimeOnly.MinValue).AddDays(1)
                };

                _operationsRepository.Insert(removeOperation);
            }
            else
            {
                var removeOperation = new TeacherMSTeamOperation
                {
                    OfferingId = cover.OfferingId,
                    StaffId = cover.TeacherId,
                    CoverId = cover.Id,
                    Action = MSTeamOperationAction.Remove,
                    PermissionLevel = MSTeamOperationPermissionLevel.Owner,
                    DateScheduled = notification.NewEndDate.ToDateTime(TimeOnly.MinValue).AddDays(1)
                };

                _operationsRepository.Insert(removeOperation);
            }

            var cathyRemoveOperation = new TeacherMSTeamOperation
            {
                OfferingId = cover.OfferingId,
                StaffId = "1030937",
                Action = MSTeamOperationAction.Remove,
                PermissionLevel = MSTeamOperationPermissionLevel.Owner,
                DateScheduled = notification.NewEndDate.ToDateTime(TimeOnly.MinValue).AddDays(1),
                CoverId = cover.Id
            };

            _operationsRepository.Insert(cathyRemoveOperation);

            var karenRemoveOperation = new TeacherMSTeamOperation
            {
                OfferingId = cover.OfferingId,
                StaffId = "1112830",
                Action = MSTeamOperationAction.Remove,
                PermissionLevel = MSTeamOperationPermissionLevel.Owner,
                DateScheduled = notification.NewEndDate.ToDateTime(TimeOnly.MinValue).AddDays(1),
                CoverId = cover.Id
            };

            _operationsRepository.Insert(karenRemoveOperation);
        }
        else
        {
            foreach (var operation in removeOperations)
            {
                operation.DateScheduled = notification.NewEndDate.ToDateTime(TimeOnly.MinValue).AddDays(1);
            }
        }

        await _unitOfWork.CompleteAsync(cancellationToken);
    }
}
