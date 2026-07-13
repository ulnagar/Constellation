namespace Constellation.Presentation.Schools.Areas.Schools.Pages.Absences;

using Application.Common.PresentationModels;
using Application.Domains.Attendance.Absences.Commands.RejectStudentExplanation;
using Application.Domains.Attendance.Absences.Commands.VerifyStudenExplanation;
using Application.Models.Auth;
using Constellation.Application.Domains.Attendance.Absences.Queries.GetAbsenceResponseDetailsForSchool;
using Constellation.Core.Models.Absences.Identifiers;
using Constellation.Core.Shared;
using Constellation.Presentation.Shared.Helpers.Logging;
using Core.Abstractions.Services;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Presentation.Shared.Extensions;
using Presentation.Shared.Helpers.Attributes;
using Serilog;

[HasPermission(AuthPermission.SchoolsPortal_Absences_Edit_Value)]
public class VerifyModel : BasePageModel
{
    private ISender _mediator => Mediator;
    private readonly LinkGenerator _linkGenerator;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public VerifyModel(
        LinkGenerator linkGenerator,
        ICurrentUserService currentUserService,
        ILogger logger)
    {
        _linkGenerator = linkGenerator;
        _currentUserService = currentUserService;
        _logger = logger
            .ForContext<VerifyModel>()
            .ForSchoolPortal();
    }

    [ViewData] public string ActivePage => Models.ActivePage.Absences;

    [BindProperty(SupportsGet = true)]
    public AbsenceId AbsenceId { get; set; }

    [BindProperty(SupportsGet = true)]
    public AbsenceResponseId ResponseId { get; set; }

    public string StudentName { get; set; } = string.Empty;
    public DateOnly AbsenceDate { get; set; }
    public string ClassName { get; set; } = string.Empty;
    public string PeriodName { get; set; } = string.Empty;
    public string AbsenceTimeframe { get; set; } = string.Empty;
    public string Explanation { get; set; } = string.Empty;

    [BindProperty]
    public string? Comment { get; set; }

    public async Task OnGet() => await PreparePage();

    public async Task<IActionResult> OnPostVerify()
    {
        VerifyStudentExplanationCommand command = new(
            AbsenceId,
            ResponseId,
            _currentUserService.EmailAddress,
            Comment);

        _logger.Information("Requested to verify absence explanation by user {user} with details {@command}", _currentUserService.EmailAddress, command);

        Result response = await _mediator.Send(command);

        if (response.IsFailure)
        {
            ModalContent = ErrorDisplay.Create(
                response.Error,
                _linkGenerator.GetPathByPage("/Absences/Index", values: new { area = "Schools" }));

            return Page();
        }

        ModalContent = FeedbackDisplay.Create(
            "Absence Verification", 
            "Absence explanation verification has been forwarded to our Administration Team. Thank you!",
            "Ok",
            "btn-success", 
            _linkGenerator.GetPathByPage("/Absences/Index", values: new { area = "Schools" }));

        return Page();
    }

    public async Task<IActionResult> OnPostReject()
    {
        if (string.IsNullOrWhiteSpace(Comment) || Comment.Length < 5)
        {
            ModelState.AddModelError(nameof(Comment), "You must enter a longer comment to reject the students explanation for this absence!");

            await PreparePage();

            return Page();
        }

        RejectStudentExplanationCommand command = new(
            AbsenceId,
            ResponseId,
            _currentUserService.EmailAddress,
            Comment);

        _logger.Information("Requested to reject absence explanation by user {user} with details {@command}", _currentUserService.EmailAddress, command);

        Result response = await _mediator.Send(command);

        if (response.IsFailure)
        {
            ModalContent = ErrorDisplay.Create(
                response.Error,
                _linkGenerator.GetPathByPage("/Absences/Index", values: new { area = "Schools" }));

            await PreparePage();

            return Page();
        }

        ModalContent = FeedbackDisplay.Create(
            "Absence Verification",
            "Absence explanation rejection has been forwarded to our Administration Team. Thank you!",
            "Ok",
            "btn-success",
            _linkGenerator.GetPathByPage("/Absences/Index", values: new { area = "Schools" }));

        return Page();
    }

    private async Task PreparePage()
    {
        _logger.Information("Requested to retrieve absence response by user {user} for Id {responseId}", _currentUserService.UserName, ResponseId);

        Result<SchoolAbsenceResponseDetailsResponse> responseRequest = await _mediator.Send(new GetAbsenceResponseDetailsForSchoolQuery(AbsenceId, ResponseId));

        if (responseRequest.IsFailure)
        {
            ModalContent = ErrorDisplay.Create(
                responseRequest.Error,
                _linkGenerator.GetPathByPage("/Absences/Index", values: new { area = "Schools" }));

            return;
        }

        StudentName = responseRequest.Value.StudentName;
        AbsenceDate = DateOnly.FromDateTime(responseRequest.Value.AbsenceDate);
        ClassName = responseRequest.Value.ClassName;
        PeriodName = responseRequest.Value.PeriodName;
        AbsenceTimeframe = responseRequest.Value.AbsenceTimeframe;
        Explanation = responseRequest.Value.Explanation;
    }
}