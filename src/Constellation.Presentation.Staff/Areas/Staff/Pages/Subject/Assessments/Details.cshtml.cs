namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Subject.Assessments;

using Application.Domains.Assessments.Assessments.Commands.AddDownloadToAssessment;
using Application.Domains.Assessments.Assessments.Commands.AddInstructionsToAssessment;
using Application.Domains.Assessments.Assessments.Commands.AddProvisionToAssessmentStudent;
using Application.Domains.Assessments.Assessments.Commands.AddStudentToAssessment;
using Application.Domains.Assessments.Assessments.Commands.AddSubmissionToAssessment;
using Application.Domains.Assessments.Assessments.Commands.LinkAssessmentToCanvas;
using Application.Domains.Assessments.Assessments.Commands.RemoveDownloadFromAssessment;
using Application.Domains.Assessments.Assessments.Commands.RemoveInstructionFromAssessment;
using Application.Domains.Assessments.Assessments.Commands.RemoveStudentFromAssessment;
using Application.Domains.Assessments.Assessments.Commands.SendAssessmentNotification;
using Application.Domains.Assessments.Assessments.Commands.UploadSubmissionToCanvas;
using Application.Domains.Assessments.Assessments.Queries.GetAssessmentDetailsById;
using Application.Domains.Assessments.Assessments.Queries.GetAssessmentDownload;
using Application.Domains.Assessments.Assessments.Queries.GetAssessmentDownloadFile;
using Application.Domains.Assessments.Assessments.Queries.GetAssessmentSubmissionFile;
using Application.Domains.Assessments.Assessments.Queries.GetAssessmentSubmissionsForDownload;
using Application.Domains.Assessments.Assessments.Queries.GetCanvasCoursesAndAssessments;
using Application.Domains.Assessments.Provisions.Models;
using Application.Domains.Assessments.Provisions.Queries.GetAssessmentProvisions;
using Application.Domains.Assessments.Provisions.Queries.GetCurrentStudentProvisionsByStudentId;
using Application.Domains.Messaging.Drafts.Commands.AddAssessmentRecipientsToDraft;
using Application.Domains.Students.Models;
using Application.Domains.Students.Queries.GetStudentById;
using Application.DTOs;
using Application.Models.Auth;
using Constellation.Application.Common.PresentationModels;
using Constellation.Application.Domains.Assessments.Assessments.Models;
using Constellation.Core.Abstractions.Services;
using Constellation.Core.Models.Canvas.Models;
using Constellation.Core.Shared;
using Core.Models.Assessments.Enums;
using Core.Models.Assessments.Errors;
using Core.Models.Assessments.Identifiers;
using Core.Models.Attachments.DTOs;
using Core.Models.Students.Errors;
using Core.Models.Students.Identifiers;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Presentation.Shared.Extensions;
using Presentation.Shared.Helpers.Attributes;
using Serilog;
using Shared.Components.AddDownloadToAssessment;
using Shared.Components.AddSubmissionToAssessment;
using Shared.Components.ConfirmAssessmentNotificationSend;
using Shared.Components.CreateMessageDraftFromAssessment;
using Shared.Components.LinkAssessmentToCanvas;
using Shared.PartialViews.AddAssessmentProvisionForStudent;
using Shared.PartialViews.ConfirmRemoveDocumentFromAssessmentModal;
using Shared.PartialViews.ConfirmRemoveInstructionFromAssessmentModal;
using Shared.PartialViews.ConfirmRemoveStudentFromAssessmentModal;
using Shared.PartialViews.UpsertAssessmentInstructions;

