namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Subject.Assessments;

using Application.Domains.Assessments.Assessments.Commands.RemoveStudentFromAssessment;
using Application.Domains.Assessments.Assessments.Queries.GetAssessmentDetailsById;
using Application.Domains.Students.Models;
using Application.Domains.Students.Queries.GetStudentById;
using Application.Models.Auth;
using Constellation.Application.Common.PresentationModels;
using Constellation.Application.Domains.Assessments.Assessments.Models;
using Constellation.Core.Abstractions.Services;
using Constellation.Core.Shared;
using Core.Models.Assessments.Identifiers;
using Core.Models.Students.Errors;
using Core.Models.Students.Identifiers;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Presentation.Shared.Helpers.Attributes;
using Serilog;
using Shared.PartialViews.ConfirmRemoveStudentFromAssessmentModal;

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
        _logger = logger;
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.Subject_Assessments_Assessments;
    [ViewData] public string PageTitle => "Assessment Details";

    [BindProperty(SupportsGet = true)] 
    public AssessmentId Id { get; set; } = AssessmentId.Empty;

    public AssessmentDetailsResponse Assessment { get; set; }

    public async Task OnGet()
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

            return Page();
        }

        return RedirectToPage();
    }
}
