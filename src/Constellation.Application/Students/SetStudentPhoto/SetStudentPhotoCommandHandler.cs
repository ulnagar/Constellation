namespace Constellation.Application.Students.SetStudentPhoto;

using Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Models.Attachments.Repository;
using Constellation.Core.Models.Attachments.ValueObjects;
using Constellation.Core.Models.Students;
using Constellation.Core.Models.Students.Repositories;
using Core.Abstractions.Clock;
using Core.Abstractions.Services;
using Core.Errors;
using Core.Models.Attachments;
using Core.Models.Attachments.Services;
using Core.Models.Students.Errors;
using Core.Shared;
using Serilog;
using System;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;

internal sealed class SetStudentPhotoCommandHandler
: ICommandHandler<SetStudentPhotoCommand>
{
    private readonly IStudentRepository _studentRepository;
    private readonly IAttachmentRepository _attachmentRepository;
    private readonly IAttachmentService _attachmentService;
    private readonly IDateTimeProvider _dateTime;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public SetStudentPhotoCommandHandler(
        IStudentRepository studentRepository,
        IAttachmentRepository attachmentRepository,
        IAttachmentService attachmentService,
        IDateTimeProvider dateTime,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _studentRepository = studentRepository;
        _attachmentRepository = attachmentRepository;
        _attachmentService = attachmentService;
        _dateTime = dateTime;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
        _logger = logger
            .ForContext<SetStudentPhotoCommand>();
    }

    public async Task<Result> Handle(SetStudentPhotoCommand request, CancellationToken cancellationToken)
    {
        _logger
            .ForContext(nameof(SetStudentPhotoCommand), request, true)
            .Information("Requested to update Student Photo by user {User}", _currentUserService.UserName);

        Student student = await _studentRepository.GetById(request.StudentId, cancellationToken);

        if (student is null)
        {
            _logger
                .ForContext(nameof(SetStudentPhotoCommand), request, true)
                .ForContext(nameof(Error), StudentErrors.NotFound(request.StudentId), true)
                .Warning("Failed to update student photo by user {User}", _currentUserService.UserName);

            return Result.Failure(StudentErrors.NotFound(request.StudentId));
        }
        
        Attachment attachment = await _attachmentRepository.GetByTypeAndLinkId(AttachmentType.StudentPhoto, student.Id.ToString(), cancellationToken);

        try
        {
            bool newAttachment = false;

            if (attachment is null)
            {
                newAttachment = true;
                attachment = Attachment.CreateStudentPhotoAttachment(student.Name.SortOrder, MediaTypeNames.Image.Png, student.Id.ToString(), _dateTime.Now);
            }

            Result storage = await _attachmentService.StoreAttachmentData(attachment, request.Photo, true, cancellationToken);

            if (storage.IsFailure)
            {
                _logger
                    .ForContext(nameof(SetStudentPhotoCommand), request, true)
                    .ForContext(nameof(Error), storage.Error, true)
                    .Error("Failed to update student photo by user {User}", _currentUserService.UserName);

                return Result.Failure(storage.Error);
            }

            if (newAttachment)
                _attachmentRepository.Insert(attachment);

            await _unitOfWork.CompleteAsync(cancellationToken);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger
                .ForContext(nameof(SetStudentPhotoCommand), request, true)
                .ForContext(nameof(Exception), ex, true)
                .Error("Failed to update student photo by user {User}", _currentUserService.UserName);

            return Result.Failure(ApplicationErrors.UnknownError);
        }
    }
}