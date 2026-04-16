namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Subject.Archive;

using Constellation.Application.Common.PresentationModels;
using Constellation.Application.Domains.Assessments.Archive.Queries.GetAllAssignmentSubmissionFiles;
using Constellation.Application.Domains.Assessments.Archive.Queries.GetAssignmentById;
using Constellation.Application.Domains.Assessments.Archive.Queries.GetAssignmentSubmissionFile;
using Constellation.Application.DTOs;
using Constellation.Application.Models.Auth;
using Constellation.Core.Abstractions.Services;
using Constellation.Core.Models.Assessments.Archive.Identifiers;
using Constellation.Core.Shared;
using Constellation.Presentation.Shared.Helpers.Attributes;
using Constellation.Presentation.Shared.Helpers.Logging;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Serilog;

[HasPermission(AuthPermission.Subjects_Assignments_View_Value)]
public class DetailsModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly LinkGenerator _linkGenerator;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public DetailsModel(
        ISender mediator,
        LinkGenerator linkGenerator,
        ICurrentUserService currentUserService,
        ILogger logger)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
        _currentUserService = currentUserService;
        _logger = logger
            .ForContext<DetailsModel>()
            .ForContext(LogDefaults.Application, LogDefaults.StaffPortal);
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.Subject_Assessments_Archive;
    [ViewData] public string PageTitle { get; set; } = "Assignment Details";


    [BindProperty(SupportsGet = true)]
    public AssignmentId Id { get; set; } = AssignmentId.Empty;

    public AssignmentResponse Assignment { get; set; }

    public async Task OnGet(CancellationToken cancellationToken)
    {
        _logger.Information("Requested to retrieve details of Assignment with id {Id} by user {User}", Id, _currentUserService.UserName);
        
        Result<AssignmentResponse> request = await _mediator.Send(new GetAssignmentByIdQuery(Id), cancellationToken);

        if (request.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), request.Error, true)
                .Warning("Failed to retrieve details of Assignment with id {Id} by user {User}", Id, _currentUserService.UserName);

            ModalContent = ErrorDisplay.Create(
                request.Error,
                _linkGenerator.GetPathByPage("/Subject/Assessments/Archive/Index", values: new { area = "Staff" }));

            return;
        }

        Assignment = request.Value;
        PageTitle = $"Details - {Assignment.AssignmentName}";
    }

    public async Task<IActionResult> OnGetDownloadAll(CancellationToken cancellationToken)
    {
        _logger.Information("Requested to download Assignment Submissions by user {User}", _currentUserService.UserName);

        Result<FileDto> fileRequest = await _mediator.Send(new GetAllAssignmentSubmissionFilesQuery(Id), cancellationToken);

        if (fileRequest.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), fileRequest.Error, true)
                .Warning("Failed to download Assignment Submissions by user {User}", _currentUserService.UserName);

            ModalContent = ErrorDisplay.Create(
                fileRequest.Error,
                _linkGenerator.GetPathByPage("/Subject/Assessments/Archive/Index", values: new { area = "Staff" }));

            return Page();
        }

        return File(fileRequest.Value.FileData, fileRequest.Value.FileType, fileRequest.Value.FileName);
    }
    
    public async Task<IActionResult> OnGetDownload(
        AssignmentSubmissionId submission, 
        CancellationToken cancellationToken)
    {
        GetAssignmentSubmissionFileQuery command = new(Id, submission);

        _logger
            .ForContext(nameof(GetAssignmentSubmissionFileQuery), command, true)
            .Information("Requested to download Assignment Submission by user {User}", _currentUserService.UserName);

        Result<FileDto> fileRequest = await _mediator.Send(command, cancellationToken);

        if (fileRequest.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), fileRequest.Error, true)
                .Warning("Failed to download Assignment Submission by user {User}", _currentUserService.UserName);

            ModalContent = ErrorDisplay.Create(
                fileRequest.Error,
                _linkGenerator.GetPathByPage("/Subject/Assessments/Archive/Index", values: new { area = "Staff" }));

            return Page();
        }

        return File(fileRequest.Value.FileData, fileRequest.Value.FileType, fileRequest.Value.FileName);
    }
}
