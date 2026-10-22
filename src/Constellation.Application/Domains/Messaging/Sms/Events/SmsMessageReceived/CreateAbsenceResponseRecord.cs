namespace Constellation.Application.Domains.Messaging.Sms.Events.SmsMessageReceived;

using Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Models.Absences;
using Constellation.Core.Models.Absences.Enums;
using Constellation.Core.Models.Absences.Errors;
using Constellation.Core.Models.Messaging.Sms;
using Constellation.Core.Models.Messaging.Sms.Errors;
using Core.Models.Messaging.Enums;
using Core.Models.Messaging.Sms.Events;
using Core.Models.Messaging.Sms.Repositories;
using Core.Shared;
using Serilog;

internal sealed class CreateAbsenceResponseRecord
: IDomainEventHandler<SmsMessageReceivedDomainEvent>
{
    private readonly ISmsRepository _smsRepository;
    private readonly IAbsenceRepository _absenceRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public CreateAbsenceResponseRecord(
        ISmsRepository smsRepository,
        IAbsenceRepository absenceRepository,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _smsRepository = smsRepository;
        _absenceRepository = absenceRepository;
        _unitOfWork = unitOfWork;
        _logger = logger
            .ForContext<SmsMessageReceivedDomainEvent>();
    }

    public async Task Handle(SmsMessageReceivedDomainEvent notification, CancellationToken cancellationToken)
    {
        SmsMessage? message = await _smsRepository.GetById(notification.SmsId, cancellationToken);

        if (message is null)
        {
            _logger
                .ForContext(nameof(SmsMessageReceivedDomainEvent), notification, true)
                .ForContext(nameof(Error), SmsMessagingErrors.NotFound(notification.SmsId), true)
                .Warning("Failed to update Absence Record for received SMS");

            return;
        }

        if (message.SendingModule != MessageModule.Absences)
            return;

        Absence? absence = await _absenceRepository.GetByNotificationOutgoingId(message.OutgoingId, cancellationToken);

        if (absence is null)
        {
            _logger
                .ForContext(nameof(SmsMessageReceivedDomainEvent), notification, true)
                .ForContext(nameof(Error), AbsenceErrors.NoMatchingOutgoingId(message.OutgoingId), true)
                .Warning("Failed to update Absence Record for received SMS");

            return;
        }

        absence.AddResponse(
            ResponseType.Parent,
            message.Sender.ToString(),
            message.Message);

        await _unitOfWork.CompleteAsync(cancellationToken);
    }
}
