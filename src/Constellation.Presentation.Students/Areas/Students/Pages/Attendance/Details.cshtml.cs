namespace Constellation.Presentation.Students.Areas.Students.Pages.Attendance;

using Application.Domains.Attendance.Absences.Commands.CreateAbsenceResponseFromStudent;
using Constellation.Application.Common.PresentationModels;
using Constellation.Application.Domains.Attendance.Absences.Queries.GetAbsenceDetailsForStudent;
using Constellation.Application.Models.Auth;
using Constellation.Core.Models.Identifiers;
using Constellation.Core.Models.Students.Errors;
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

[Authorize(Policy = AuthPolicies.IsStudent)]
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
            .ForContext(LogDefaults.Application, LogDefaults.StudentPortal);
    }

    [ViewData] public string ActivePage => Models.ActivePage.Attendance;

    [BindProperty(SupportsGet = true)]
    public AbsenceId Id { get; set; }

    public AbsenceForStudentResponse? Absence { get; set; }

    [BindProperty]
    public string Comment { get; set; } = string.Empty;
    
    public async Task OnGet() => await PreparePage();

    public async Task<IActionResult> OnPost()
    {
        if (string.IsNullOrWhiteSpace(Comment) || Comment.Length < 5)
        {
            ModelState.TryAddModelError(nameof(Comment), "You must include a longer comment");

            await PreparePage();

            return Page();
        }

        string studentIdClaimValue = User.Claims.FirstOrDefault(claim => claim.Type == AuthClaimType.StudentId)?.Value ?? string.Empty;

        if (string.IsNullOrWhiteSpace(studentIdClaimValue))
        {
            _logger
                .ForContext(nameof(Error), StudentErrors.InvalidId, true)
                .Warning("Failed to submit absence explanation by user {user}", _currentUserService.UserName);

            ModalContent = ErrorDisplay.Create(StudentErrors.InvalidId);

            await PreparePage();

            return Page();
        }

        StudentId studentId = StudentId.FromValue(new (studentIdClaimValue));

        CreateAbsenceResponseFromStudentCommand command = new(Id, studentId, Comment);

        _logger
            .ForContext(nameof(CreateAbsenceResponseFromStudentCommand), command, true)
            .Information("Requested to submit absence explanation by user {user}", _currentUserService.UserName);

        Result commandRequest = await _mediator.Send(command);

        if (commandRequest.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), commandRequest.Error, true)
                .Warning("Failed to submit absence explanation by user {user}", _currentUserService.UserName);

            ModalContent = ErrorDisplay.Create(commandRequest.Error);
            
            await PreparePage();

            return Page();
        }

        return RedirectToPage("/Attendance/Index", new { area = "Students" });
    }

    public async Task PreparePage()
    {
        _logger.Information("Requested to retrieve absence details by user {user} with Id {id}", _currentUserService.UserName, Id);

        Result<AbsenceForStudentResponse> absenceRequest = await _mediator.Send(new GetAbsenceDetailsForStudentQuery(Id));

        if (absenceRequest.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), absenceRequest.Error, true)
                .Warning("Failed to retrieve absence details by user {user} with Id {id}", _currentUserService.UserName, Id);

            ModalContent = ErrorDisplay.Create(absenceRequest.Error);

            return;
        }

        Absence = absenceRequest.Value;
    }
}
