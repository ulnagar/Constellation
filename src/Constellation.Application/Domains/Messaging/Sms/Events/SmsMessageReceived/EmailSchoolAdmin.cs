namespace Constellation.Application.Domains.Messaging.Sms.Events.SmsMessageReceived;

using Abstractions.Messaging;
using Core.Abstractions.Repositories;
using Core.Enums;
using Core.Extensions;
using Core.Models.Families;
using Core.Models.Messaging.Sms;
using Core.Models.Messaging.Sms.Errors;
using Core.Models.Messaging.Sms.Events;
using Core.Models.Messaging.Sms.Repositories;
using Core.Models.Students;
using Core.Models.Students.Identifiers;
using Core.Models.Students.Repositories;
using Core.Shared;
using Core.ValueObjects;
using Interfaces.Services;
using Serilog;

internal sealed class EmailSchoolAdmin
: IDomainEventHandler<SmsMessageReceivedDomainEvent>
{
    private readonly IFamilyRepository _familyRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly ISmsRepository _smsRepository;
    private readonly IEmailService _emailService;
    private readonly ILogger _logger;

    public EmailSchoolAdmin(
        IFamilyRepository familyRepository,
        IStudentRepository studentRepository,
        ISmsRepository smsRepository,
        IEmailService emailService,
        ILogger logger)
    {
        _familyRepository = familyRepository;
        _studentRepository = studentRepository;
        _smsRepository = smsRepository;
        _emailService = emailService;
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
                .Warning("Failed to forward received SMS");

            return;
        }

        Result<PhoneNumber> parentPhoneNumber = PhoneNumber.Create(message.Sender.Number);

        if (parentPhoneNumber.IsFailure)
        {
            await _emailService.SendIncomingSmsAlert(message, [], cancellationToken);
            return;
        }

        List<Family> families = await _familyRepository.GetFamilyByMobileNumber(parentPhoneNumber.Value, cancellationToken);

        List<StudentId> studentIds = families
            .SelectMany(entry => entry.Students)
            .Select(entry => entry.StudentId)
            .Distinct()
            .ToList();

        List<Student> students = await _studentRepository.GetListFromIds(studentIds, cancellationToken);

        List<string> studentNames = [];
        foreach (var student in students.OrderBy(entry => entry.CurrentEnrolment?.Grade ?? Grade.SpecialProgram))
            studentNames.Add($"{student.StudentReferenceNumber} - {student.Name.DisplayName} - {student.CurrentEnrolment?.Grade.AsName()}");

        await _emailService.SendIncomingSmsAlert(message, studentNames, cancellationToken);
    }
}
