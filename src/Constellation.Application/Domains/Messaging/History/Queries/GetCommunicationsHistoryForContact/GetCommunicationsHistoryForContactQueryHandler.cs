namespace Constellation.Application.Domains.Messaging.History.Queries.GetCommunicationsHistoryForContact;

using Abstractions.Messaging;
using Core.Abstractions.Repositories;
using Core.Models;
using Core.Models.Families;
using Core.Models.Identifiers;
using Core.Models.Messaging.Email;
using Core.Models.Messaging.Email.Enums;
using Core.Models.Messaging.Email.Repositories;
using Core.Models.Messaging.Enums;
using Core.Models.Messaging.Sms;
using Core.Models.Messaging.Sms.Repositories;
using Core.Models.SchoolContacts;
using Core.Models.SchoolContacts.Identifiers;
using Core.Models.SchoolContacts.Repositories;
using Core.Models.StaffMembers;
using Core.Models.StaffMembers.Identifiers;
using Core.Models.StaffMembers.Repositories;
using Core.Models.Students;
using Core.Models.Students.Identifiers;
using Core.Models.Students.Repositories;
using Core.Shared;
using Core.ValueObjects;
using Interfaces.Repositories;
using Models;

internal sealed class GetCommunicationsHistoryForContactQueryHandler
    : IQueryHandler<GetCommunicationsHistoryForContactQuery, List<CommunicationRecordResponse>>
{
    private readonly IFamilyRepository _familyRepository;
    private readonly ISchoolRepository _schoolRepository;
    private readonly ISchoolContactRepository _contactRepository;
    private readonly IStaffRepository _staffRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly IEmailRepository _emailRepository;
    private readonly ISmsRepository _smsRepository;

    public GetCommunicationsHistoryForContactQueryHandler(
        IFamilyRepository familyRepository,
        ISchoolRepository schoolRepository,
        ISchoolContactRepository contactRepository,
        IStaffRepository staffRepository,
        IStudentRepository studentRepository,
        IEmailRepository emailRepository,
        ISmsRepository smsRepository)
    {
        _familyRepository = familyRepository;
        _schoolRepository = schoolRepository;
        _contactRepository = contactRepository;
        _staffRepository = staffRepository;
        _studentRepository = studentRepository;
        _emailRepository = emailRepository;
        _smsRepository = smsRepository;
    }

    public async Task<Result<List<CommunicationRecordResponse>>> Handle(GetCommunicationsHistoryForContactQuery request,
        CancellationToken cancellationToken)
    {
        (List<EmailMessage> Emails, List<SmsMessage> Sms) communications = request.Id switch
        {
            FamilyId familyId => await GetFamilyCommunications(familyId, cancellationToken),
            ParentId parentId => await GetParentCommunications(parentId, cancellationToken),
            SchoolCode schoolCode => await GetSchoolCommunications(schoolCode, cancellationToken),
            SchoolContactId schoolContactId => await GetSchoolContactCommunications(schoolContactId, cancellationToken),
            StaffId staffId => await GetStaffCommunications(staffId, cancellationToken),
            StudentId studentId => await GetStudentCommunications(studentId, cancellationToken),
            _ => throw new InvalidOperationException($"Unsupported entity type: {request.Id.GetType().Name}")
        };

        List<CommunicationRecordResponse> responses = [];

        foreach (var email in communications.Emails)
        {
            List<CommunicationRecordResponse.Recipient> recipients = [];

            foreach (EmailMessageRecipient recipient in email.Recipients)
            {
                recipients.Add(new(
                    recipient.RecipientType,
                    recipient.Name,
                    recipient.Email));
            }

            responses.Add(new(
                email.Id,
                MessageType.Email,
                MessageDirection.Outbound, 
                new(email.From.Name, email.From.Destination),
                recipients,
                email.Subject,
                email.Status,
                email.CreatedAt));
        }

        foreach (var sms in communications.Sms)
        {
            List<CommunicationRecordResponse.Recipient> recipients =
            [
                new(
                    EmailRecipientType.To,
                    sms.Recipient.Name,
                    sms.Recipient.Number)
            ];

            responses.Add(new(
                sms.Id,
                MessageType.SMS,
                sms.Direction,
                new(sms.Sender.Name, sms.Sender.Number),
                recipients,
                sms.Message,
                sms.Status,
                sms.CreatedAt));
        }

        return responses;
    }

    private async Task<(List<EmailMessage> Emails, List<SmsMessage> Sms)> GetFamilyCommunications(
        FamilyId familyId,
        CancellationToken cancellationToken)
    {
        Family? family = await _familyRepository.GetFamilyById(familyId, cancellationToken);

        if (family is null)
        {
            return ([], []);
        }

        Result<EmailAddress> emailAddress = EmailAddress.Create(family.FamilyEmail);

        if (emailAddress.IsFailure)
        {
            return ([], []);
        }

        List<EmailMessage> emails = await _emailRepository.GetByRecipient(emailAddress.Value, cancellationToken);

        return (emails, []);
    }

    private async Task<(List<EmailMessage> Emails, List<SmsMessage> Sms)> GetParentCommunications(
        ParentId parentId,
        CancellationToken cancellationToken)
    {
        Parent? parent = await _familyRepository.GetParentById(parentId, cancellationToken);

        if (parent is null)
        {
            return ([], []);
        }

        List<EmailMessage> emails = [];
        List<SmsMessage> sms = [];

        if (parent.EmailAddress != EmailAddress.None)
            emails = await _emailRepository.GetByRecipient(parent.EmailAddress, cancellationToken);

        if (parent.MobileNumber != PhoneNumber.Empty)
            sms = await _smsRepository.GetByNumber(parent.MobileNumber, cancellationToken);

        return (emails, sms);
    }

    private async Task<(List<EmailMessage> Emails, List<SmsMessage> Sms)> GetSchoolCommunications(
        SchoolCode schoolCode,
        CancellationToken cancellationToken)
    {
        School? school = await _schoolRepository.GetById(schoolCode, cancellationToken);

        if (school is null)
        {
            return ([], []);
        }

        Result<EmailAddress> emailAddress = EmailAddress.Create(school.EmailAddress);

        if (emailAddress.IsFailure)
        {
            return ([], []);
        }

        List<EmailMessage> emails = await _emailRepository.GetByRecipient(emailAddress.Value, cancellationToken);

        return (emails, []);
    }

    private async Task<(List<EmailMessage> Emails, List<SmsMessage> Sms)> GetSchoolContactCommunications(
        SchoolContactId contactId,
        CancellationToken cancellationToken)
    {
        SchoolContact? contact = await _contactRepository.GetById(contactId, cancellationToken);

        if (contact is null)
        {
            return ([], []);
        }

        List<EmailMessage> emails = [];
        List<SmsMessage> sms = [];

        if (contact.EmailAddress != EmailAddress.None)
            emails = await _emailRepository.GetByRecipient(contact.EmailAddress, cancellationToken);

        if (contact.PhoneNumber != PhoneNumber.Empty)
            sms = await _smsRepository.GetByNumber(contact.PhoneNumber, cancellationToken);

        return (emails, sms);
    }

    private async Task<(List<EmailMessage> Emails, List<SmsMessage> Sms)> GetStaffCommunications(
        StaffId staffId,
        CancellationToken cancellationToken)
    {
        StaffMember? staffMember = await _staffRepository.GetById(staffId, cancellationToken);

        if (staffMember is null)
        {
            return ([], []);
        }

        List<EmailMessage> emails = [];
        List<SmsMessage> sms = [];

        if (staffMember.EmailAddress != EmailAddress.None)
            emails = await _emailRepository.GetByRecipient(staffMember.EmailAddress, cancellationToken);

        if (staffMember.PhoneNumber != PhoneNumber.Empty)
            sms = await _smsRepository.GetByNumber(staffMember.PhoneNumber, cancellationToken);

        return (emails, sms);
    }

    private async Task<(List<EmailMessage> Emails, List<SmsMessage> Sms)> GetStudentCommunications(
        StudentId studentId,
        CancellationToken cancellationToken)
    {
        Student? student = await _studentRepository.GetById(studentId, cancellationToken);

        if (student is null)
        {
            return ([], []);
        }

        List<EmailMessage> emails = [];

        if (student.EmailAddress != EmailAddress.None)
            emails = await _emailRepository.GetByRecipient(student.EmailAddress, cancellationToken);

        return (emails, []);
    }
}