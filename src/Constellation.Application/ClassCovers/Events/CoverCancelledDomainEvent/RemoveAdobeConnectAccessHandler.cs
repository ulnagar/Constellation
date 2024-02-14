namespace Constellation.Application.ClassCovers.Events.CoverCancelledDomainEvent;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.DomainEvents;
using Constellation.Core.Enums;
using Constellation.Core.Models;
using Constellation.Core.ValueObjects;
using Core.Models.Covers;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class RemoveAdobeConnectAccessHandler
    : IDomainEventHandler<CoverCancelledDomainEvent>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IClassCoverRepository _classCoverRepository;
    private readonly IAdobeConnectOperationsRepository _operationsRepository;
    private readonly ILogger _logger;

    public RemoveAdobeConnectAccessHandler(
        IUnitOfWork unitOfWork,
        IClassCoverRepository classCoverRepository,
        IAdobeConnectOperationsRepository operationsRepository,
        ILogger logger)
    {
        _unitOfWork = unitOfWork;
        _classCoverRepository = classCoverRepository;
        _operationsRepository = operationsRepository;
        _logger = logger.ForContext<CoverCancelledDomainEvent>();
    }

    public async Task Handle(CoverCancelledDomainEvent notification, CancellationToken cancellationToken)
    {
        ClassCover cover = await _classCoverRepository.GetById(notification.CoverId, cancellationToken);

        if (cover is null)
        {
            _logger.Error("{action}: Could not find cover with Id {id} in database", nameof(RemoveAdobeConnectAccessHandler), notification.CoverId);

            return;
        }

        List<AdobeConnectOperation> existingRequests = await _operationsRepository
            .GetByCoverId(notification.CoverId, cancellationToken);

        if (existingRequests is null)
        {
            _logger.Warning("{action}: Could not find operations for cover with Id {id} in database", nameof(RemoveAdobeConnectAccessHandler), notification.CoverId);

            return;
        }

        IEnumerable<IGrouping<string, AdobeConnectOperation>> requestsByRoom = existingRequests.GroupBy(operation => operation.ScoId);

        foreach (IGrouping<string, AdobeConnectOperation> room in requestsByRoom)
        {
            List<AdobeConnectOperation> addRequests = room
                .Where(operation =>
                    operation.Action == AdobeConnectOperationAction.Add)
                .ToList();

            List<AdobeConnectOperation> removeRequests = room
                .Where(operation =>
                    operation.Action == AdobeConnectOperationAction.Remove)
                .ToList();

            // In the set of operations for this cover, has access been granted and not yet revoked?
            bool accessGranted = addRequests
                    .Any(operation => operation.IsCompleted);

            bool accessRevoked = removeRequests
                    .Any(operation => operation.IsCompleted);

            if (accessGranted && !accessRevoked)
            {
                // Set the first un-actioned remove request to process asap
                AdobeConnectOperation removeOperation = removeRequests.FirstOrDefault(operation => !operation.IsCompleted && !operation.IsDeleted);

                if (removeOperation is null)
                {
                    // Create new operation to remove access
                    if (cover.TeacherType == CoverTeacherType.Casual)
                    {
                        removeOperation = new CasualAdobeConnectOperation
                        {
                            ScoId = room.Key,
                            CasualId = Guid.Parse(cover.TeacherId),
                            Action = AdobeConnectOperationAction.Remove,
                            DateScheduled = DateTime.Now,
                            CoverId = cover.Id.Value
                        };

                        _operationsRepository.Insert(removeOperation);
                    }
                    else
                    {
                        removeOperation = new TeacherAdobeConnectOperation
                        {
                            ScoId = room.Key,
                            StaffId = cover.TeacherId,
                            Action = AdobeConnectOperationAction.Remove,
                            DateScheduled = DateTime.Now,
                            CoverId = cover.Id.Value
                        };

                        _operationsRepository.Insert(removeOperation);
                    }
                }
                else
                {
                    removeOperation.DateScheduled = DateTime.Now;

                    List<AdobeConnectOperation> remainingRemoveOperations = removeRequests.Where(operation => !operation.IsCompleted && !operation.IsDeleted).Skip(1).ToList();

                    foreach (AdobeConnectOperation operation in remainingRemoveOperations)
                    {
                        operation.IsDeleted = true;
                    }
                }

                List<AdobeConnectOperation> remainingAddOperations = addRequests.Where(operation => !operation.IsCompleted && !operation.IsDeleted).ToList();

                foreach (AdobeConnectOperation operation in remainingAddOperations)
                {
                    operation.IsDeleted = true;
                }
            }
            else
            {
                List<AdobeConnectOperation> removeOperations = removeRequests.Where(operation => !operation.IsCompleted && !operation.IsDeleted).ToList();

                foreach (AdobeConnectOperation operation in removeOperations)
                {
                    operation.IsDeleted = true;
                }

                List<AdobeConnectOperation> addOperations = addRequests.Where(operation => !operation.IsCompleted && !operation.IsDeleted).ToList();

                foreach (AdobeConnectOperation operation in addOperations)
                {
                    operation.IsDeleted = true;
                }
            }
        }

        await _unitOfWork.CompleteAsync(cancellationToken);
    }
}
