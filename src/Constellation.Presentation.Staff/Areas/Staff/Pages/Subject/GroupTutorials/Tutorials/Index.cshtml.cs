namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Subject.GroupTutorials.Tutorials;

using Application.Common.PresentationModels;
using Constellation.Application.GroupTutorials.GetAllTutorials;
using Constellation.Application.Models.Auth;
using Core.Abstractions.Services;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models;
using Presentation.Shared.Helpers.Logging;
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

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.Subject_GroupTutorials_Tutorials;
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

            ModalContent = new ErrorDisplay(tutorialResponse.Error);

            return;
        }

        Tutorials = tutorialResponse.Value.OrderBy(tutorial => tutorial.StartDate).ToList();
    }
}
