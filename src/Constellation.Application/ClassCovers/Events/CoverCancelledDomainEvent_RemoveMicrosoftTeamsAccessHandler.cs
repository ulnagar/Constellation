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

internal sealed class CoverCancelledDomainEvent_RemoveMicrosoftTeamsAccessHandler
    : IDomainEventHandler<CoverCancelledDomainEvent>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMSTeamOperationsRepository _operationsRepository;
    private readonly IClassCoverRepository _classCoverRepository;
    private readonly ILogger _logger;

    public CoverCancelledDomainEvent_RemoveMicrosoftTeamsAccessHandler(
        IUnitOfWork unitOfWork,
        IMSTeamOperationsRepository operationsRepository,
        IClassCoverRepository classCoverRepository,
        Serilog.ILogger logger)
    {
        _unitOfWork = unitOfWork;
        _operationsRepository = operationsRepository;
        _classCoverRepository = classCoverRepository;
        _logger = logger.ForContext<CoverCancelledDomainEvent>();
    }

    public async Task Handle(CoverCancelledDomainEvent notification, CancellationToken cancellationToken)
    {
        var cover = await _classCoverRepository.GetById(notification.CoverId, cancellationToken);

        if (cover is null)
        {
            _logger.Warning("{action}: Could not find cover with Id {id} in database", nameof(CoverCancelledDomainEvent), notification.CoverId);

            return;
        }

        var existingRequests = await _operationsRepository
            .GetByCoverId(notification.CoverId, cancellationToken);

        var addRequests = existingRequests
            .Where(operation =>
                operation.Action == MSTeamOperationAction.Add)
            .ToList();

        var removeRequests = existingRequests
            .Where(operation =>
                operation.Action == MSTeamOperationAction.Remove)
            .ToList();

        // In the set of operations for this cover, has access been granted and not yet revoked?
        var accessGranted = addRequests
                .Any(operation => operation.IsCompleted);

        var accessRevoked = removeRequests
                .Any(operation => !operation.IsCompleted);

        if (accessGranted && !accessRevoked)
        {
            // Set the first unactioned remove request to process asap
            var removeOperation = removeRequests.First(operation => !operation.IsCompleted && !operation.IsDeleted);

            if (removeOperation is null)
            {
                // Create new operation to remove access
                if (cover.TeacherType == CoverTeacherType.Casual)
                {
                    removeOperation = new CasualMSTeamOperation
                    {
                        OfferingId = cover.OfferingId,
                        CasualId = int.Parse(cover.TeacherId),
                        CoverId = cover.Id,
                        Action = MSTeamOperationAction.Remove,
                        PermissionLevel = MSTeamOperationPermissionLevel.Owner,
                        DateScheduled = DateTime.Now
                    };

                    _operationsRepository.Insert(removeOperation);
                }
                else
                {
                    removeOperation = new TeacherMSTeamOperation
                    {
                        OfferingId = cover.OfferingId,
                        StaffId = cover.TeacherId,
                        CoverId = cover.Id,
                        Action = MSTeamOperationAction.Remove,
                        PermissionLevel = MSTeamOperationPermissionLevel.Owner,
                        DateScheduled = DateTime.Now
                    };

                    _operationsRepository.Insert(removeOperation);
                }
            }
            else
            {
                removeOperation.DateScheduled = DateTime.Now;

                var remainingRemoveOperations = removeRequests.Where(operation => !operation.IsCompleted && !operation.IsDeleted).Skip(1).ToList();

                foreach (var operation in remainingRemoveOperations)
                {
                    operation.IsDeleted = true;
                }
            }

            var remainingAddOperations = addRequests.Where(operation => !operation.IsCompleted && !operation.IsDeleted).ToList();

            foreach (var operation in remainingAddOperations)
            {
                operation.IsDeleted = true;
            }
        }
        else
        {
            var removeOperations = removeRequests.Where(operation => !operation.IsCompleted && !operation.IsDeleted).ToList();

            foreach (var operation in removeOperations)
            {
                operation.IsDeleted = true;
            }

            var addOperations = addRequests.Where(operation => !operation.IsCompleted && !operation.IsDeleted).ToList();

            foreach (var operation in addOperations)
            {
                operation.IsDeleted = true;
            }
        }

        await _unitOfWork.CompleteAsync(cancellationToken);
    }
}
