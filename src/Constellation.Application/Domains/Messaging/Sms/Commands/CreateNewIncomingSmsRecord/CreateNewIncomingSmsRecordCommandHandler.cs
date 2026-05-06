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
using Interfaces.Repositories;
using Serilog;
using System.Globalization;

internal sealed class CreateNewIncomingSmsRecordCommandHandler
: ICommandHandler<CreateNewIncomingSmsRecordCommand>
{
    private readonly ISmsRepository _smsRepository;
    private readonly IStaffRepository _staffRepository;
    private readonly IFamilyRepository _familyRepository;
    private readonly ISchoolContactRepository _contactRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public CreateNewIncomingSmsRecordCommandHandler(
        ISmsRepository smsRepository,
        IStaffRepository staffRepository,
        IFamilyRepository familyRepository,
        ISchoolContactRepository contactRepository,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
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
        Result<PhoneNumber> recipientPhoneNumber = PhoneNumber.Create(request.IncomingSms.To ?? string.Empty);

        SmsRecipient receiver = SmsRecipient.Unknown;

        if (recipientPhoneNumber.IsSuccess)
        {
            if (recipientPhoneNumber.Value.ToString(PhoneNumber.Format.None) == SmsRecipient.AuroraNoReply.Number)
                receiver = SmsRecipient.AuroraNoReply;

            if (recipientPhoneNumber.Value.ToString(PhoneNumber.Format.None) == SmsRecipient.Aurora.Number)
                receiver = SmsRecipient.Aurora;
        }

        Result<PhoneNumber> senderPhoneNumber = PhoneNumber.Create(request.IncomingSms.From ?? string.Empty);

        SmsRecipient? sender = null;

        if (senderPhoneNumber.IsSuccess)
        {
            SchoolContact? contact = await _contactRepository.GetByPhoneNumber(senderPhoneNumber.Value, cancellationToken);
            if (contact is not null)
                sender = SmsRecipient.Create(contact.Name, senderPhoneNumber.Value).Value;

            StaffMember? teacher = await _staffRepository.GetCurrentByPhoneNumber(senderPhoneNumber.Value, cancellationToken);
            if (teacher is not null)
                sender = SmsRecipient.Create(teacher.Name, senderPhoneNumber.Value).Value;

            Parent? parent = await _familyRepository.GetParentByMobileNumber(senderPhoneNumber.Value, cancellationToken);
            if (parent is not null && sender == null)
                sender = SmsRecipient.Create(parent.Name, senderPhoneNumber.Value).Value;
        }

        sender ??= SmsRecipient.Create("Unknown", senderPhoneNumber.Value.ToString(PhoneNumber.Format.None)).Value;

        SmsMessage inboundMessage = new(
            string.Empty,
            request.IncomingSms.MsgId?.ToString(CultureInfo.InvariantCulture) ?? string.Empty,
            sender,
            receiver,
            request.IncomingSms.Msg!,
            MessageDirection.Inbound,
            MessageStatus.Received,
            DateTimeOffset.UtcNow)
        {
            SmsGlobalDate = request.IncomingSms.Date.ToUniversalTime()
        };

        _smsRepository.Insert(inboundMessage);

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
