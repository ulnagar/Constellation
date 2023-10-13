namespace Constellation.Application.Awards.Events;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Interfaces.Services;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.DomainEvents;
using Constellation.Core.ValueObjects;
using Core.Models.Attachments.DTOs;
using Core.Models.Attachments.Services;
using Core.Models.Attachments.ValueObjects;
using Core.Shared;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;

internal sealed class AwardCreatedDomainEvent_SendAwardCertificateToFamily
    : IDomainEventHandler<AwardCreatedDomainEvent>
{
    private readonly IStudentAwardRepository _awardRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly IAttachmentService _attachmentService;
    private readonly IStaffRepository _staffRepository;
    private readonly IFamilyRepository _familyRepository;
    private readonly IEmailService _emailService;
    private readonly ILogger _logger;

    public AwardCreatedDomainEvent_SendAwardCertificateToFamily(
        IStudentAwardRepository awardRepository,
        IStudentRepository studentRepository,
        IAttachmentService attachmentService,
        IStaffRepository staffRepository,
        IFamilyRepository familyRepository,
        IEmailService emailService,
        ILogger logger)
    {
        _awardRepository = awardRepository;
        _studentRepository = studentRepository;
        _attachmentService = attachmentService;
        _staffRepository = staffRepository;
        _familyRepository = familyRepository;
        _emailService = emailService;
        _logger = logger.ForContext<AwardCreatedDomainEvent>();
    }
    public async Task Handle(AwardCreatedDomainEvent notification, CancellationToken cancellationToken)
    {
        // Gather details
        var award = await _awardRepository.GetById(notification.AwardId, cancellationToken);

        if (award is null)
        {
            _logger.Error("Could not retrieve award when attempting to send certificate to parents: {@notification}", notification);
            return;
        }

        if (award.AwardedOn.Date < DateTime.Today.AddDays(-1))
        {
            _logger.Warning("Cancelling send of award certificate as the awarded date is too old: {@award}", award);

            return;
        }

        var families = await _familyRepository.GetFamiliesByStudentId(award.StudentId, cancellationToken);

        if (families.Count == 0)
        {
            _logger.Error("Could not retrieve family details when attempting to send certificate to parents: {@award}", award);
            return;
        }

        var parents = families.SelectMany(family => family.Parents).ToList();

        List<EmailRecipient> recipients = new();

        foreach (var parent in parents)
        {
            if (string.IsNullOrWhiteSpace(parent.EmailAddress))
                continue;

            var name = Name.Create(parent.FirstName, string.Empty, parent.LastName);

            var parentName = name.IsFailure ? $"{parent.FirstName} {parent.LastName}" : name.Value.DisplayName;

            var recipient = EmailRecipient.Create(parentName, parent.EmailAddress);

            if (recipient.IsFailure)
            {
                _logger.Warning("Failed to create email recipient for parent {@parent}", parent);
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
            _logger.Error("Could not retrieve certificate while attempting to send certificate to parents: {@award}", award);
            return;
        }

        MemoryStream stream = new(fileRequest.Value.FileData);

        Attachment attachment = new(stream, fileRequest.Value.FileName, fileRequest.Value.FileType);

        var student = await _studentRepository.GetById(award.StudentId, cancellationToken);

        if (student is null)
        {
            _logger.Warning("Failed to retrieve student while attempting to send certificate to parents: {@award}", award);
        }

        var teacher = await _staffRepository.GetById(award.TeacherId, cancellationToken);

        if (teacher is null)
        {
            _logger.Warning("Failed to retrieve teacher while attempting to send certificate to parents: {@award}", award);
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
