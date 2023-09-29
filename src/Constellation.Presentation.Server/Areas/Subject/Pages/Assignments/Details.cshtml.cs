namespace Constellation.Presentation.Server.Areas.Subject.Pages.Assignments;

using Constellation.Application.Assignments.GetAssignmentById;
using Constellation.Application.Assignments.GetAssignmentSubmissionFile;
using Constellation.Application.Assignments.ResendAssignmentSubmissionToCanvas;
using Constellation.Application.Assignments.UploadAssignmentSubmission;
using Constellation.Application.DTOs;
using Constellation.Application.Models.Auth;
using Constellation.Core.Models.Assignments.Identifiers;
using Constellation.Presentation.Server.BaseModels;
using Constellation.Presentation.Server.Pages.Shared.Components.UploadAssignmentSubmission;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize(Policy = AuthPolicies.IsStaffMember)]
public class DetailsModel : BasePageModel
{
    private readonly IMediator _mediator;
    private readonly LinkGenerator _linkGenerator;

    public DetailsModel(
        IMediator mediator,
        LinkGenerator linkGenerator)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
    }

    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    public AssignmentResponse Assignment { get; set; }

    public async Task<IActionResult> OnGet(CancellationToken cancellationToken)
    {
        await GetClasses(_mediator);

        var assignmentId = AssignmentId.FromValue(Id);

        var request = await _mediator.Send(new GetAssignmentByIdQuery(assignmentId), cancellationToken);

        if (request.IsFailure)
        {
            Error = new()
            {
                Error = request.Error,
                RedirectPath = _linkGenerator.GetPathByPage("/Assignments/Index", values: new { area = "Subject" })
            };

            return Page();
        }

        Assignment = request.Value;

        return Page();
    }

    public async Task<IActionResult> OnGetResubmit(Guid submission, CancellationToken cancellationToken)
    {
        var assignmentId = AssignmentId.FromValue(Id);
        var submissionId = AssignmentSubmissionId.FromValue(submission);

        await _mediator.Send(new ResendAssignmentSubmissionToCanvasCommand(assignmentId, submissionId), cancellationToken);

        return RedirectToPage("/Assignments/Details", new { area = "Subject", Id });
    }

    public async Task<IActionResult> OnGetDownload(Guid submission, CancellationToken cancellationToken)
    {
        var assignmentId = AssignmentId.FromValue(Id);
        var submissionId = AssignmentSubmissionId.FromValue(submission);

        var fileRequest = await _mediator.Send(new GetAssignmentSubmissionFileQuery(assignmentId, submissionId), cancellationToken);

        if (fileRequest.IsFailure)
        {
            Error = new()
            {
                Error = fileRequest.Error,
                RedirectPath = _linkGenerator.GetPathByPage("/Assignments/Index", values: new { area = "Subject" })
            };

            return Page();
        }

        return File(fileRequest.Value.FileData, fileRequest.Value.FileType, fileRequest.Value.FileName);
    }

    public async Task<IActionResult> OnPostUpload(AssignmentStudentSelection viewModel, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(viewModel.StudentId))
        {
            Error = new()
            {
                Error = new("Page.Parameter.InvalidValue", "The student Id value is invalid"),
                RedirectPath = _linkGenerator.GetPathByPage("/Assignments/Details", values: new { area = "Subject", Id })
            };

            return Page();
        }

        if (viewModel.File is null)
        {
            Error = new()
            {
                Error = new("Page.Parameter.InvalidValue", "You must upload a file to submit to Canvas"),
                RedirectPath = _linkGenerator.GetPathByPage("/Assignments/Details", values: new { area = "Subject", Id })
            };

            return Page();
        }

        var file = new FileDto
        {
            FileName = viewModel.File.FileName,
            FileType = viewModel.File.ContentType
        };

        try
        {
            await using var target = new MemoryStream();
            await viewModel.File.CopyToAsync(target, cancellationToken);
            file.FileData = target.ToArray();
        }
        catch (Exception ex)
        {
            //Whats going on here?
            Error = new()
            {
                Error = new("Page.Parameter.ProcessingError", "Could not read the uploaded file"),
                RedirectPath = _linkGenerator.GetPathByPage("/Assignments/Details", values: new { area = "Subject", Id })
            };

            return Page();
        }

        var assignmentId = AssignmentId.FromValue(Id);

        await _mediator.Send(new UploadAssignmentSubmissionCommand(assignmentId, viewModel.StudentId, file), cancellationToken);

        return RedirectToPage("/Assignments/Details", new { area = "Subject", Id });
    }
}
