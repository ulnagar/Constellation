namespace Constellation.Infrastructure.Jobs;

using Application.Interfaces.Configuration;
using Application.Interfaces.Jobs;
using Application.Interfaces.Repositories;
using Constellation.Core.Models.Attachments.Repository;
using Constellation.Core.Models.Attachments.ValueObjects;
using Constellation.Core.Models.Awards;
using Constellation.Core.Models.Students;
using Core.Abstractions.Clock;
using Core.Abstractions.Repositories;
using Core.Models.Assignments;
using Core.Models.Assignments.Repositories;
using Core.Models.Attachments;
using Core.Models.Attachments.DTOs;
using Core.Models.Attachments.Services;
using Core.Models.Reports;
using Core.Models.Students.Repositories;
using Core.Shared;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;

internal sealed class AttachmentManagementJob : IAttachmentManagementJob
{
    private readonly AppConfiguration.AttachmentsConfiguration _configuration;
    private readonly IAttachmentRepository _attachmentRepository;
    private readonly IAttachmentService _attachmentService;
    private readonly IStudentRepository _studentRepository;
    private readonly IStudentAwardRepository _awardRepository;
    private readonly IAcademicReportRepository _reportRepository;
    private readonly IAssignmentRepository _assignmentRepository;
    private readonly IDateTimeProvider _dateTime;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public AttachmentManagementJob(
        IOptions<AppConfiguration> configuration,
        IAttachmentRepository attachmentRepository,
        IAttachmentService attachmentService,
        IStudentRepository studentRepository,
        IStudentAwardRepository awardRepository,
        IAcademicReportRepository reportRepository,
        IAssignmentRepository assignmentRepository,
        IDateTimeProvider dateTime,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _configuration = configuration.Value.Attachments;
        _attachmentRepository = attachmentRepository;
        _attachmentService = attachmentService;
        _studentRepository = studentRepository;
        _awardRepository = awardRepository;
        _reportRepository = reportRepository;
        _assignmentRepository = assignmentRepository;
        _dateTime = dateTime;
        _unitOfWork = unitOfWork;
        _logger = logger.ForContext<IAttachmentManagementJob>();
    }

    public async Task StartJob(Guid jobId, CancellationToken cancellationToken)
    {
        // Move oversize attachments from db to file system
        List<Attachment> forRelocation = await _attachmentRepository.GetSubsetOverSizeInDb(_configuration.MaxDBStoreSize, 10, cancellationToken);

        foreach (Attachment attachment in forRelocation)
        {
            if (attachment.FileData == Array.Empty<byte>())
                continue;

            _logger
                .Information("Processing file {filename} from attachment {id}", attachment.Name, attachment.Id.Value);

            Result attempt = await _attachmentService.StoreAttachmentData(attachment, attachment.FileData, true, cancellationToken);

            if (attempt.IsFailure)
            {
                _logger
                    .Warning("Failed to move file {filename} from Database to Disk", attachment.Name);
            }
            else
            {
                _logger
                    .ForContext(nameof(Attachment.FilePath), attachment.FilePath)
                    .Information("Successfully moved file {filename} from Database to Disk", attachment.Name);
            }

            await _unitOfWork.CompleteAsync(cancellationToken);
        }

        // Update attachments with missing filepath that are not stored in db
        List<Attachment> invalidItems = await _attachmentRepository.GetEmptyArrayItems(100, cancellationToken);

        foreach (Attachment attachment in invalidItems)
        {
            _logger
                .Information("Remediating file {filename} from attachment {id}", attachment.Name, attachment.Id.Value);

            Result attempt = await _attachmentService.RemediateEntry(attachment, cancellationToken);

            if (attempt.IsFailure)
            {
                _logger
                    .ForContext(nameof(Error), attempt.Error, true)
                    .Warning("Failed to remediate existing attachment \"{attachment}\"", attachment.Name);
            }

            await _unitOfWork.CompleteAsync(cancellationToken);
        }

        // Update checksum for files stored in db
        List<Attachment> withoutChecksum = await _attachmentRepository.GetSubsetLocallyStoredWithoutChecksum(1000, cancellationToken);
        SHA256 sha = SHA256.Create();

        foreach (Attachment attachment in withoutChecksum)
        {
            if (attachment.FileData == Array.Empty<byte>())
                continue;

            _logger
                .Information("Updating checksum for file {filename} from attachment {id}", attachment.Name, attachment.Id.Value);

            byte[] checksum = sha.ComputeHash(attachment.FileData);

            attachment.UpdateChecksum(BitConverter.ToString(checksum).Replace("-", string.Empty));

            await _unitOfWork.CompleteAsync(cancellationToken);
        }

        // Update checksum for files stored on disk
        withoutChecksum = await _attachmentRepository.GetSubsetExternallyStoredWithoutChecksum(1000, cancellationToken);

        foreach (Attachment attachment in withoutChecksum)
        {
            Result<AttachmentResponse> file = await _attachmentService.GetAttachmentFile(attachment.LinkType, attachment.LinkId, cancellationToken);

            if (file.IsFailure)
            {
                _logger
                    .ForContext(nameof(Error), file.Error, true)
                    .Warning("Failed to update checksum for external attachment \"{attachment}\"", attachment.Name);
            }

            if (file.Value.FileData == Array.Empty<byte>())
                continue;

            _logger
                .Information("Updating checksum for file {filename} from attachment {id}", attachment.Name, attachment.Id.Value);

            byte[] checksum = sha.ComputeHash(file.Value.FileData);

            attachment.UpdateChecksum(BitConverter.ToString(checksum).Replace("-", string.Empty));

            await _unitOfWork.CompleteAsync(cancellationToken);
        }

        // Delete expired attachments
        // E.g. Canvas Assignment Submissions > 18 months old
        // or Student Reports and Student Awards for withdrawn students
        if (_dateTime.Today.Day != 1)
            return;

        List<Student> students = await _studentRepository.GetInactiveStudents(cancellationToken);

        foreach (var student in students)
        {
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

            List<AcademicReport> reportRecords = await _reportRepository.GetForStudent(student.Id, cancellationToken);

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

        List<CanvasAssignment> assignments = await _assignmentRepository.GetForCleanup(cancellationToken);

        foreach (CanvasAssignment assignment in assignments)
        {
            foreach (var submission in assignment.Submissions)
            {
                Attachment assignmentAttachment = await _attachmentRepository.GetByTypeAndLinkId(AttachmentType.CanvasAssignmentSubmission, submission.Id.ToString(), cancellationToken);

                if (assignmentAttachment is not null)
                {
                    _logger
                        .Information("Removing Canvas assignment submission attachment ({id}) for expired assignment {assignment}", assignmentAttachment.Id, assignment.Name);

                    _attachmentService.DeleteAttachment(assignmentAttachment);
                }

                await _unitOfWork.CompleteAsync(cancellationToken);
            }
        }
    }
}