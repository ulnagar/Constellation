namespace Constellation.Application.Domains.Assessments.Assessments.Commands.AddSubmissionToAssessment;

using Abstractions.Messaging;
using Constellation.Application.Models.Identity.Errors;
using Constellation.Core.Models.Auth;
using Core.Abstractions.Clock;
using Core.Abstractions.Services;
using Core.Models.Assessments;
using Core.Models.Assessments.Errors;
using Core.Models.Assessments.Identifiers;
using Core.Models.Assessments.Repositories;
using Core.Models.Attachments;
using Core.Models.Attachments.Repository;
using Core.Models.Attachments.Services;
using Core.Shared;
using Interfaces.Repositories;
using Microsoft.AspNetCore.Identity;
using Serilog;

internal sealed class AddSubmissionToAssessmentCommandHandler
: ICommandHandler<AddSubmissionToAssessmentCommand>
{
    private readonly IAssessmentRepository _assessmentRepository;
    private readonly IAttachmentRepository _attachmentRepository;
    private readonly IAttachmentService _attachmentService;
    private readonly UserManager<AppUser> _userManager;
    private readonly IDateTimeProvider _dateTime;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public AddSubmissionToAssessmentCommandHandler(
        IAssessmentRepository assessmentRepository,
        IAttachmentRepository attachmentRepository,
        IAttachmentService attachmentService,
        UserManager<AppUser> userManager,
        IDateTimeProvider dateTime,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        ILogger logger)
    {
        _assessmentRepository = assessmentRepository;
        _attachmentRepository = attachmentRepository;
        _attachmentService = attachmentService;
        _userManager = userManager;
        _dateTime = dateTime;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _logger = logger
            .ForContext<AddSubmissionToAssessmentCommand>();
    }

    public async Task<Result> Handle(AddSubmissionToAssessmentCommand request, CancellationToken cancellationToken)
    {
        Assessment? assessment = await _assessmentRepository.GetAssessmentById(request.AssessmentId, cancellationToken);

        if (assessment is null)
        {
            _logger
                .ForContext(nameof(AddSubmissionToAssessmentCommand), request, true)
                .ForContext(nameof(Error), AssessmentErrors.NotFound(request.AssessmentId), true)
                .Warning("Failed to record Assessment Submission");

            return Result.Failure(AssessmentErrors.NotFound(request.AssessmentId));
        }

        AppUser? user = await _userManager.FindByNameAsync(_currentUserService.EmailAddress);

        if (user is null)
        {
            _logger
                .ForContext(nameof(AddSubmissionToAssessmentCommand), request, true)
                .ForContext(nameof(Error), AuthErrors.UserNotFound(Guid.Empty), true)
                .Warning("Failed to record Assessment Submission");

            return Result.Failure(AuthErrors.UserNotFound(Guid.Empty));
        }

        Result<SubmissionId> submissionResult = assessment.AddStudentSubmission(request.StudentId, user);

        if (submissionResult.IsFailure)
        {
            _logger
                .ForContext(nameof(AddSubmissionToAssessmentCommand), request, true)
                .ForContext(nameof(Error), submissionResult.Error, true)
                .Warning("Failed to record Assessment Submission");

            return Result.Failure(submissionResult.Error);
        }

        Attachment fileEntity = Attachment.CreateAssessmentSubmissionAttachment(
            request.File.FileName,
            request.File.FileType,
            submissionResult.Value.ToString(),
            _dateTime.Now);

        Result attempt = await _attachmentService.StoreAttachmentData(
            fileEntity, 
            request.File.FileData, 
            false, 
            cancellationToken);

        if (attempt.IsFailure)
        {
            _logger
                .ForContext(nameof(AddSubmissionToAssessmentCommand), request, true)
                .ForContext(nameof(Error), attempt.Error, true)
                .Warning("Failed to record Assessment Submission");

            return Result.Failure(attempt.Error);
        }

        _attachmentRepository.Insert(fileEntity);

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
