namespace Constellation.Application.Reports.Events;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.DTOs;
using Constellation.Application.Interfaces.Services;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.DomainEvents;
using Constellation.Core.Models.Students.Repositories;
using Constellation.Core.ValueObjects;
using Core.Models.Attachments.DTOs;
using Core.Models.Attachments.Services;
using Core.Models.Attachments.ValueObjects;
using Core.Shared;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class AcademicReportCreatedDomainEvent_EmailToNonResidentialParents
    : IDomainEventHandler<AcademicReportCreatedDomainEvent>
{
    private readonly IAcademicReportRepository _reportRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly IFamilyRepository _familyRepository;
    private readonly IAttachmentService _attachmentService;
    private readonly IEmailService _emailService;
    private readonly ILogger _logger;

    public AcademicReportCreatedDomainEvent_EmailToNonResidentialParents(
        IAcademicReportRepository reportRepository,
        IStudentRepository studentRepository,
        IFamilyRepository familyRepository,
        IAttachmentService attachmentService,
        IEmailService emailService,
        Serilog.ILogger logger)
    {
        _reportRepository = reportRepository;
        _studentRepository = studentRepository;
        _familyRepository = familyRepository;
        _attachmentService = attachmentService;
        _emailService = emailService;
        _logger = logger.ForContext<AcademicReportCreatedDomainEvent>();
    }

    public async Task Handle(AcademicReportCreatedDomainEvent notification, CancellationToken cancellationToken)
    {
        var reportEntry = await _reportRepository.GetById(notification.ReportId, cancellationToken);

        if (reportEntry is null)
        {
            _logger.Warning("Could not find report while attempting to send to non-residential family: {@notification}", notification);
            return;
        }

        Result<AttachmentResponse> fileRequest = await _attachmentService.GetAttachmentFile(
            AttachmentType.StudentReport, 
            reportEntry.Id.ToString(),
            cancellationToken);

        if (fileRequest.IsFailure)
        {
            _logger.Warning("Could not find report while attempting to send report to non-residential family: {@notification}", notification);
            return;
        }

        // Get Student family and check for non-residential contacts
        var families = await _familyRepository.GetFamiliesByStudentId(reportEntry.StudentId, cancellationToken);

        if (families is null || families.Count == 0)
            return;

        // Are there any non-residential families
        var nonResidentParents = families
            .Where(family => 
                family.Students.Any(student => 
                    student.StudentId == reportEntry.StudentId && 
                    student.IsResidentialFamily))
            .SelectMany(family => family.Parents)
            .ToList();

        if (nonResidentParents.Count == 0)
            return;

        List<EmailRecipient> recipients = new();

        foreach (var parent in nonResidentParents)
        {
            // Email the parent a copy of the report

            var name = Name.Create(parent.FirstName, null, parent.LastName);

            if (name.IsFailure)
                continue;

            var email = EmailAddress.Create(parent.EmailAddress);

            if (email.IsFailure)
                continue;

            var recipient = EmailRecipient.Create(name.Value.DisplayName, email.Value.Email);

            if (recipient.IsFailure)
                continue;

            recipients.Add(recipient.Value);
        }

        var student = await _studentRepository.GetById(reportEntry.StudentId, cancellationToken);

        var studentName = Name.Create(student.FirstName, null, student.LastName);

        var fileDto = new FileDto
        {
            FileData = fileRequest.Value.FileData,
            FileType = fileRequest.Value.FileType,
            FileName = fileRequest.Value.FileName
        };

        if (recipients.Count > 0)
            await _emailService.SendAcademicReportToNonResidentialParent(recipients, studentName.Value, reportEntry.ReportingPeriod, reportEntry.Year, fileDto, cancellationToken);
    }
}
