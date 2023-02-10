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

        var requestsByRoom = existingRequests
            .Where(operation => !operation.IsDeleted)
            .GroupBy(operation => operation.ScoId);

        foreach (var room in requestsByRoom)
        {
            var removeRequests = room
                .Where(operation =>
                    operation.Action == AdobeConnectOperationAction.Remove)
                .ToList();

            // Process removal
            var alreadyRemoved = removeRequests.FirstOrDefault(operation => operation.IsCompleted);

            if (alreadyRemoved is null)
            {
                foreach (var request in removeRequests)
                {
                    request.DateScheduled = notification.NewEndDate.ToDateTime(TimeOnly.MinValue).AddDays(1);
                }
            }
            else
            {
                var previousActionDate = alreadyRemoved.DateScheduled;
                var newActionDate = notification.NewEndDate.ToDateTime(TimeOnly.MinValue).AddDays(1);

                alreadyRemoved.Delete();

                if (newActionDate > previousActionDate)
                {
                    // Remove access, then create new add operation
                    if (cover.TeacherType == CoverTeacherType.Casual)
                    {
                        var reAddOperation = new CasualAdobeConnectOperation
                        {
                            ScoId = room.Key,
                            CasualId = int.Parse(cover.TeacherId),
                            Action = AdobeConnectOperationAction.Add,
                            DateScheduled = DateTime.Now,
                            CoverId = Guid.Empty
                        };

                        _operationsRepository.Insert(reAddOperation);
                    }
                    else
                    {
                        var reAddOperation = new TeacherAdobeConnectOperation
                        {
                            ScoId = room.Key,
                            StaffId = cover.TeacherId,
                            Action = AdobeConnectOperationAction.Add,
                            DateScheduled = DateTime.Now,
                            CoverId = Guid.Empty
                        };

                        _operationsRepository.Insert(reAddOperation);
                    }
                }

                if (cover.TeacherType == CoverTeacherType.Casual)
                {
                    var removeTimelyOperation = new CasualAdobeConnectOperation
                    {
                        ScoId = room.Key,
                        CasualId = int.Parse(cover.TeacherId),
                        Action = AdobeConnectOperationAction.Remove,
                        DateScheduled = newActionDate,
                        CoverId = cover.Id
                    };

                    _operationsRepository.Insert(removeTimelyOperation);
                }
                else
                {
                    var removeTimelyOperation = new TeacherAdobeConnectOperation
                    {
                        ScoId = room.Key,
                        StaffId = cover.TeacherId,
                        Action = AdobeConnectOperationAction.Remove,
                        DateScheduled = newActionDate,
                        CoverId = cover.Id
                    };

                    _operationsRepository.Insert(removeTimelyOperation);
                }
            }
        }

        await _unitOfWork.CompleteAsync(cancellationToken);
    }
}
