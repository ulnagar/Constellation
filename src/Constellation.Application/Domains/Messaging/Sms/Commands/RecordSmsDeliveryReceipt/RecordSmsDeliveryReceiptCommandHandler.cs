namespace Constellation.Application.Domains.Messaging.Sms.Commands.RecordSmsDeliveryReceipt;

using Abstractions.Messaging;
using Constellation.Application.Domains.Messaging.Sms.Enums;
using Core.Shared;
using Errors;
using Interfaces.Repositories;
using Models;
using Repositories;
using Serilog;

internal sealed class RecordSmsDeliveryReceiptCommandHandler
: ICommandHandler<RecordSmsDeliveryReceiptCommand>
{
    private readonly ISmsRepository _smsRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public RecordSmsDeliveryReceiptCommandHandler(
        ISmsRepository smsRepository,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _smsRepository = smsRepository;
        _unitOfWork = unitOfWork;
        _logger = logger
            .ForContext<RecordSmsDeliveryReceiptCommand>();
    }

    public async Task<Result> Handle(RecordSmsDeliveryReceiptCommand request, CancellationToken cancellationToken)
    {
        SmsMessage? existing = await _smsRepository.GetByOutgoingId(request.Receipt.OutgoingId!, cancellationToken);

        if (existing is null)
        {
            _logger
                .ForContext(nameof(RecordSmsDeliveryReceiptCommand), request, true)
                .ForContext(nameof(Error), SmsMessagingErrors.DeliveryReceiptIncomplete, true)
                .Warning("SMS Delivery receipt received for unknown message");

            return Result.Failure(SmsMessagingErrors.DeliveryReceiptIncomplete);
        }

        existing.Status = request.Receipt.Status switch
        {
            "Delivered" => SmsStatus.Delivered,
            "Failed" => SmsStatus.Failed,
            _ => existing.Status
        };
        existing.StatusUpdatedAt = request.Receipt.DateTime;

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
