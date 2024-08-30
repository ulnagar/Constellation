namespace Constellation.Application.Reports.Events;

using Abstractions.Messaging;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Models.Students.Repositories;
using Core.DomainEvents;
using Core.Models.Attachments.DTOs;
using Core.Models.Attachments.Services;
using Core.Models.Attachments.ValueObjects;
using Core.Models.Families;
using Core.Models.Reports;
using Core.Models.Students;
using Core.Shared;
using Core.ValueObjects;
using DTOs;
using Interfaces.Services;
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
        ILogger logger)
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
        AcademicReport reportEntry = await _reportRepository.GetById(notification.ReportId, cancellationToken);

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
        List<Family> families = await _familyRepository.GetFamiliesByStudentId(reportEntry.StudentId, cancellationToken);

        if (families is null || families.Count == 0)
            return;

        // Are there any non-residential families
        List<Parent> nonResidentParents = families
            .Where(family => 
                family.Students.Any(student => 
                    student.StudentId == reportEntry.StudentId && 
                    student.IsResidentialFamily))
            .SelectMany(family => family.Parents)
            .ToList();

        if (nonResidentParents.Count == 0)
            return;

        List<EmailRecipient> recipients = new();

        foreach (Parent parent in nonResidentParents)
        {
            // Email the parent a copy of the report

            Result<Name> name = Name.Create(parent.FirstName, null, parent.LastName);

            if (name.IsFailure)
                continue;

            Result<EmailAddress> email = EmailAddress.Create(parent.EmailAddress);

            if (email.IsFailure)
                continue;

            Result<EmailRecipient> recipient = EmailRecipient.Create(name.Value.DisplayName, email.Value.Email);

            if (recipient.IsFailure)
                continue;

            recipients.Add(recipient.Value);
        }

        Student student = await _studentRepository.GetById(reportEntry.StudentId, cancellationToken);

        FileDto fileDto = new()
        {
            FileData = fileRequest.Value.FileData,
            FileType = fileRequest.Value.FileType,
            FileName = fileRequest.Value.FileName
        };

        if (recipients.Count > 0)
            await _emailService.SendAcademicReportToNonResidentialParent(recipients, student.Name, reportEntry.ReportingPeriod, reportEntry.Year, fileDto, cancellationToken);
    }
}