[HasPermission(AuthPermission.Subjects_Assessments_View_Value)]
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
            .ForStaffPortal();
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.Subject_Assessments_Assessments;
    [ViewData] public string PageTitle => "Assessment Details";

    [BindProperty(SupportsGet = true)] 
    public AssessmentId Id { get; set; } = AssessmentId.Empty;

    public AssessmentDetailsResponse Assessment { get; set; }

    public async Task OnGet() => await PreparePage();

    private async Task PreparePage()
    {
        Result<AssessmentDetailsResponse> assessment = await _mediator.Send(new GetAssessmentDetailsByIdQuery(Id));

        if (assessment.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), assessment.Error, true)
                .Warning("Failed to retrieve Assessment by user {User}", _currentUserService.UserName);

            ModalContent = ErrorDisplay.Create(
                assessment.Error,
                _linkGenerator.GetPathByPage("/Subject/Assessments/Index", values: new { area = "Staff" }));

            return;
        }

        Assessment = assessment.Value;
    }

    public async Task<IActionResult> OnPostAjaxRemoveStudent(StudentId studentId)
    {
        Result<StudentResponse> student = await _mediator.Send(new GetStudentByIdQuery(studentId));

        if (student.IsFailure)
            return BadRequest();

        ConfirmRemoveStudentFromAssessmentModalViewModel viewModel = new(
            studentId,
            student.Value.Name);

        return Partial("ConfirmRemoveStudentFromAssessmentModal", viewModel);
    }

    public async Task<IActionResult> OnPostAjaxRemoveDocument(AssessmentDownloadId documentId)
    {
        Result<AssessmentDownloadResponse> download = await _mediator.Send(new GetAssessmentDownloadQuery(Id, documentId));

        if (download.IsFailure)
            return BadRequest();

        ConfirmRemoveDocumentFromAssessmentModalViewModel viewModel = new(
            documentId,
            download.Value.Name);

        return Partial("ConfirmRemoveDocumentFromAssessmentModal", viewModel);
    }

    public async Task<IActionResult> OnGetRemoveStudent(StudentId studentId)
    {
        if (studentId == StudentId.Empty)
        {
            _logger
                .ForContext(nameof(Error), StudentErrors.InvalidId)
                .Warning("Failed to remove student from Assessment by user {User}", _currentUserService.UserName);

            ModalContent = ErrorDisplay.Create(
                StudentErrors.InvalidId,
                _linkGenerator.GetPathByPage("/Subject/Assessments/Details", values: new { area = "Staff", Id }));

            await PreparePage();

            return Page();
        }

        RemoveStudentFromAssessmentCommand command = new(Id, studentId);

        _logger
            .ForContext(nameof(RemoveStudentFromAssessmentCommand), command, true)
            .Information("Requested to remove student from Assessment by user {User}", _currentUserService.UserName);

        Result result = await _mediator.Send(command);

        if (result.IsFailure)
        {
            _logger
                .ForContext(nameof(RemoveStudentFromAssessmentCommand), command, true)
                .ForContext(nameof(Error), result.Error)
                .Warning("Failed to remove student from Assessment by user {User}", _currentUserService.UserName);
            
            ModalContent = ErrorDisplay.Create(
                result.Error,
                _linkGenerator.GetPathByPage("/Subject/Assessments/Details", values: new { area = "Staff", Id }));

            await PreparePage();

            return Page();
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnGetRemoveDocument(AssessmentDownloadId documentId)
    {
        if (documentId == AssessmentDownloadId.Empty)
        {
            _logger
                .ForContext(nameof(Error), AssessmentDownloadErrors.NotFound(documentId), true)
                .Warning("Failed to remove download from Assessment by user {User}", _currentUserService.UserName);

            ModalContent = ErrorDisplay.Create(
                StudentErrors.InvalidId,
                _linkGenerator.GetPathByPage("/Subject/Assessments/Details", values: new { area = "Staff", Id }));

            await PreparePage();

            return Page();
        }

        RemoveDownloadFromAssessmentCommand command = new(Id, documentId);

        _logger
            .ForContext(nameof(RemoveDownloadFromAssessmentCommand), command, true)
            .Information("Requested to remove document from Assessment by user {User}", _currentUserService.UserName);

        Result result = await _mediator.Send(command);

        if (result.IsFailure)
        {
            _logger
                .ForContext(nameof(RemoveDownloadFromAssessmentCommand), command, true)
                .ForContext(nameof(Error), result.Error)
                .Warning("Failed to remove document from Assessment by user {User}", _currentUserService.UserName);

            ModalContent = ErrorDisplay.Create(
                result.Error,
                _linkGenerator.GetPathByPage("/Subject/Assessments/Details", values: new { area = "Staff", Id }));

            await PreparePage();

            return Page();
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostAddStudent(StudentId studentId)
    {
        if (studentId == StudentId.Empty)
        {
            _logger
                .ForContext(nameof(Error), StudentErrors.InvalidId)
                .Warning("Failed to add student to Assessment by user {User}", _currentUserService.UserName);

            ModalContent = ErrorDisplay.Create(
                StudentErrors.InvalidId,
                _linkGenerator.GetPathByPage("/Subject/Assessments/Details", values: new { area = "Staff", Id }));

            await PreparePage();

            return Page();
        }

        AddStudentToAssessmentCommand command = new(Id, studentId);

        _logger
            .ForContext(nameof(AddStudentToAssessmentCommand), command, true)
            .Information("Requested to add student to Assessment by user {User}", _currentUserService.UserName);

        Result result = await _mediator.Send(command);

        if (result.IsFailure)
        {
            _logger
                .ForContext(nameof(AddStudentToAssessmentCommand), command, true)
                .ForContext(nameof(Error), result.Error)
                .Warning("Failed to add student to Assessment by user {User}", _currentUserService.UserName);

            ModalContent = ErrorDisplay.Create(
                result.Error,
                _linkGenerator.GetPathByPage("/Subject/Assessments/Details", values: new { area = "Staff", Id }));

            await PreparePage();

            return Page();
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostAjaxAddProvision(StudentId studentId)
    {
        Result<StudentResponse> student = await _mediator.Send(new GetStudentByIdQuery(studentId));

        if (student.IsFailure)
            return BadRequest();

        Result<List<AssessmentProvisionResponse>> enabledProvisions = await _mediator.Send(new GetCurrentStudentProvisionsByStudentIdQuery(studentId, Id));

        if (enabledProvisions.IsFailure)
            return BadRequest();

        Result<List<AssessmentProvisionResponse>> provisions = await _mediator.Send(new GetAssessmentProvisionsQuery());

        if (provisions.IsFailure)
            return BadRequest();

        AddAssessmentProvisionForStudentViewModel viewModel = new()
        {
            StudentId = student.Value.StudentId,
            Student = student.Value.Name, 
            EnabledProvisionIds = enabledProvisions.Value.Select(entry => entry.Id).ToList(),
            Provisions = provisions.Value
        };

        return Partial("AddAssessmentProvisionForStudent", viewModel);
    }

    public async Task<IActionResult> OnPostAddProvision(
        StudentId studentId, 
        List<ProvisionId> provisions)
    {
        if (studentId == StudentId.Empty)
        {
            _logger
                .ForContext(nameof(Error), StudentErrors.InvalidId)
                .Warning("Failed to add provision to student in Assessment by user {User}", _currentUserService.UserName);

            ModalContent = ErrorDisplay.Create(
                StudentErrors.InvalidId,
                _linkGenerator.GetPathByPage("/Subject/Assessments/Details", values: new { area = "Staff", Id }));

            await PreparePage();

            return Page();
        }

        AddProvisionToAssessmentStudentCommand command = new(Id, studentId, provisions);

        _logger
            .ForContext(nameof(AddProvisionToAssessmentStudentCommand), command, true)
            .Information("Requested to add provision to student in Assessment by user {User}", _currentUserService.UserName);

        Result result = await _mediator.Send(command);

        if (result.IsFailure)
        {
            _logger
                .ForContext(nameof(AddProvisionToAssessmentStudentCommand), command, true)
                .ForContext(nameof(Error), result.Error)
                .Warning("Failed to add provision to student in Assessment by user {User}", _currentUserService.UserName);

            ModalContent = ErrorDisplay.Create(
                result.Error,
                _linkGenerator.GetPathByPage("/Subject/Assessments/Details", values: new { area = "Staff", Id }));

            await PreparePage();

            return Page();
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostAddDownload(AddDownloadToAssessmentSelection viewModel)
    {
        if (viewModel?.UploadFile is not null)
        {
            try
            {
                _logger.Information("Requested to upload document for Assessment by user {User}", _currentUserService.UserName);

                await using MemoryStream target = new();
                await viewModel.UploadFile.CopyToAsync(target);

                FileDto file = new FileDto()
                {
                    FileData = target.ToArray(),
                    FileName = viewModel.UploadFile.FileName,
                    FileType = viewModel.UploadFile.ContentType
                };

                AddDownloadToAssessmentCommand command = new(
                    Id,
                    viewModel.Name,
                    viewModel.AvailableFrom,
                    viewModel.AvailableTo,
                    viewModel.IsRestricted,
                    file);

                Result request = await _mediator.Send(command);

                if (request.IsFailure)
                {
                    _logger
                        .ForContext(nameof(Error), request.Error, true)
                        .Warning("Failed to upload document for Assessment by user {User}", _currentUserService.UserName);

                    ModalContent = ErrorDisplay.Create(
                        request.Error,
                        _linkGenerator.GetPathByPage("/Subject/Assessments/Details", values: new { area = "Staff", Id }));

                    return Page();

                }
            }
            catch (Exception ex)
            {
                _logger
                    .ForContext(nameof(Exception), ex, true)
                    .Warning("Failed to upload document for Assessment by user {User}", _currentUserService.UserName);

                ModalContent = ErrorDisplay.Create(
                    new(ex.Source!, ex.Message),
                    _linkGenerator.GetPathByPage("/Subject/Assessments/Details", values: new { area = "Staff", Id }));

                await PreparePage();

                return Page();
            }
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostUploadSubmission(AssessmentSubmissionSelection viewModel)
    {
        if (viewModel?.File is not null)
        {
            try
            {
                _logger.Information("Requested to upload submission for Assessment by user {User}", _currentUserService.UserName);

                await using MemoryStream target = new();
                await viewModel.File.CopyToAsync(target);

                FileDto file = new()
                {
                    FileData = target.ToArray(),
                    FileName = viewModel.File.FileName,
                    FileType = viewModel.File.ContentType
                };

                AddSubmissionToAssessmentCommand command = new(
                    Id,
                    viewModel.StudentId,
                    file);

                Result request = await _mediator.Send(command);

                if (request.IsFailure)
                {
                    _logger
                        .ForContext(nameof(Error), request.Error, true)
                        .Warning("Failed to upload submission for Assessment by user {User}", _currentUserService.UserName);

                    ModalContent = ErrorDisplay.Create(
                        request.Error,
                        _linkGenerator.GetPathByPage("/Subject/Assessments/Details", values: new { area = "Staff", Id }));

                    return Page();

                }
            }
            catch (Exception ex)
            {
                _logger
                    .ForContext(nameof(Exception), ex, true)
                    .Warning("Failed to upload submission for Assessment by user {User}", _currentUserService.UserName);

                ModalContent = ErrorDisplay.Create(
                    new(ex.Source!, ex.Message),
                    _linkGenerator.GetPathByPage("/Subject/Assessments/Details", values: new { area = "Staff", Id }));

                await PreparePage();

                return Page();
            }
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnGetDownloadFile(AssessmentDownloadId downloadId)
    {
        _logger.Information("Requested to download document for Assessment by user {User}", _currentUserService.UserName);

        Result<AttachmentResponse> documentRequest = await _mediator.Send(new GetAssessmentDownloadFileQuery(Id, downloadId));

        if (documentRequest.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), documentRequest.Error, true)
                .Warning("Failed to download document for Assessment by user {User}", _currentUserService.UserName);

            ModalContent = ErrorDisplay.Create(
                documentRequest.Error,
                _linkGenerator.GetPathByPage("/Subject/Assessments/Details", values: new { area = "Staff", Id }));

            await PreparePage();

            return Page();
        }
        
        return File(documentRequest.Value.FileData, documentRequest.Value.FileType, documentRequest.Value.FileName);
    }

    public async Task<IActionResult> OnGetDownloadAllSubmissions()
    {
        _logger.Information("Requested to download all submissions for Assessment by user {User}", _currentUserService.UserName);

        Result<FileDto> downloadRequest = await _mediator.Send(new GetAssessmentSubmissionsForDownloadQuery(Id));

        if (downloadRequest.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), downloadRequest.Error, true)
                .Warning("Failed to download all submissions for Assessment by user {User}", _currentUserService.UserName);

            ModalContent = ErrorDisplay.Create(
                downloadRequest.Error,
                _linkGenerator.GetPathByPage("/Subject/Assessments/Details", values: new { area = "Staff", Id }));

            await PreparePage();

            return Page();
        }

        return File(downloadRequest.Value.FileData, downloadRequest.Value.FileType, downloadRequest.Value.FileName);
    }

    public async Task<IActionResult> OnGetDownloadSubmission(SubmissionId submissionId)
    {
        _logger.Information("Requested to download submission from Assessment by user {User}", _currentUserService.UserName);

        Result<FileDto> downloadRequest = await _mediator.Send(new GetAssessmentSubmissionFileQuery(Id, submissionId));

        if (downloadRequest.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), downloadRequest.Error, true)
                .Warning("Failed to download submission from Assessment by user {User}", _currentUserService.UserName);

            ModalContent = ErrorDisplay.Create(
                downloadRequest.Error,
                _linkGenerator.GetPathByPage("/Subject/Assessments/Details", values: new { area = "Staff", Id }));

            await PreparePage();

            return Page();
        }

        return File(downloadRequest.Value.FileData, downloadRequest.Value.FileType, downloadRequest.Value.FileName);
    }

    public async Task<IActionResult> OnGetUploadToCanvas(SubmissionId submissionId)
    {
        UploadSubmissionToCanvasCommand command = new(
            Id,
            submissionId);

        _logger
            .ForContext(nameof(UploadSubmissionToCanvasCommand), command, true)
            .Information("Requested to upload submission to canvas from Assessment by user {User}", _currentUserService.UserName);

        Result result = await _mediator.Send(command);

        if (result.IsFailure)
        {
            _logger
                .ForContext(nameof(UploadSubmissionToCanvasCommand), command, true)
                .ForContext(nameof(Error), result.Error, true)
                .Warning("Failed to upload submission to canvas from Assessment by user {User}", _currentUserService.UserName);

            ModalContent = ErrorDisplay.Create(result.Error);

            await PreparePage();
            return Page();
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostAjaxUpdateInstructions(AssessmentInstructionId instructionId, UserCategory category)
    {
        UpsertAssessmentInstructionsViewModel viewModel = new()
        {
            Category = category
        };

        if (instructionId == AssessmentInstructionId.Empty)
            return Partial("UpsertAssessmentInstructions", viewModel);

        Result<AssessmentDetailsResponse> assessment = await _mediator.Send(new GetAssessmentDetailsByIdQuery(Id));

        if (assessment.IsFailure)
            return Partial("UpsertAssessmentInstructions", viewModel);
            
        AssessmentDetailsResponse.Instruction? instruction = assessment.Value.Instructions.FirstOrDefault(entry => entry.InstructionId == instructionId);

        if (instruction is null)
            return Partial("UpsertAssessmentInstructions", viewModel);

        viewModel.Instructions = instruction.Description;

        return Partial("UpsertAssessmentInstructions", viewModel);
    }

    public async Task<IActionResult> OnPostUpsertInstructions(UpsertAssessmentInstructionsViewModel viewModel)
    {
        AddInstructionsToAssessmentCommand command = new(Id, viewModel.Category, viewModel.Instructions);

        _logger
            .ForContext(nameof(AddInstructionsToAssessmentCommand), command, true)
            .Information("Requested to add Instructions to Assessment by user {User}", _currentUserService.UserName);

        Result result = await _mediator.Send(command);

        if (result.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), result.Error, true)
                .Warning("Failed to add Instructions to Assessment by user {User}", _currentUserService.UserName);

            ModalContent = ErrorDisplay.Create(
                result.Error,
                _linkGenerator.GetPathByPage("/Subject/Assessments/Details", values: new { area = "Staff", Id }));

            await PreparePage();

            return Page();

        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostAjaxRemoveInstruction(AssessmentInstructionId instructionId)
    {
        ConfirmRemoveInstructionFromAssessmentModalViewModel viewModel = new() { InstructionId = instructionId };

        return Partial("ConfirmRemoveInstructionFromAssessmentModal", viewModel);
    }

    public async Task<IActionResult> OnGetRemoveInstruction(AssessmentInstructionId instructionId)
    {
        if (instructionId == AssessmentInstructionId.Empty)
        {
            _logger
                .ForContext(nameof(Error), AssessmentInstructionErrors.InvalidId)
                .Warning("Failed to remove instruction from Assessment by user {User}", _currentUserService.UserName);

            ModalContent = ErrorDisplay.Create(
                AssessmentInstructionErrors.InvalidId,
                _linkGenerator.GetPathByPage("/Subject/Assessments/Details", values: new { area = "Staff", Id }));

            await PreparePage();

            return Page();
        }

        RemoveInstructionFromAssessmentCommand command = new(Id, instructionId);

        _logger
            .ForContext(nameof(RemoveInstructionFromAssessmentCommand), command, true)
            .Information("Requested to remove instruction from Assessment by user {User}", _currentUserService.UserName);

        Result result = await _mediator.Send(command);

        if (result.IsFailure)
        {
            _logger
                .ForContext(nameof(RemoveInstructionFromAssessmentCommand), command, true)
                .ForContext(nameof(Error), result.Error)
                .Warning("Failed to remove instruction from Assessment by user {User}", _currentUserService.UserName);

            ModalContent = ErrorDisplay.Create(
                result.Error,
                _linkGenerator.GetPathByPage("/Subject/Assessments/Details", values: new { area = "Staff", Id }));

            await PreparePage();

            return Page();
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnGetAjaxLoadCanvasAssignments()
    {
        Result<List<CanvasCourseWithAssessmentResponse>> courses = await _mediator.Send(new GetCanvasCoursesAndAssessmentsQuery());

        if (courses.IsFailure)
            return Content(string.Empty);

        return ViewComponent("LinkAssessmentToCanvas", courses.Value);
    }

    public async Task<IActionResult> OnPostLinkCanvasAssignment(LinkAssessmentToCanvasSelection viewModel)
    {
        string[] parts = viewModel.SelectedAssessment.Split(':');
        string courseCodeValue = parts[0];
        CanvasCourseCode courseCode = CanvasCourseCode.FromValue(courseCodeValue);
        int assessmentId = int.Parse(parts[1]);

        LinkAssessmentToCanvasCommand command = new(Id, courseCode, assessmentId, viewModel.ForwardDate);

        _logger
            .ForContext(nameof(LinkAssessmentToCanvasCommand), command, true)
            .Information("Requested to link Canvas Assignment to Assessment by user {User}", _currentUserService.UserName);

        Result result = await _mediator.Send(command);

        if (result.IsFailure)
        {
            _logger
                .ForContext(nameof(LinkAssessmentToCanvasCommand), command, true)
                .ForContext(nameof(Error), result.Error)
                .Information("Requested to link Canvas Assignment to Assessment by user {User}", _currentUserService.UserName);

            ModalContent = ErrorDisplay.Create(
                result.Error,
                _linkGenerator.GetPathByPage("/Subject/Assessments/Details", values: new { area = "Staff", Id }));

            await PreparePage();

            return Page();
        }

        return RedirectToPage();
    }
    
    public async Task<IActionResult> OnPostSendNotifications(ConfirmAssessmentNotificationSendSelection viewModel)
    {
        SendAssessmentNotificationCommand command = new(
            Id,
            viewModel.IncludeStudents,
            viewModel.IncludeParents,
            viewModel.IncludeSchoolContacts,
            viewModel.IncludeClassroomTeachers);

        var result = await _mediator.Send(command);

        if (result.IsFailure)
        {
            ModalContent = ErrorDisplay.Create(result.Error);

            await PreparePage();

            return Page();
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostCreateDraft(CreateMessageDraftFromAssessmentSelection viewModel)
    {
        AddAssessmentRecipientsToDraftCommand command = new(
            Id, 
            User.GetUserId(), 
            viewModel.IncludeStudents,
            viewModel.IncludeParents, 
            viewModel.IncludeSchoolContacts, 
            viewModel.IncludeClassroomTeachers);

        Result result = await _mediator.Send(command);

        if (result.IsFailure)
        {
            ModalContent = ErrorDisplay.Create(result.Error);
        }
        else
        {
            ModalContent = FeedbackDisplay.Create("Draft Created", "The draft message has been created", "Ok", "btn-success");
        }

        await PreparePage();
        return Page();
    }
}
