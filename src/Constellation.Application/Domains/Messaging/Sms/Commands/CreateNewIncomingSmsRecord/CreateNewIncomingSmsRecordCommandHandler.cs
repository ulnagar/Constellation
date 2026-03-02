namespace Constellation.Application.Domains.Messaging.Sms.Commands.CreateNewIncomingSmsRecord;

using Abstractions.Messaging;
using Constellation.Application.Domains.Messaging.Sms.Enums;
using Constellation.Application.Domains.Messaging.Sms.Models;
using Core.Shared;
using Interfaces.Repositories;
using Repositories;
using Serilog;
using System.Globalization;

internal sealed class CreateNewIncomingSmsRecordCommandHandler
: ICommandHandler<CreateNewIncomingSmsRecordCommand>
{
    private readonly ISmsRepository _smsRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public CreateNewIncomingSmsRecordCommandHandler(
        ISmsRepository smsRepository,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _smsRepository = smsRepository;
        _unitOfWork = unitOfWork;
        _logger = logger
            .ForContext<CreateNewIncomingSmsRecordCommand>();
    }

    public async Task<Result> Handle(CreateNewIncomingSmsRecordCommand request, CancellationToken cancellationToken)
    {
        // Match to original outgoing message using phone number + time window
        SmsMessage? originalMessage = string.IsNullOrWhiteSpace(request.IncomingSms.From)
            ? null
            : await _smsRepository.GetMostRecentOutboundToNumber(request.IncomingSms.From, cancellationToken);
        
        SmsMessage inboundMessage = new SmsMessage()
        {
            SmsGlobalId = request.IncomingSms.MsgId.ToString(CultureInfo.InvariantCulture),
            From = request.IncomingSms.From!,
            To = request.IncomingSms.To.ToString(CultureInfo.InvariantCulture),
            Message = request.IncomingSms.Msg!,
            Direction = SmsDirection.Inbound,
            Status = SmsStatus.Received,
            CreatedAt = DateTimeOffset.UtcNow,
            SmsGlobalDate = DateTimeOffset.Parse(request.IncomingSms.Date!, DateTimeFormatInfo.CurrentInfo),
            ReplyToId = originalMessage?.Id ?? null
        };

        _smsRepository.Insert(inboundMessage);

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
