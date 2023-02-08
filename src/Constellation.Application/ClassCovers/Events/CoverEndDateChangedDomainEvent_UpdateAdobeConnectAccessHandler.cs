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

internal sealed class CoverEndDateChangedDomainEvent_UpdateAdobeConnectAccessHandler
    : IDomainEventHandler<CoverEndDateChangedDomainEvent>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAdobeConnectOperationsRepository _operationsRepository;
    private readonly IClassCoverRepository _classCoverRepository;
    private readonly IAdobeConnectRoomRepository _roomRepository;
    private readonly ILogger _logger;

    public CoverEndDateChangedDomainEvent_UpdateAdobeConnectAccessHandler(
        IUnitOfWork unitOfWork,
        IAdobeConnectOperationsRepository operationsRepository,
        IClassCoverRepository classCoverRepository,
        IAdobeConnectRoomRepository roomRepository,
        Serilog.ILogger logger)
    {
        _unitOfWork = unitOfWork;
        _operationsRepository = operationsRepository;
        _classCoverRepository = classCoverRepository;
        _roomRepository = roomRepository;
        _logger = logger.ForContext<CoverEndDateChangedDomainEvent>();
    }

    public async Task Handle(CoverEndDateChangedDomainEvent notification, CancellationToken cancellationToken)
    {
        var cover = await _classCoverRepository.GetById(notification.CoverId, cancellationToken);

        if (cover is null)
        {
            _logger.Error("{action}: Could not find cover with Id {id} in database", nameof(CoverEndDateChangedDomainEvent_UpdateAdobeConnectAccessHandler), notification.CoverId);

            return;
        }

        var existingRequests = await _operationsRepository
            .GetByCoverId(notification.CoverId, cancellationToken);

        if (existingRequests is null)
        {
            _logger.Warning("{action}: Could not find operations for cover with Id {id} in database", nameof(CoverEndDateChangedDomainEvent_UpdateAdobeConnectAccessHandler), notification.CoverId);
        }

        var requestsByRoom = existingRequests.GroupBy(operation => operation.ScoId);

        foreach (var room in requestsByRoom)
        {
            var removeRequests = room
                .Where(operation =>
                    operation.Action == AdobeConnectOperationAction.Remove)
                .ToList();

            // Set the first unactioned remove request to change date for
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
                        DateScheduled = notification.NewEndDate.ToDateTime(TimeOnly.MinValue).AddDays(1),
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
                        DateScheduled = notification.NewEndDate.ToDateTime(TimeOnly.MinValue).AddDays(1),
                        CoverId = cover.Id
                    };

                    _operationsRepository.Insert(removeOperation);
                }
            }
            else
            {
                removeOperation.DateScheduled = notification.NewEndDate.ToDateTime(TimeOnly.MinValue).AddDays(1);

                var remainingRemoveOperations = removeRequests.Where(operation => !operation.IsCompleted && !operation.IsDeleted).Skip(1).ToList();

                foreach (var operation in remainingRemoveOperations)
                {
                    operation.IsDeleted = true;
                }
            }
        }
    }
}
