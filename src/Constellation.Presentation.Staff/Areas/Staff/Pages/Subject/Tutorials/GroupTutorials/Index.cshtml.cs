namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Subject.Tutorials.GroupTutorials;

using Constellation.Application.Common.PresentationModels;
using Constellation.Application.Domains.GroupTutorials.Queries.GetAllTutorials;
using Constellation.Application.Models.Auth;
using Constellation.Core.Abstractions.Services;
using Constellation.Core.Shared;
using Constellation.Presentation.Shared.Helpers.Logging;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;

[Authorize(Policy = AuthPolicies.CanViewGroupTutorials)]
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

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.Subject_Tutorials_GroupTutorials;
    [ViewData] public string PageTitle => "Group Tutorials";

    [BindProperty(SupportsGet = true)]
    public GetAllTutorialsQuery.FilterEnum Filter { get; set; } = GetAllTutorialsQuery.FilterEnum.Active;

    public List<GroupTutorialSummaryResponse> Tutorials { get; set; } = new();

    public async Task OnGet()
    {
        _logger.Information("Requested to retrieve list of Group Tutorials by user {User}", _currentUserService.UserName);

        Result<List<GroupTutorialSummaryResponse>> tutorialResponse = await _mediator.Send(new GetAllTutorialsQuery { Filter = Filter });

        if (tutorialResponse.IsFailure) 
        {
            _logger
                .ForContext(nameof(Error), tutorialResponse.Error, true)
                .Warning("Failed to retrieve list of Group Tutorials by user {User}", _currentUserService.UserName);

            ModalContent = ErrorDisplay.Create(tutorialResponse.Error);

            return;
        }

        Tutorials = tutorialResponse.Value.OrderBy(tutorial => tutorial.StartDate).ToList();
    }
}
