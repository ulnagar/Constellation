namespace Constellation.Infrastructure.Jobs;

using Constellation.Application.Interfaces.Gateways;
using Constellation.Application.Interfaces.Jobs;
using Core.Extensions;
using Core.Models.Attachments;
using Core.Models.Attachments.Repository;
using Core.Models.Attachments.Services;
using Core.Models.Attachments.ValueObjects;
using Core.Models.Students;
using Core.Models.Students.Repositories;
using Core.Shared;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class SentralPhotoSyncJob : ISentralPhotoSyncJob
{
    private readonly IAttachmentService _attachmentService;
    private readonly IAttachmentRepository _attachmentRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly ISentralGateway _gateway;
    private readonly ILogger _logger;

    public SentralPhotoSyncJob(
        IAttachmentService attachmentService,
        IAttachmentRepository attachmentRepository,
        IStudentRepository studentRepository,
        ISentralGateway gateway, 
        ILogger logger)
    {
        _attachmentService = attachmentService;
        _attachmentRepository = attachmentRepository;
        _studentRepository = studentRepository;
        _gateway = gateway;
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

            byte[] photo = await _gateway.GetSentralStudentPhoto(student.StudentReferenceNumber.Number);

            Attachment attachment = await _attachmentRepository.GetByTypeAndLinkId(AttachmentType.StudentPhoto, student.Id.ToString(), token);

            try
            {
                _logger.Information("{id}: Found new photo for {student} ({grade})", jobId, student.Name.DisplayName, student.CurrentEnrolment?.Grade.AsName());

                Result storage = await _attachmentService.StoreAttachmentData(attachment, photo, true, token);

                if (storage.IsFailure)
                {
                    _logger
                        .ForContext(nameof(Error), storage.Error, true)
                        .Error("{id}: Failed to update student photo for {student} ({grade})", jobId, student.Name.DisplayName, student.CurrentEnrolment?.Grade.AsName());
                }

            }
            catch (Exception ex)
            {
                _logger.Error("{id}: Failed to check student photo for {student} ({grade}) due to error {error}", jobId, student.Name.DisplayName, student.CurrentEnrolment?.Grade.AsName(), ex.Message);
            }
        }
    }
}