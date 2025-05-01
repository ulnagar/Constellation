namespace Constellation.Application.Domains.Attachments.Commands.EmailExternalReports;

using Abstractions.Messaging;
using Core.Abstractions.Repositories;
using Core.Models.Attachments;
using Core.Models.Attachments.DTOs;
using Core.Models.Attachments.Repository;
using Core.Models.Attachments.Services;
using Core.Models.Attachments.ValueObjects;
using Core.Models.Families;
using Core.Models.Families.Errors;
using Core.Models.Reports;
using Core.Models.Reports.Repositories;
using Core.Models.Students;
using Core.Models.Students.Errors;
using Core.Models.Students.Identifiers;
using Core.Models.Students.Repositories;
using Core.Shared;
using Core.ValueObjects;
using Interfaces.Gateways;
using Interfaces.Repositories;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class EmailExternalReportsCommandHandler
: ICommandHandler<EmailExternalReportsCommand>
{
    private readonly IReportRepository _reportRepository;
    private readonly IAttachmentRepository _attachmentRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly IFamilyRepository _familyRepository;
    private readonly IAttachmentService _attachmentService;
    private readonly IEmailGateway _emailGateway;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public EmailExternalReportsCommandHandler(
        IReportRepository reportRepository,
        IAttachmentRepository attachmentRepository,
        IStudentRepository studentRepository,
        IFamilyRepository familyRepository,
        IAttachmentService attachmentService,
        IEmailGateway emailGateway,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _reportRepository = reportRepository;
        _attachmentRepository = attachmentRepository;
        _studentRepository = studentRepository;
        _familyRepository = familyRepository;
        _attachmentService = attachmentService;
        _emailGateway = emailGateway;
        _unitOfWork = unitOfWork;
        _logger = logger
            .ForContext<EmailExternalReportsCommand>();
    }

    public async Task<Result> Handle(EmailExternalReportsCommand request, CancellationToken cancellationToken)
    {
        List<TempExternalReport> reports = await _reportRepository.GetTempExternalReports(cancellationToken);

        if (reports.Count == 0)
            return Result.Success();

        List<Student> students = await _studentRepository.GetCurrentStudents(cancellationToken);

        if (students.Count == 0)
        {
            _logger
                .ForContext(nameof(EmailExternalReportsCommand), request, true)
                .ForContext(nameof(Error), StudentErrors.NoneFoundFilter, true)
                .Warning("Failed to send External Report emails to parents");

            return Result.Failure(StudentErrors.NoneFoundFilter);
        }

        List<Family> families = await _familyRepository.GetAllCurrent(cancellationToken);

        if (families.Count == 0)
        {
            _logger
                .ForContext(nameof(EmailExternalReportsCommand), request, true)
                .ForContext(nameof(Error), FamilyErrors.NoneFound, true)
                .Warning("Failed to send External Report emails to parents");

            return Result.Failure(FamilyErrors.NoneFound);
        }

        List<Attachment> attachments = await _attachmentRepository.GetTempFiles(cancellationToken);

        foreach (TempExternalReport report in reports)
        {
            if (report.StudentId == StudentId.Empty)
                continue;

            Student? student = students.FirstOrDefault(entry => entry.Id == report.StudentId);

            if (student is null)
                continue;

            List<Family> studentFamilies = families
                .Where(entry => 
                    entry.Students.Any(record => 
                        record.StudentId == student.Id))
                .ToList();

            if (studentFamilies.Count == 0)
                continue;

            Attachment? attachment = attachments.FirstOrDefault(entry => entry.LinkId == report.Id.ToString());

            if (attachment is null)
                continue;

            Result<AttachmentResponse> fileData = await _attachmentService.GetAttachmentFile(AttachmentType.TempFile, report.Id.ToString(), cancellationToken);

            if (fileData.IsFailure)
                continue;

            MemoryStream fileStream = new(fileData.Value.FileData);
            System.Net.Mail.Attachment emailAttachment = new(fileStream, fileData.Value.FileName, fileData.Value.FileType);

            // Convert to External Report
            ExternalReport externalReport = ExternalReport.ConvertFromTempExternalReport(report);
            Attachment newAttachment = Attachment.CreateExternalReportAttachment(attachment.Name, attachment.FileType, externalReport.Id.ToString(), attachment.CreatedAt);

            Result attempt = await _attachmentService.StoreAttachmentData(newAttachment, fileData.Value.FileData, true, cancellationToken);

            if (attempt.IsFailure)
            {
                _logger
                    .ForContext(nameof(EmailExternalReportsCommand), request, true)
                    .ForContext(nameof(TempExternalReport), report, true)
                    .ForContext(nameof(Error), attempt.Error, true)
                    .Warning("Failed to send External Report emails to parents");

                continue;
            }

            _attachmentRepository.Insert(newAttachment);
            _reportRepository.Insert(externalReport);
            _attachmentService.DeleteAttachment(attachment);
            _reportRepository.Remove(report);

            await _unitOfWork.CompleteAsync(cancellationToken);

            // Email to parents
            List<EmailRecipient> recipients = new();
            foreach (Parent parent in studentFamilies.SelectMany(entry => entry.Parents))
            {
                Result<EmailRecipient> recipient = EmailRecipient.Create($"{parent.FirstName} {parent.LastName}", parent.EmailAddress);

                if (recipient.IsFailure)
                    continue;

                if (recipients.All(entry => entry.Email != recipient.Value.Email))
                    recipients.Add(recipient.Value);
            }

            foreach (Family family in studentFamilies)
            {
                Result<EmailRecipient> recipient = EmailRecipient.Create(family.FamilyTitle, family.FamilyEmail);

                if (recipient.IsFailure)
                    continue;

                if (recipients.All(entry => entry.Email != recipient.Value.Email))
                    recipients.Add(recipient.Value);
            }

            foreach (EmailRecipient recipient in recipients)
            {
                string subject = request.Subject.Replace("::parent_name::", recipient.Name, StringComparison.CurrentCultureIgnoreCase);
                subject = subject.Replace("::report_type::", report.Type.ToString(), StringComparison.CurrentCultureIgnoreCase);
                subject = subject.Replace("::report_month::", report.IssuedDate.ToString("MMM yyyy"), StringComparison.CurrentCultureIgnoreCase);

                string body = request.Body.Replace("::parent_name::", recipient.Name, StringComparison.CurrentCultureIgnoreCase);
                body = body.Replace("::report_type::", report.Type.ToString(), StringComparison.CurrentCultureIgnoreCase);
                body = body.Replace("::report_month::", report.IssuedDate.ToString("MMM yyyy"), StringComparison.CurrentCultureIgnoreCase);

                // Send email with subject and body
                await _emailGateway.Send([recipient], "auroracoll-h.school@det.nsw.edu.au", subject, body, [emailAttachment], cancellationToken);
            }
        }

        return Result.Success();
    }
}
