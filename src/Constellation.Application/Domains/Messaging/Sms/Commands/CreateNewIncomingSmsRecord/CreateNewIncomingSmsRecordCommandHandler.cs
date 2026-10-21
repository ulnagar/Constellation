namespace Constellation.Application.Domains.Messaging.Sms.Commands.CreateNewIncomingSmsRecord;

using Abstractions.Messaging;
using Core.Abstractions.Repositories;
using Core.Models.Families;
using Core.Models.Messaging.Enums;
using Core.Models.Messaging.Sms;
using Core.Models.Messaging.Sms.Repositories;
using Core.Models.SchoolContacts;
using Core.Models.SchoolContacts.Repositories;
using Core.Models.StaffMembers;
using Core.Models.StaffMembers.Repositories;
using Core.Shared;
using Core.ValueObjects;
using Dtos;
using Interfaces.Gateways;
using Interfaces.Repositories;
using Serilog;

internal sealed class CreateNewIncomingSmsRecordCommandHandler
: ICommandHandler<CreateNewIncomingSmsRecordCommand>
{
    private readonly ISMSGateway _smsGateway;
    private readonly ISmsRepository _smsRepository;
    private readonly IStaffRepository _staffRepository;
    private readonly IFamilyRepository _familyRepository;
    private readonly ISchoolContactRepository _contactRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public CreateNewIncomingSmsRecordCommandHandler(
        ISMSGateway smsGateway,
        ISmsRepository smsRepository,
        IStaffRepository staffRepository,
        IFamilyRepository familyRepository,
        ISchoolContactRepository contactRepository,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _smsGateway = smsGateway;
        _smsRepository = smsRepository;
        _staffRepository = staffRepository;
        _familyRepository = familyRepository;
        _contactRepository = contactRepository;
        _unitOfWork = unitOfWork;
        _logger = logger
            .ForContext<CreateNewIncomingSmsRecordCommand>();
    }

    public async Task<Result> Handle(CreateNewIncomingSmsRecordCommand request, CancellationToken cancellationToken)
    {
        SmsMessage inboundMessage;

        if (string.IsNullOrWhiteSpace(request.IncomingSms.MsgId))
        {
            inboundMessage = await BuildMessage(
                request.IncomingSms.To ?? string.Empty,
                request.IncomingSms.From ?? string.Empty,
                request.IncomingSms.MsgId ?? string.Empty,
                request.IncomingSms.Msg ?? string.Empty,
                request.IncomingSms.Date,
                cancellationToken);

            _smsRepository.Insert(inboundMessage);

            await _unitOfWork.CompleteAsync(cancellationToken);

            return Result.Success();
        }
        
        Result<IncomingSmsEntry> messageFromApi = await _smsGateway.ReceiveSms(request.IncomingSms.MsgId, cancellationToken);

        if (messageFromApi.IsFailure)
        {
            inboundMessage = await BuildMessage(
                request.IncomingSms.To ?? string.Empty,
                request.IncomingSms.From ?? string.Empty,
                request.IncomingSms.MsgId,
                request.IncomingSms.Msg ?? string.Empty,
                request.IncomingSms.Date,
                cancellationToken);

            _smsRepository.Insert(inboundMessage);

            await _unitOfWork.CompleteAsync(cancellationToken);

            return Result.Success();
        }

        IncomingSmsEntry message = messageFromApi.Value;

        if (string.IsNullOrWhiteSpace(message.OutgoingId))
        {
            inboundMessage = await BuildMessage(
                message.Destination ?? string.Empty,
                message.Origin ?? string.Empty,
                message.Id ?? string.Empty,
                request.IncomingSms.Msg ?? string.Empty,
                message.DateTime,
                cancellationToken);

            _smsRepository.Insert(inboundMessage);

            await _unitOfWork.CompleteAsync(cancellationToken);

            return Result.Success();
        }

        SmsMessage? replyTo = await _smsRepository.GetByOutgoingId(message.OutgoingId, cancellationToken);

        if (replyTo is null)
        {
            inboundMessage = await BuildMessage(
                message.Destination ?? string.Empty,
                message.Origin ?? string.Empty,
                message.Id ?? string.Empty,
                request.IncomingSms.Msg ?? string.Empty,
                message.DateTime,
                cancellationToken);

            _smsRepository.Insert(inboundMessage);

            await _unitOfWork.CompleteAsync(cancellationToken);

            return Result.Success();
        }
        
        inboundMessage = new(
            replyTo.SendingModule,
            message.Id ?? string.Empty,
            replyTo.Recipient,
            replyTo.Sender,
            request.IncomingSms.Msg ?? string.Empty,
            MessageDirection.Inbound,
            MessageStatus.Received,
            DateTimeOffset.UtcNow)
        {
            SmsGlobalDate = message.DateTime.ToUniversalTime(),
            OutgoingId = replyTo.OutgoingId
        };

        _smsRepository.Insert(inboundMessage);

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }

    private async Task<SmsMessage> BuildMessage(string to, string from, string id, string msg, DateTimeOffset date, CancellationToken cancellationToken)
    {
        Result<PhoneNumber> recipientPhoneNumber = PhoneNumber.Create(to);

        SmsRecipient receiver = SmsRecipient.Unknown;

        if (recipientPhoneNumber.IsSuccess)
        {
            if (recipientPhoneNumber.Value.ToString(PhoneNumber.Format.None) == SmsRecipient.AuroraNoReply.Number)
                receiver = SmsRecipient.AuroraNoReply;

            if (recipientPhoneNumber.Value.ToString(PhoneNumber.Format.None) == SmsRecipient.Aurora.Number)
                receiver = SmsRecipient.Aurora;
        }

        Result<PhoneNumber> senderPhoneNumber = PhoneNumber.Create(from);
        SmsRecipient? sender = null;

        if (senderPhoneNumber.IsFailure)
            sender = SmsRecipient.Unknown;

        if (sender is null)
        {
            Parent? parent = await _familyRepository.GetParentByMobileNumber(senderPhoneNumber.Value, cancellationToken);
            if (parent is not null)
                sender = SmsRecipient.Create(parent.Name, senderPhoneNumber.Value).Value;
        }

        if (sender is null)
        {
            StaffMember? teacher = await _staffRepository.GetCurrentByPhoneNumber(senderPhoneNumber.Value, cancellationToken);
            if (teacher is not null)
                sender = SmsRecipient.Create(teacher.Name, senderPhoneNumber.Value).Value;
        }

        if (sender is null)
        {
            SchoolContact? contact = await _contactRepository.GetByPhoneNumber(senderPhoneNumber.Value, cancellationToken);
            if (contact is not null)
                sender = SmsRecipient.Create(contact.Name, senderPhoneNumber.Value).Value;
        }

        sender ??= SmsRecipient.Create("Unknown", from).Value;
        
        SmsMessage inboundMessage = new(
            string.Empty,
            id,
            sender,
            receiver,
            msg,
            MessageDirection.Inbound,
            MessageStatus.Received,
            DateTimeOffset.UtcNow)
        {
            SmsGlobalDate = date.ToUniversalTime()
        };

        return inboundMessage;
    }
}
