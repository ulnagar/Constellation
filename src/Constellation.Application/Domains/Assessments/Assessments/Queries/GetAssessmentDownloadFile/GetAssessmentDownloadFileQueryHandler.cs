namespace Constellation.Application.Domains.Assessments.Assessments.Queries.GetAssessmentDownloadFile;

using Abstractions.Messaging;
using Application.Models.Identity.Errors;
using Core.Abstractions.Services;
using Core.Models.Assessments;
using Core.Models.Assessments.Errors;
using Core.Models.Assessments.Repositories;
using Core.Models.Attachments.DTOs;
using Core.Models.Attachments.Enums;
using Core.Models.Attachments.Services;
using Core.Models.Auth;
using Core.Shared;
using Interfaces.Repositories;
using Microsoft.AspNetCore.Identity;
using Serilog;

internal sealed class GetAssessmentDownloadFileQueryHandler
: IQueryHandler<GetAssessmentDownloadFileQuery, AttachmentResponse>
{
    private readonly IAttachmentService _attachmentService;
    private readonly IAssessmentRepository _assessmentRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly UserManager<AppUser> _userManager;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public GetAssessmentDownloadFileQueryHandler(
        IAttachmentService attachmentService,
        IAssessmentRepository assessmentRepository,
        ICurrentUserService currentUserService,
        UserManager<AppUser> userManager,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _attachmentService = attachmentService;
        _assessmentRepository = assessmentRepository;
        _currentUserService = currentUserService;
        _userManager = userManager;
        _unitOfWork = unitOfWork;
        _logger = logger
            .ForContext<GetAssessmentDownloadFileQuery>();
    }

    public async Task<Result<AttachmentResponse>> Handle(GetAssessmentDownloadFileQuery request, CancellationToken cancellationToken)
    {
        Assessment? assessment = await _assessmentRepository.GetAssessmentById(request.AssessmentId, cancellationToken);

        if (assessment is null)
        {
            _logger
                .ForContext(nameof(GetAssessmentDownloadFileQuery), request, true)
                .ForContext(nameof(Error), AssessmentErrors.NotFound(request.AssessmentId), true)
                .Warning("Failed to retrieve Assessment Download File");

            return Result.Failure<AttachmentResponse>(AssessmentErrors.NotFound(request.AssessmentId));
        }

        AssessmentDownload? download = assessment.Downloads.FirstOrDefault(entry => entry.Id == request.DownloadId);

        if (download is null)
        {
            _logger
                .ForContext(nameof(GetAssessmentDownloadFileQuery), request, true)
                .ForContext(nameof(Error), AssessmentDownloadErrors.NotFound(request.DownloadId), true)
                .Warning("Failed to retrieve Assessment Download File");

            return Result.Failure<AttachmentResponse>(AssessmentDownloadErrors.NotFound(request.DownloadId));
        }
        
        Result<AttachmentResponse> file = await _attachmentService.GetAttachmentFile(AttachmentType.AssessmentDownload, request.DownloadId.ToString(), cancellationToken);

        if (file.IsFailure)
        {
            _logger
                .ForContext(nameof(GetAssessmentDownloadFileQuery), request, true)
                .ForContext(nameof(Error), file.Error, true)
                .Warning("Failed to retrieve Assessment Download File");

            return file;
        }

        AppUser? user = await _userManager.FindByNameAsync(_currentUserService.EmailAddress);

        if (user is null)
        {
            _logger
                .ForContext(nameof(GetAssessmentDownloadFileQuery), request, true)
                .ForContext(nameof(Error), AuthErrors.UserNotFound(Guid.Empty), true)
                .Warning("Failed to retrieve Assessment Download File");

            return Result.Failure<AttachmentResponse>(AuthErrors.UserNotFound(Guid.Empty));
        }

        download.AddDownloadEvent(user);

        await _unitOfWork.CompleteAsync(cancellationToken);

        return file;
    }
}
