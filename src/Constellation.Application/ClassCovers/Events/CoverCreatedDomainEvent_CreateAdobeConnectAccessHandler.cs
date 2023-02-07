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
using System.Threading;
using System.Threading.Tasks;

internal sealed class CoverCreatedDomainEvent_CreateAdobeConnectAccessHandler
    : IDomainEventHandler<CoverCreatedDomainEvent>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAdobeConnectOperationsRepository _operationsRepository;
    private readonly IClassCoverRepository _classCoverRepository;
    private readonly IAdobeConnectRoomRepository _roomRepository;
    private readonly ILogger _logger;

    public CoverCreatedDomainEvent_CreateAdobeConnectAccessHandler(
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
        _logger = logger.ForContext<CoverCreatedDomainEvent>();
    }

    public async Task Handle(CoverCreatedDomainEvent notification, CancellationToken cancellationToken)
    {
        var cover = await _classCoverRepository.GetById(notification.CoverId, cancellationToken);

        if (cover is null)
        {
            _logger.Error("{action}: Could not find cover with Id {id} in database", nameof(CoverCreatedDomainEvent_CreateAdobeConnectAccessHandler), notification.CoverId);

            return;
        }

        var rooms = await _roomRepository.GetByOfferingId(cover.OfferingId, cancellationToken);

        foreach (var room in rooms)
        {
            if (cover.TeacherType == CoverTeacherType.Casual)
            {
                var addOperation = new CasualAdobeConnectOperation
                {
                    ScoId = room.ScoId,
                    CasualId = int.Parse(cover.TeacherId),
                    Action = AdobeConnectOperationAction.Add,
                    DateScheduled = cover.StartDate.ToDateTime(TimeOnly.MinValue).AddDays(-1),
                    CoverId = cover.Id
                };

                _operationsRepository.Insert(addOperation);

                var removeOperation = new CasualAdobeConnectOperation
                {
                    ScoId = room.ScoId,
                    CasualId = int.Parse(cover.TeacherId),
                    Action = AdobeConnectOperationAction.Remove,
                    DateScheduled = cover.EndDate.ToDateTime(TimeOnly.MinValue).AddDays(1),
                    CoverId = cover.Id
                };

                _operationsRepository.Insert(removeOperation);
            }
            else
            {
                var addOperation = new TeacherAdobeConnectOperation
                {
                    ScoId = room.ScoId,
                    StaffId = cover.TeacherId,
                    Action = AdobeConnectOperationAction.Add,
                    DateScheduled = cover.StartDate.ToDateTime(TimeOnly.MinValue).AddDays(-1),
                    CoverId = cover.Id
                };

                _operationsRepository.Insert(addOperation);

                var removeOperation = new TeacherAdobeConnectOperation
                {
                    ScoId = room.ScoId,
                    StaffId = cover.TeacherId,
                    Action = AdobeConnectOperationAction.Remove,
                    DateScheduled = cover.EndDate.ToDateTime(TimeOnly.MinValue).AddDays(1),
                    CoverId = cover.Id
                };

                _operationsRepository.Insert(removeOperation);
            }

            await _unitOfWork.CompleteAsync(cancellationToken);
        }
    }
}