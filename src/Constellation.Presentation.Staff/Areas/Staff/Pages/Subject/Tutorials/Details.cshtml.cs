namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Subject.Tutorials;

using Application.Domains.Enrolments.Commands.EnrolStudentInTutorial;
using Application.Domains.Tutorials.Queries.GetTutorialDetails;
using Application.Models.Auth;
using Constellation.Application.Common.PresentationModels;
using Constellation.Core.Abstractions.Services;
using Core.Abstractions.Clock;
using Core.Models.Tutorials.Identifiers;
using Core.Models.Tutorials.ValueObjects;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Presentation.Shared.Helpers.Logging;
using Serilog;
using Shared.Components.EnrolStudentInTutorial;

[Authorize(Policy = AuthPolicies.IsStaffMember)]
public class DetailsModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly LinkGenerator _linkGenerator;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeProvider _dateTime;
    private readonly ILogger _logger;

    public DetailsModel(
        ISender mediator,
        LinkGenerator linkGenerator,
        ICurrentUserService currentUserService,
        IDateTimeProvider dateTime,
        ILogger logger)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
        _currentUserService = currentUserService;
        _dateTime = dateTime;
        _logger = logger
            .ForContext<DetailsModel>()
            .ForContext(LogDefaults.Application, LogDefaults.StaffPortal);
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.Subject_Tutorials_Tutorials;
    [ViewData] public string PageTitle => "Tutorials";

    [BindProperty(SupportsGet = true)]
    public TutorialId Id { get; set; } = TutorialId.Empty;

    public TutorialDetailsResponse Tutorial { get; set; }

    public async Task OnGet() => await PreparePage();

    private async Task PreparePage()
    {
        Result<TutorialDetailsResponse> response = await _mediator.Send(new GetTutorialDetailsQuery(Id));

        if (response.IsFailure)
        {
            ModalContent = ErrorDisplay.Create(
                response.Error,
                _linkGenerator.GetPathByPage("/Subject/Tutorials/Index", values: new{ area = "Staff" }));

            Tutorial = new(TutorialId.Empty, TutorialName.None, _dateTime.Today, _dateTime.Today, false, [], [], [], 0);

            return;
        }

        Tutorial = response.Value;
    }

    public async Task<IActionResult> OnPostEnrolStudent(EnrolStudentInTutorialSelection viewModel)
    {
        EnrolStudentInTutorialCommand command = new(viewModel.StudentId, Id);

        _logger
            .ForContext(nameof(EnrolStudentInTutorialCommand), command, true)
            .Information("Requested to add Student to Tutorial by user {User}", _currentUserService.UserName);

        Result request = await _mediator.Send(command);

        if (request.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), request.Error, true)
                .Warning("Failed to add Student to Tutorial by user {User}", _currentUserService.UserName);

            ModalContent = ErrorDisplay.Create(request.Error);

            await PreparePage();

            return Page();
        }

        return RedirectToPage();
    }
}