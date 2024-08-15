namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Subject.Assignments;

using Application.Common.PresentationModels;
using Constellation.Application.Assignments.GetAllAssignmentSubmissionFiles;
using Constellation.Application.Assignments.GetAssignmentById;
using Constellation.Application.Assignments.GetAssignmentSubmissionFile;
using Constellation.Application.Assignments.ResendAssignmentSubmissionToCanvas;
using Constellation.Application.Assignments.UploadAssignmentSubmission;
using Constellation.Application.DTOs;
using Constellation.Application.Models.Auth;
using Constellation.Core.Models.Assignments.Identifiers;
using Constellation.Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Shared.Components.UploadAssignmentSubmission;

[Authorize(Policy = AuthPolicies.IsStaffMember)]
public class DetailsModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly LinkGenerator _linkGenerator;

    public DetailsModel(
        ISender mediator,
        LinkGenerator linkGenerator)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.Subject_Assignments_Assignments;

    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    public AssignmentResponse Assignment { get; set; }

    public async Task<IActionResult> OnGet(CancellationToken cancellationToken)
    {
        AssignmentId assignmentId = AssignmentId.FromValue(Id);

        Result<AssignmentResponse> request = await _mediator.Send(new GetAssignmentByIdQuery(assignmentId), cancellationToken);

        if (request.IsFailure)
        {
            ModalContent = new ErrorDisplay(
                request.Error,
                _linkGenerator.GetPathByPage("/Subject/Assignments/Index", values: new { area = "Staff" }));

            return Page();
        }

        Assignment = request.Value;

        return Page();
    }

    public async Task<IActionResult> OnGetDownloadAll(CancellationToken cancellationToken)
    {
        AssignmentId assignmentId = AssignmentId.FromValue(Id);

        Result<FileDto> fileRequest = await _mediator.Send(new GetAllAssignmentSubmissionFilesQuery(assignmentId), cancellationToken);

        if (fileRequest.IsFailure)
        {
            ModalContent = new ErrorDisplay(
                fileRequest.Error,
                _linkGenerator.GetPathByPage("/Subject/Assignments/Index", values: new { area = "Staff" }));

            return Page();
        }

        return File(fileRequest.Value.FileData, fileRequest.Value.FileType, fileRequest.Value.FileName);
    }

    public async Task<IActionResult> OnGetResubmit(Guid submission, CancellationToken cancellationToken)
    {
        AssignmentId assignmentId = AssignmentId.FromValue(Id);
        AssignmentSubmissionId submissionId = AssignmentSubmissionId.FromValue(submission);

        await _mediator.Send(new ResendAssignmentSubmissionToCanvasCommand(assignmentId, submissionId), cancellationToken);

        return RedirectToPage("/Subject/Assignments/Details", new { area = "Staff", Id });
    }

    public async Task<IActionResult> OnGetDownload(Guid submission, CancellationToken cancellationToken)
    {
        AssignmentId assignmentId = AssignmentId.FromValue(Id);
        AssignmentSubmissionId submissionId = AssignmentSubmissionId.FromValue(submission);

        Result<FileDto> fileRequest = await _mediator.Send(new GetAssignmentSubmissionFileQuery(assignmentId, submissionId), cancellationToken);

        if (fileRequest.IsFailure)
        {
            ModalContent = new ErrorDisplay(
                fileRequest.Error,
                _linkGenerator.GetPathByPage("/Subject/Assignments/Index", values: new { area = "Staff" }));

            return Page();
        }

        return File(fileRequest.Value.FileData, fileRequest.Value.FileType, fileRequest.Value.FileName);
    }

    public async Task<IActionResult> OnPostUpload(AssignmentStudentSelection viewModel, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(viewModel.StudentId))
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

        AssignmentId assignmentId = AssignmentId.FromValue(Id);

        await _mediator.Send(new UploadAssignmentSubmissionCommand(assignmentId, viewModel.StudentId, file), cancellationToken);

        return RedirectToPage("/Subject/Assignments/Details", new { area = "Staff", Id });
    }
}
