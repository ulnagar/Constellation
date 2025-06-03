namespace Constellation.Infrastructure.Jobs;

using Application.Interfaces.Repositories;
using Constellation.Application.Interfaces.Gateways;
using Constellation.Application.Interfaces.Jobs;
using Constellation.Core.Enums;
using Constellation.Core.Primitives;
using Core.Extensions;
using Core.Models.Attachments;
using Core.Models.Attachments.Repository;
using Core.Models.Attachments.Services;
using Core.Models.Attachments.ValueObjects;
using Core.Models.Students;
using Core.Models.Students.Enums;
using Core.Models.Students.Repositories;
using Core.Models.Students.ValueObjects;
using Core.Shared;
using System;
using System.Linq;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;

internal sealed class SentralPhotoSyncJob : ISentralPhotoSyncJob
{
    private readonly IAttachmentService _attachmentService;
    private readonly IAttachmentRepository _attachmentRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly ISentralGateway _gateway;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public SentralPhotoSyncJob(
        IAttachmentService attachmentService,
        IAttachmentRepository attachmentRepository,
        IStudentRepository studentRepository,
        ISentralGateway gateway,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _attachmentService = attachmentService;
        _attachmentRepository = attachmentRepository;
        _studentRepository = studentRepository;
        _gateway = gateway;
        _unitOfWork = unitOfWork;
        _logger = logger.ForContext<ISentralPhotoSyncJob>();
    }

    public async Task StartJob(Guid jobId, CancellationToken token)
    {
        List<Student> students = await _studentRepository.GetCurrentStudents(token);

        foreach (Student student in students.OrderBy(student => student.CurrentEnrolment?.Grade).ThenBy(student => student.Name.SortOrder))
        {
            if (token.IsCancellationRequested)
                return;

            _logger.Information("{id}: Checking student {student} ({grade}) for photo", jobId, student.Name.DisplayName, student.CurrentEnrolment?.Grade.AsName());

            if (student.StudentReferenceNumber is null ||
                student.StudentReferenceNumber == StudentReferenceNumber.Empty)
            {
                _logger
                    .Warning("{id}: No student identifier found for student {student} ({grade})", jobId, student.Name.DisplayName, student.CurrentEnrolment?.Grade.AsName());
                
                continue;
            }
            
            SystemLink sentralId = student.SystemLinks.FirstOrDefault(link => link.System == SystemType.Sentral);

            if (sentralId is null)
                continue;

            IndigenousStatus status = await _gateway.GetStudentIndigenousStatus(sentralId.Value);

            if (student.IndigenousStatus != status && status != IndigenousStatus.Unknown)
            {
                student.UpdateIndigenousStatus(status);

                await _unitOfWork.CompleteAsync(token);
            }

            byte[] photo = await _gateway.GetSentralStudentPhotoFromApi(sentralId.Value, token);

            if (photo.Length == 0)
                continue;

            Attachment attachment = await _attachmentRepository.GetByTypeAndLinkId(AttachmentType.StudentPhoto, student.Id.ToString(), token);

            try
            {
                _logger.Information("{id}: Found photo for {student} ({grade})", jobId, student.Name.DisplayName, student.CurrentEnrolment?.Grade.AsName());

                bool newAttachment = false;

                if (attachment is null)
                {
                    newAttachment = true;
                    attachment = Attachment.CreateStudentPhotoAttachment(student.Name.SortOrder, MediaTypeNames.Image.Jpeg, student.Id.ToString(), DateTime.Now);
                }

                Result storage = await _attachmentService.StoreAttachmentData(attachment, photo, true, token);

                if (storage.IsFailure)
                {
                    _logger
                        .ForContext(nameof(Error), storage.Error, true)
                        .Error("{id}: Failed to update student photo for {student} ({grade})", jobId, student.Name.DisplayName, student.CurrentEnrolment?.Grade.AsName());
                }

                if (newAttachment)
                    _attachmentRepository.Insert(attachment);

                await _unitOfWork.CompleteAsync(token);
            }
            catch (Exception ex)
            {
                _logger.Error("{id}: Failed to check student photo for {student} ({grade}) due to error {error}", jobId, student.Name.DisplayName, student.CurrentEnrolment?.Grade.AsName(), ex.Message);
            }
        }
    }
}