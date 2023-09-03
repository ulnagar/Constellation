namespace Constellation.Application.Enrolments.Events.EnrolmentDeletedDomainEvent;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions.Clock;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Enums;
using Constellation.Core.Errors;
using Constellation.Core.Models;
using Constellation.Core.Models.Enrolments.Events;
using Constellation.Core.Models.Offerings;
using Constellation.Core.Models.Offerings.Errors;
using Constellation.Core.Models.Offerings.ValueObjects;
using Constellation.Core.Shared;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class RemoveFromAdobeConnectRooms
    : IDomainEventHandler<EnrolmentDeletedDomainEvent>
{
    private readonly IStudentRepository _studentRepository;
    private readonly IOfferingRepository _offeringRepository;
    private readonly IAdobeConnectRoomRepository _roomRepository;
    private readonly IAdobeConnectOperationsRepository _operationRepository;
    private readonly IDateTimeProvider _dateTime;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public RemoveFromAdobeConnectRooms(
        IStudentRepository studentRepository,
        IOfferingRepository offeringRepository,
        IAdobeConnectRoomRepository roomRepository,
        IAdobeConnectOperationsRepository operationRepository,
        IDateTimeProvider dateTime,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _studentRepository = studentRepository;
        _offeringRepository = offeringRepository;
        _roomRepository = roomRepository;
        _operationRepository = operationRepository;
        _dateTime = dateTime;
        _unitOfWork = unitOfWork;
        _logger = logger.ForContext<EnrolmentDeletedDomainEvent>();
    }

    public async Task Handle(EnrolmentDeletedDomainEvent notification, CancellationToken cancellationToken)
    {
        Student student = await _studentRepository.GetById(notification.StudentId, cancellationToken);

        if (student is null)
        {
            _logger
                .ForContext(nameof(EnrolmentDeletedDomainEvent), notification, true)
                .ForContext(nameof(Error), DomainErrors.Partners.Student.NotFound(notification.StudentId))
                .Error("Failed to complete the event handler");

            return;
        }

        Offering offering = await _offeringRepository.GetById(notification.OfferingId, cancellationToken);

        if (offering is null)
        {
            _logger
                .ForContext(nameof(EnrolmentDeletedDomainEvent), notification, true)
                .ForContext(nameof(Error), OfferingErrors.NotFound(notification.OfferingId))
                .Error("Failed to complete the event handler");

            return;
        }

        List<string> roomIds = offering.Resources
            .Where(resource => resource.Type == ResourceType.AdobeConnectRoom)
            .Select(resource => resource as AdobeConnectRoomResource)
            .Select(resource => resource.ScoId)
            .Distinct()
            .ToList();

        foreach (string roomId in roomIds)
        {
            AdobeConnectRoom room = await _roomRepository.GetById(roomId, cancellationToken);

            if (room is null)
            {
                _logger
                    .ForContext(nameof(EnrolmentDeletedDomainEvent), notification, true)
                    .Warning("Could not remove student {student} from room {room}: Could not find room", student.DisplayName, roomId);

                continue;
            }

            StudentAdobeConnectOperation operation = new()
            {
                ScoId = roomId,
                StudentId = student.StudentId,
                Action = AdobeConnectOperationAction.Remove,
                DateScheduled = _dateTime.Now
            };

            _operationRepository.Insert(operation);
        }

        await _unitOfWork.CompleteAsync(cancellationToken);
    }
}
