namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Subject.Tutorials;

using Application.Common.PresentationModels;
using Application.Domains.Tutorials.Commands.CreateTutorial;
using Application.Domains.Tutorials.Commands.UpdateTutorial;
using Application.Domains.Tutorials.Queries.GetTutorialForEdit;
using Application.Models.Auth;
using Core.Abstractions.Clock;
using Core.Abstractions.Services;
using Core.Models.Tutorials.Identifiers;
using Core.Models.Tutorials.ValueObjects;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Presentation.Shared.Helpers.Logging;
using Serilog;
using System.ComponentModel.DataAnnotations;

[Authorize(Policy = AuthPolicies.CanEditSubjects)]
public class UpsertModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly LinkGenerator _linkGenerator;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeProvider _dateTime;
    private readonly ILogger _logger;

    public UpsertModel(
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
            .ForContext<UpsertModel>()
            .ForContext(LogDefaults.Application, LogDefaults.StaffPortal);
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.Subject_Tutorials_Tutorials;
    [ViewData] public string PageTitle => "Tutorials";

    [BindProperty(SupportsGet = true)]
    public TutorialId Id { get; set; } = TutorialId.Empty;


    [BindProperty]
    public string Name { get; set; }

    [BindProperty]
    [DataType(DataType.Date)]
    [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd}")]
    public DateOnly StartDate { get; set; }

    [BindProperty]
    [DataType(DataType.Date)]
    [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd}")]
    public DateOnly EndDate { get; set; }

    public async Task OnGet()
    {
        StartDate = _dateTime.Today;
        EndDate = _dateTime.LastDayOfYear;

        if (Id == TutorialId.Empty)
            return;

        Result<TutorialForEditResponse> tutorialRequest = await _mediator.Send(new GetTutorialForEditQuery(Id));

        if (tutorialRequest.IsFailure)
        {
            ModalContent = ErrorDisplay.Create(
                tutorialRequest.Error,
                _linkGenerator.GetPathByPage("/Subject/Tutorials/Index", values: new { area = "Staff" }));

            return;
        }

        Name = tutorialRequest.Value.Name.ToString();
        StartDate = tutorialRequest.Value.StartDate;
        EndDate = tutorialRequest.Value.EndDate;
    }

    public async Task<IActionResult> OnPostCreate()
    {
        TutorialName name = TutorialName.FromValue(Name);

        CreateTutorialCommand command = new(name, StartDate, EndDate);

        _logger
            .ForContext(nameof(CreateTutorialCommand), command, true)
            .Information("Requested to create Tutorial by user {User}", _currentUserService.UserName);

        Result<TutorialId> request = await _mediator.Send(command);

        if (request.IsFailure)
        {
            _logger
                .ForContext(nameof(CreateTutorialCommand), command, true)
                .ForContext(nameof(Error), request.Error, true)
                .Warning("Failed to create Tutorial by user {User}", _currentUserService.UserName);

            ModalContent = ErrorDisplay.Create(request.Error);

            return Page();
        }

        return RedirectToPage("/Subject/Tutorials/Details", new { area = "Staff", Id = request.Value });
    }

    public async Task<IActionResult> OnPostUpdate()
    {
        TutorialName name = TutorialName.FromValue(Name);

        UpdateTutorialCommand command = new(Id, name, StartDate, EndDate);

        _logger
            .ForContext(nameof(UpdateTutorialCommand), command, true)
            .Information("Requested to update Tutorial by user {User}", _currentUserService.UserName);

        Result request = await _mediator.Send(command);

        if (request.IsFailure)
        {
            _logger
                .ForContext(nameof(UpdateTutorialCommand), command, true)
                .ForContext(nameof(Error), request.Error, true)
                .Warning("Failed to update Tutorial by user {User}", _currentUserService.UserName);

            ModalContent = ErrorDisplay.Create(request.Error);

            return Page();
        }

        return RedirectToPage("/Subject/Tutorials/Details", new { area = "Staff", Id });
    }
}