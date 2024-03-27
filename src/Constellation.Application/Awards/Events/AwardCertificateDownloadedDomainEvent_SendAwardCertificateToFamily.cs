namespace Constellation.Application.Awards.Events;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Services;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Models.Awards.Events;
using Constellation.Core.Models.Students.Repositories;
using Constellation.Core.ValueObjects;
using Core.Abstractions.Clock;
using Core.Models;
using Core.Models.Attachments.DTOs;
using Core.Models.Attachments.Services;
using Core.Models.Attachments.ValueObjects;
using Core.Models.Awards;
using Core.Models.Families;
using Core.Models.StaffMembers.Repositories;
using Core.Models.Students;
using Core.Shared;
using Serilog;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;

internal sealed class AwardCertificateDownloadedDomainEvent_SendAwardCertificateToFamily
    : IDomainEventHandler<AwardCertificateDownloadedDomainEvent>
{
    private readonly IStudentAwardRepository _awardRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly IAttachmentService _attachmentService;
    private readonly IStaffRepository _staffRepository;
    private readonly IFamilyRepository _familyRepository;
    private readonly IEmailService _emailService;
    private readonly IDateTimeProvider _dateTime;
    private readonly ILogger _logger;

    public AwardCertificateDownloadedDomainEvent_SendAwardCertificateToFamily(
        IStudentAwardRepository awardRepository,
        IStudentRepository studentRepository,
        IAttachmentService attachmentService,
        IStaffRepository staffRepository,
        IFamilyRepository familyRepository,
        IEmailService emailService,
        IDateTimeProvider dateTime,
        ILogger logger)
    {
        _awardRepository = awardRepository;
        _studentRepository = studentRepository;
        _attachmentService = attachmentService;
        _staffRepository = staffRepository;
        _familyRepository = familyRepository;
        _emailService = emailService;
        _dateTime = dateTime;
        _logger = logger.ForContext<AwardCertificateDownloadedDomainEvent>();
    }
    public async Task Handle(AwardCertificateDownloadedDomainEvent notification, CancellationToken cancellationToken)
    {
        // Gather details
        StudentAward award = await _awardRepository.GetById(notification.AwardId, cancellationToken);

        if (award is null)
        {
            _logger
                .ForContext(nameof(AwardCertificateDownloadedDomainEvent), notification, true)
                .Warning("Could not retrieve award when attempting to send certificate to parents");

            return;
        }

        if (award.AwardedOn.Date < _dateTime.Now.Date.AddDays(-1))
        {
            _logger
                .ForContext(nameof(AwardCertificateDownloadedDomainEvent), notification, true)
                .Warning("Cancelling send of award certificate as the awarded date is too old");

            return;
        }

        List<Family> families = await _familyRepository.GetFamiliesByStudentId(award.StudentId, cancellationToken);

        if (families.Count == 0)
        {
            _logger
                .ForContext(nameof(AwardCertificateDownloadedDomainEvent), notification, true)
                .Warning("Could not retrieve family details when attempting to send certificate to parents");

            return;
        }

        List<Parent> parents = families.SelectMany(family => family.Parents).ToList();

        List<EmailRecipient> recipients = new();

        foreach (Parent parent in parents)
        {
            if (string.IsNullOrWhiteSpace(parent.EmailAddress))
                continue;

            Result<Name> name = Name.Create(parent.FirstName, string.Empty, parent.LastName);

            string parentName = name.IsFailure ? $"{parent.FirstName} {parent.LastName}" : name.Value.DisplayName;

            Result<EmailRecipient> recipient = EmailRecipient.Create(parentName, parent.EmailAddress);

            if (recipient.IsFailure)
            {
                _logger
                    .ForContext(nameof(AwardCertificateDownloadedDomainEvent), notification, true)
                    .ForContext(nameof(Parent), parent, true)
                    .Warning("Failed to create email recipient for parent");

                continue;
            }

            recipients.Add(recipient.Value);
        }

        Result<AttachmentResponse> fileRequest = await _attachmentService.GetAttachmentFile(
            AttachmentType.AwardCertificate,
            award.Id.ToString(), 
            cancellationToken);

        if (fileRequest.IsFailure)
        {
            _logger
                .ForContext(nameof(AwardCertificateDownloadedDomainEvent), notification, true)
                .Warning("Could not retrieve certificate while attempting to send certificate to parents");

            return;
        }

        MemoryStream stream = new(fileRequest.Value.FileData);

        Attachment attachment = new(stream, fileRequest.Value.FileName, fileRequest.Value.FileType);

        Student student = await _studentRepository.GetById(award.StudentId, cancellationToken);

        if (student is null)
        {
            _logger
                .ForContext(nameof(AwardCertificateDownloadedDomainEvent), notification, true)
                .Warning("Failed to retrieve student while attempting to send certificate to parents");

            return;
        }

        Staff teacher = await _staffRepository.GetById(award.TeacherId, cancellationToken);

        if (teacher is null)
        {
            _logger
                .ForContext(nameof(AwardCertificateDownloadedDomainEvent), notification, true)
                .Warning("Failed to retrieve teacher while attempting to send certificate to parents");

            return;
        }

        await _emailService.SendAwardCertificateParentEmail(
            recipients,
            attachment,
            award,
            student,
            teacher,
            cancellationToken);
    }
}
