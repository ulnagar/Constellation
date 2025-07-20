namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Subject.Tutorials;

using Application.Domains.Tutorials.Queries.GetAllTutorials;
using Application.Models.Auth;
using Constellation.Application.Common.PresentationModels;
using Constellation.Core.Abstractions.Services;
using Constellation.Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Presentation.Shared.Helpers.Logging;
using Serilog;

[Authorize(Policy = AuthPolicies.IsStaffMember)]
public class IndexModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public IndexModel(
        ISender mediator,
        ICurrentUserService currentUserService,
        ILogger logger)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
        _logger = logger
            .ForContext<IndexModel>()
            .ForContext(LogDefaults.Application, LogDefaults.StaffPortal);
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.Subject_Tutorials_Tutorials;
    [ViewData] public string PageTitle => "Tutorials";

    [BindProperty(SupportsGet = true)]
    public GetAllTutorialsQuery.FilterEnum Filter { get; set; } = GetAllTutorialsQuery.FilterEnum.Active;

    public List<TutorialSummaryResponse> Tutorials { get; set; } = [];

    public async Task OnGet()
    {
        _logger.Information("Requested to retrieve list of Tutorials by user {User}", _currentUserService.UserName);

        Result<List<TutorialSummaryResponse>> tutorialResponse = await _mediator.Send(new GetAllTutorialsQuery(Filter));

        if (tutorialResponse.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), tutorialResponse.Error, true)
                .Warning("Failed to retrieve list of Group Tutorials by user {User}", _currentUserService.UserName);

            ModalContent = ErrorDisplay.Create(tutorialResponse.Error);

            return;
        }

        Tutorials = tutorialResponse.Value.OrderBy(tutorial => tutorial.Name).ToList();
    }
}