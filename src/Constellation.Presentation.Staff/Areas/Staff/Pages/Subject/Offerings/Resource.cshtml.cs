namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Subject.Offerings;

using Application.Common.PresentationModels;
using Constellation.Application.Courses.GetCourseSummary;
using Constellation.Application.Courses.Models;
using Constellation.Application.Models.Auth;
using Constellation.Application.Offerings.AddResourceToOffering;
using Constellation.Application.Offerings.GetOfferingDetails;
using Constellation.Application.Teams.GetAllTeams;
using Constellation.Application.Teams.GetTeamByName;
using Constellation.Application.Teams.Models;
using Constellation.Core.Models.Offerings.Identifiers;
using Constellation.Core.Models.Offerings.ValueObjects;
using Constellation.Core.Shared;
using Constellation.Presentation.Staff.Areas;
using Core.Abstractions.Services;
using Core.Extensions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Models;
using Presentation.Shared.Helpers.ModelBinders;
using Serilog;

[Authorize(Policy = AuthPolicies.CanEditSubjects)]
public class ResourceModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly LinkGenerator _linkGenerator;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public ResourceModel(
        ISender mediator,
        LinkGenerator linkGenerator,
        ICurrentUserService currentUserService,
        ILogger logger)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
        _currentUserService = currentUserService;
        _logger = logger
            .ForContext<ResourceModel>()
            .ForContext(StaffLogDefaults.Application, StaffLogDefaults.StaffPortal);
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.Subject_Offerings_Offerings;
    [ViewData] public string PageTitle => "Add Resource";

    [BindProperty(SupportsGet = true)]
    public OfferingId Id { get; set; } = OfferingId.Empty;

    [BindProperty]
    public Phase CurrentStep { get; set; }

    [BindProperty]
    public List<Phase> PreviousSteps { get; set; } = new();

    [BindProperty]
    [ModelBinder(typeof(FromValueBinder))]
    public ResourceType Type { get; set; }
    [BindProperty]
    public string? ResourceId { get; set; }
    [BindProperty]
    public string? Name { get; set; }
    [BindProperty]
    public bool CreateNew { get; set; }
    public string ResourceName { get; set; }

    public List<TeamResource> Teams { get; set; } = new();

    public async Task OnGet()
    {
        CurrentStep = Phase.StartEntry;
    }

    public async Task<IActionResult> OnPost()
    {
        if (CurrentStep == Phase.StartEntry)
        {
            if (Type == ResourceType.MicrosoftTeam.Value)
                CurrentStep = Phase.MicrosoftTeamsSelection;

            if (Type == ResourceType.CanvasCourse.Value)
                CurrentStep = Phase.CanvasCourseSelection;

            PreviousSteps.Add(Phase.StartEntry);
        }

        if (CurrentStep == Phase.MicrosoftTeamsSelection || PreviousSteps.Contains(Phase.MicrosoftTeamsSelection))
        {
            _logger.Information("Requested to retrieve defaults for new Team Resource in Offering by user {User}", _currentUserService.UserName);

            Result<List<TeamResource>> teamRequest = await _mediator.Send(new GetAllTeamsQuery());

            if (teamRequest.IsFailure)
            {
                _logger
                    .ForContext(nameof(Error), teamRequest.Error, true)
                    .Warning("Failed to retrieve defaults for new Team Resource in Offering by user {User}", _currentUserService.UserName);

                ModalContent = new ErrorDisplay(
                    teamRequest.Error,
                    _linkGenerator.GetPathByPage("/Subject/Offerings/Details", values: new { area = "Staff", Id = Id }));

                return Page();
            }

            Teams = teamRequest.Value;

            if (!string.IsNullOrWhiteSpace(ResourceId))
            {
                ResourceName = ResourceId;
                Name = ResourceId;
            }

            if (CurrentStep == Phase.MicrosoftTeamsSelection && !string.IsNullOrWhiteSpace(ResourceId))
            {
                PreviousSteps.Add(CurrentStep);
                CurrentStep = Phase.DataEntry;
            }
        }

        if (CurrentStep == Phase.CanvasCourseSelection && string.IsNullOrWhiteSpace(ResourceId))
        {
            _logger.Information("Requested to retrieve defaults for new Canvas Resource in Offering by user {User}", _currentUserService.UserName);

            Result<OfferingDetailsResponse> offeringRequest = await _mediator.Send(new GetOfferingDetailsQuery(Id));

            if (offeringRequest.IsFailure)
            {
                _logger
                    .ForContext(nameof(Error), offeringRequest.Error, true)
                    .Warning("Failed to retrieve defaults for new Canvas Resource in Offering by user {User}", _currentUserService.UserName);

                ModalContent = new ErrorDisplay(
                    offeringRequest.Error,
                    _linkGenerator.GetPathByPage("/Subject/Offerings/Details", values: new { area = "Staff", Id = Id }));

                return Page();
            }

            Result<CourseSummaryResponse> courseRequest = await _mediator.Send(new GetCourseSummaryQuery(offeringRequest.Value.CourseId));

            if (courseRequest.IsFailure)
            {
                _logger
                    .ForContext(nameof(Error), courseRequest.Error, true)
                    .Warning("Failed to retrieve defaults for new Canvas Resource in Offering by user {User}", _currentUserService.UserName);

                ModalContent = new ErrorDisplay(
                    courseRequest.Error,
                    _linkGenerator.GetPathByPage("/Subject/Offerings/Details", values: new { area = "Staff", Id = Id }));

                return Page();
            }

            string year = offeringRequest.Value.EndDate.Year.ToString();
            string grade = courseRequest.Value.Grade.ToString()[1..].PadLeft(2, '0');
            string code = (string.IsNullOrWhiteSpace(courseRequest.Value.Code) ? "XXX" : courseRequest.Value.Code);

            ResourceId = $"{year}-{grade}{code}";
            Name = $"{year} {courseRequest.Value.Grade.AsName()} {courseRequest.Value.Name}";
        }
        else if (CurrentStep == Phase.CanvasCourseSelection)
        {
            PreviousSteps.Add(CurrentStep);
            CurrentStep = Phase.DataEntry;
        }

        return Page();
    }

    public async Task<IActionResult> OnPostFinalSubmit()
    {
        // Send resource creation command
        if (Type == ResourceType.MicrosoftTeam)
        {
            Result<TeamResource> teamRequest = await _mediator.Send(new GetTeamByNameQuery(ResourceId));

            if (teamRequest.IsFailure)
            {
                _logger
                    .ForContext(nameof(Error), teamRequest.Error, true)
                    .Warning("Failed to create new Team Resource for Offering by user {User}", _currentUserService.UserName);

                ModalContent = new ErrorDisplay(
                    teamRequest.Error,
                    _linkGenerator.GetPathByPage("/Subject/Offerings/Resource", values: new { area = "Staff", Id = Id }));

                return Page();
            }

            AddResourceToOfferingCommand command = new(Id, Type, Name, teamRequest.Value.Link, ResourceId);

            _logger
                .ForContext(nameof(AddResourceToOfferingCommand), command, true)
                .Information("Requested to create new Team Resource for Offering by user {User}", _currentUserService.UserName);

            Result request = await _mediator.Send(command);

            if (request.IsFailure)
            {
                _logger
                    .ForContext(nameof(Error), request.Error, true)
                    .Warning("Failed to create new Team Resource for Offering by user {User}", _currentUserService.UserName);

                ModalContent = new ErrorDisplay(
                    request.Error,
                    _linkGenerator.GetPathByPage("/Subject/Offerings/Resource", values: new { area = "Staff", Id = Id }));

                return Page();
            }
        }

        if (Type == ResourceType.CanvasCourse)
        {
            AddResourceToOfferingCommand command = new(Id, Type, Name, string.Empty, ResourceId);

            _logger
                .ForContext(nameof(AddResourceToOfferingCommand), command, true)
                .Information("Requested to create new Canvas Resource for Offering by user {User}", _currentUserService.UserName);

            Result request = await _mediator.Send(command);

            if (request.IsFailure)
            {
                _logger
                    .ForContext(nameof(Error), request.Error, true)
                    .Warning("Failed to create new Canvas Resource for Offering by user {User}", _currentUserService.UserName);
                
                ModalContent = new ErrorDisplay(
                    request.Error,
                    _linkGenerator.GetPathByPage("/Subject/Offerings/Resource", values: new { area = "Staff", Id = Id }));

                return Page();
            }
        }

        return RedirectToPage("/Subject/Offerings/Details", new { area = "Staff", Id = Id });
    }

    public enum Phase
    {
        StartEntry,
        MicrosoftTeamsSelection,
        CanvasCourseSelection,
        DataEntry
    }
}
