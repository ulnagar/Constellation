namespace Constellation.Application.Domains.Students.Events.StudentWithdrawnDomainEvent;

using Abstractions.Messaging;
using Constellation.Core.Models.Reports.Repositories;
using Core.Abstractions.Repositories;
using Core.Models.Attachments;
using Core.Models.Attachments.Repository;
using Core.Models.Attachments.Services;
using Core.Models.Attachments.ValueObjects;
using Core.Models.Awards;
using Core.Models.Reports;
using Core.Models.Students;
using Core.Models.Students.Errors;
using Core.Models.Students.Events;
using Core.Models.Students.Repositories;
using Core.Shared;
using Interfaces.Repositories;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class RemoveAttachments
    : IDomainEventHandler<StudentWithdrawnDomainEvent>
{
    private readonly IStudentRepository _studentRepository;
    private readonly IAttachmentService _attachmentService;
    private readonly IAttachmentRepository _attachmentRepository;
    private readonly IStudentAwardRepository _awardRepository;
    private readonly IReportRepository _reportRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public RemoveAttachments(
        IStudentRepository studentRepository,
        IAttachmentService attachmentService,
        IAttachmentRepository attachmentRepository,
        IStudentAwardRepository awardRepository,
        IReportRepository reportRepository,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _studentRepository = studentRepository;
        _attachmentService = attachmentService;
        _attachmentRepository = attachmentRepository;
        _awardRepository = awardRepository;
        _reportRepository = reportRepository;
        _unitOfWork = unitOfWork;
        _logger = logger
            .ForContext<StudentWithdrawnDomainEvent>();
    }

    public async Task Handle(StudentWithdrawnDomainEvent notification, CancellationToken cancellationToken)
    {
        Student student = await _studentRepository.GetById(notification.StudentId, cancellationToken);

        if (student is null)
        {
            _logger
                .ForContext(nameof(StudentWithdrawnDomainEvent), notification, true)
                .ForContext(nameof(Error), StudentErrors.NotFound(notification.StudentId), true)
                .Warning("Failed to remove attachments for withdrawn Student");

            return;
        }

        Attachment photoAttachment = await _attachmentRepository.GetByTypeAndLinkId(AttachmentType.StudentPhoto, student.Id.ToString(), cancellationToken);

        if (photoAttachment is not null)
        {
            _logger
                .Information("Removing photo attachment ({id}) for withdrawn student {student}", photoAttachment.Id, student.Name.DisplayName);

            _attachmentService.DeleteAttachment(photoAttachment);

            await _unitOfWork.CompleteAsync(cancellationToken);
        }

        List<StudentAward> awardRecords = await _awardRepository.GetByStudentId(student.Id, cancellationToken);

        foreach (StudentAward awardRecord in awardRecords.Where(entry => entry.Type == StudentAward.Astra))
        {
            Attachment awardAttachment = await _attachmentRepository.GetByTypeAndLinkId(AttachmentType.AwardCertificate, awardRecord.Id.ToString(), cancellationToken);

            if (awardAttachment is not null)
            {
                _logger
                    .Information("Removing award certificate attachment ({id}) for withdrawn student {student}", awardAttachment.Id, student.Name.DisplayName);

                _attachmentService.DeleteAttachment(awardAttachment);
                await _unitOfWork.CompleteAsync(cancellationToken);
            }
        }

        List<AcademicReport> reportRecords = await _reportRepository.GetAcademicReportsForStudent(student.Id, cancellationToken);

        foreach (AcademicReport reportRecord in reportRecords)
        {
            Attachment reportAttachment = await _attachmentRepository.GetByTypeAndLinkId(AttachmentType.StudentReport, reportRecord.Id.ToString(), cancellationToken);

            if (reportAttachment is not null)
            {
                _logger
                    .Information("Removing academic report attachment ({id}) for withdrawn student {student}", reportAttachment.Id, student.Name.DisplayName);

                _attachmentService.DeleteAttachment(reportAttachment);
            }

            _reportRepository.Remove(reportRecord);
            await _unitOfWork.CompleteAsync(cancellationToken);
        }
    }
}