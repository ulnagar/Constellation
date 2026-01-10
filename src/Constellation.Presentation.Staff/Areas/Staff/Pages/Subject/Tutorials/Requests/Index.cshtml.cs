namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Subject.Tutorials.Requests;

using Application.Common.PresentationModels;
using Application.Domains.Tutorials.Requests.Queries.GetAllTutorialRequests;
using Application.Models.Auth;
using Core.Abstractions.Clock;
using Core.Abstractions.Services;
using Core.Models.Tutorials.Enums;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Presentation.Shared.Helpers.Attributes;
using Presentation.Shared.Helpers.Logging;
using Serilog;

[HasPermission(AuthPermission.Subjects_Tutorials_View_Value)]
public class IndexModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly LinkGenerator _linkGenerator;
    private readonly IDateTimeProvider _dateTime;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public IndexModel(
        ISender mediator,
        LinkGenerator linkGenerator,
        IDateTimeProvider dateTime,
        ICurrentUserService currentUserService,
        ILogger logger)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
        _dateTime = dateTime;
        _currentUserService = currentUserService;
        _logger = logger
            .ForContext<IndexModel>()
            .ForContext(LogDefaults.Application, LogDefaults.StaffPortal);
    }

    [ViewData]
    public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.Subject_Tutorials_Requests;

    [ViewData]
    public string PageTitle => "Tutorial Requests";

    [BindProperty(SupportsGet = true)]
    public string Year { get; set; }

    public List<string> Years { get; set; } = [];

    [BindProperty(SupportsGet = true)]
    public FilterDto Filter { get; set; } = FilterDto.Pending;

    public List<TutorialRequestSummaryResponse> Requests { get; set; } = [];

    public async Task OnGet()
    {
        if (string.IsNullOrWhiteSpace(Year))
            Year = _dateTime.CurrentYearAsString;

        Result<List<TutorialRequestSummaryResponse>> tutorialRequests = await _mediator.Send(new GetAllTutorialRequestsQuery());

        if (tutorialRequests.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), tutorialRequests.Error, true)
                .Warning("Failed to retrieve list of Tutorial Requests for user {User}", _currentUserService.UserName);

            ModalContent = ErrorDisplay.Create(tutorialRequests.Error);

            return;
        }

        Years = tutorialRequests.Value
            .Select(request => request.Year.ToString())
            .Distinct()
            .OrderDescending()
            .ToList();

        Requests = Filter switch
        {
            FilterDto.All => tutorialRequests.Value
                .Where(request => request.Year.ToString() == Year)
                .ToList(),

            FilterDto.Complete => tutorialRequests.Value
                .Where(request =>
                    request.Year.ToString() == Year && 
                    (request.Status == RequestStatus.Scheduled || request.Status == RequestStatus.Rejected))
                .ToList(),

            FilterDto.Pending => tutorialRequests.Value
                .Where(request =>
                    request.Year.ToString() == Year && 
                    (request.Status == RequestStatus.Requested || request.Status == RequestStatus.Approved))
                .ToList(),

            _ => tutorialRequests.Value
                .Where(request => request.Year.ToString() == Year)
                .ToList()
        };
    }

    public enum FilterDto
    {
        All,
        Pending,
        Complete
    }
}