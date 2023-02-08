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

internal sealed class CoverCancelledDomainEvent_RemoveAdobeConnectAccessHandler
    : IDomainEventHandler<CoverCancelledDomainEvent>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IClassCoverRepository _classCoverRepository;
    private readonly IAdobeConnectOperationsRepository _operationsRepository;
    private readonly ILogger _logger;

    public CoverCancelledDomainEvent_RemoveAdobeConnectAccessHandler(
        IUnitOfWork unitOfWork,
        IClassCoverRepository classCoverRepository,
        IAdobeConnectOperationsRepository operationsRepository,
        Serilog.ILogger logger)
    {
        _unitOfWork = unitOfWork;
        _classCoverRepository = classCoverRepository;
        _operationsRepository = operationsRepository;
        _logger = logger.ForContext<CoverCancelledDomainEvent>();
    }

    public async Task Handle(CoverCancelledDomainEvent notification, CancellationToken cancellationToken)
    {
        var cover = await _classCoverRepository.GetById(notification.CoverId, cancellationToken);

        if (cover is null)
        {
            _logger.Error("{action}: Could not find cover with Id {id} in database", nameof(CoverCancelledDomainEvent_RemoveAdobeConnectAccessHandler), notification.CoverId);

            return;
        }

        var existingRequests = await _operationsRepository
            .GetByCoverId(notification.CoverId, cancellationToken);

        if (existingRequests is null)
        {
            _logger.Warning("{action}: Could not find operations for cover with Id {id} in database", nameof(CoverCancelledDomainEvent_RemoveAdobeConnectAccessHandler), notification.CoverId);
        }

        var requestsByRoom = existingRequests.GroupBy(operation => operation.ScoId);

        foreach (var room in requestsByRoom)
        {
            var addRequests = room
                .Where(operation =>
                    operation.Action == AdobeConnectOperationAction.Add)
                .ToList();

            var removeRequests = room
                .Where(operation =>
                    operation.Action == AdobeConnectOperationAction.Remove)
                .ToList();

            // In the set of operations for this cover, has access been granted and not yet revoked?
            var accessGranted = addRequests
                    .Any(operation => operation.IsCompleted);

            var accessRevoked = removeRequests
                    .Any(operation => operation.IsCompleted);

            if (accessGranted && !accessRevoked)
            {
                // Set the first unactioned remove request to process asap
                var removeOperation = removeRequests.First(operation => !operation.IsCompleted && !operation.IsDeleted);

                if (removeOperation is null)
                {
                    // Create new operation to remove access
                    if (cover.TeacherType == CoverTeacherType.Casual)
                    {
                        removeOperation = new CasualAdobeConnectOperation
                        {
                            ScoId = room.Key,
                            CasualId = int.Parse(cover.TeacherId),
                            Action = AdobeConnectOperationAction.Remove,
                            DateScheduled = DateTime.Now,
                            CoverId = cover.Id
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
                            CoverId = cover.Id
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
        }

        await _unitOfWork.CompleteAsync(cancellationToken);
    }
}
