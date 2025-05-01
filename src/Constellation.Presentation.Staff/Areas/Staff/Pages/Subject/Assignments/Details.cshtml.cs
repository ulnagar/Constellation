namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Subject.Assignments;

using Application.Common.PresentationModels;
using Application.Domains.Assignments.Commands.ResendAssignmentSubmissionToCanvas;
using Application.Domains.Assignments.Commands.UploadAssignmentSubmission;
using Application.Domains.Assignments.Queries.GetAllAssignmentSubmissionFiles;
using Application.Domains.Assignments.Queries.GetAssignmentById;
using Application.Domains.Assignments.Queries.GetAssignmentSubmissionFile;
using Constellation.Application.DTOs;
using Constellation.Application.Models.Auth;
using Constellation.Core.Models.Assignments.Identifiers;
using Constellation.Core.Shared;
using Core.Abstractions.Services;
using Core.Models.Students.Identifiers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Models;
using Presentation.Shared.Helpers.Logging;
using Serilog;
using Shared.Components.UploadAssignmentSubmission;
using AssignmentId = Core.Models.Assignments.Identifiers.AssignmentId;

[Authorize(Policy = AuthPolicies.IsStaffMember)]
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

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.Subject_Assignments_Assignments;
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

            ModalContent = new ErrorDisplay(
                request.Error,
                _linkGenerator.GetPathByPage("/Subject/Assignments/Index", values: new { area = "Staff" }));

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

            ModalContent = new ErrorDisplay(
                fileRequest.Error,
                _linkGenerator.GetPathByPage("/Subject/Assignments/Index", values: new { area = "Staff" }));

            return Page();
        }

        return File(fileRequest.Value.FileData, fileRequest.Value.FileType, fileRequest.Value.FileName);
    }

    public async Task<IActionResult> OnGetResubmit(
        AssignmentSubmissionId submission, 
        CancellationToken cancellationToken)
    {
        ResendAssignmentSubmissionToCanvasCommand command = new(Id, submission);

        _logger
            .ForContext(nameof(ResendAssignmentSubmissionToCanvasCommand), command, true)
            .Information("Requested to resend Assignment Submission to Canvas by user {User}", _currentUserService.UserName);

        Result result = await _mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), result.Error, true)
                .Warning("Failed to resend Assignment Submission to Canvas by user {User}", _currentUserService.UserName);

            ModalContent = new ErrorDisplay(result.Error);

            return Page();
        }
        
        return RedirectToPage();
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

            ModalContent = new ErrorDisplay(
                fileRequest.Error,
                _linkGenerator.GetPathByPage("/Subject/Assignments/Index", values: new { area = "Staff" }));

            return Page();
        }

        return File(fileRequest.Value.FileData, fileRequest.Value.FileType, fileRequest.Value.FileName);
    }

    public async Task<IActionResult> OnPostUpload(AssignmentStudentSelection viewModel, CancellationToken cancellationToken)
    {
        if (viewModel.StudentId == StudentId.Empty)
        {
            ModalContent = new ErrorDisplay(
                new("Page.Parameter.InvalidValue", "The student Id value is invalid"),
                _linkGenerator.GetPathByPage("/Subject/Assignments/Details", values: new { area = "Staff", Id }));

            return Page();
        }

        if (viewModel.File is null)
        {
            ModalContent = new ErrorDisplay(
                new("Page.Parameter.InvalidValue", "You must upload a file to submit to Canvas"),
                _linkGenerator.GetPathByPage("/Subject/Assignments/Details", values: new { area = "Staff", Id }));

            return Page();
        }

        FileDto file = new()
        {
            FileName = viewModel.File.FileName,
            FileType = viewModel.File.ContentType
        };

        try
        {
            await using MemoryStream target = new MemoryStream();
            await viewModel.File.CopyToAsync(target, cancellationToken);
            file.FileData = target.ToArray();
        }
        catch (Exception ex)
        {
            //Whats going on here?
            ModalContent = new ErrorDisplay(
                new("Page.Parameter.ProcessingError", "Could not read the uploaded file"),
                _linkGenerator.GetPathByPage("/Subject/Assignments/Details", values: new { area = "Staff", Id }));

            return Page();
        }

        UploadAssignmentSubmissionCommand command = new(Id, viewModel.StudentId, file);

        _logger
            .ForContext(nameof(UploadAssignmentSubmissionCommand), command, true)
            .Information("Requested to upload new Assignment Submission by user {User}", _currentUserService.UserName);

        Result result = await _mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), result.Error, true)
                .Warning("Failed to upload new Assignment Submission by user {User}", _currentUserService.UserName);

            ModalContent = new ErrorDisplay(result.Error);

            return Page();
        }

        return RedirectToPage("/Subject/Assignments/Details", new { area = "Staff", Id });
    }
}
