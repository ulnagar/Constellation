namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Subject.Tutorials;

using Application.Domains.Tutorials.Queries.GetTutorialDetails;
using Application.Models.Auth;
using Constellation.Core.Abstractions.Services;
using Core.Models.Tutorials.Identifiers;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Presentation.Shared.Helpers.Logging;
using Serilog;

[Authorize(Policy = AuthPolicies.IsStaffMember)]
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
            .ForContext(LogDefaults.Application, LogDefaults.StaffPortal);
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.Subject_Tutorials_Tutorials;
    [ViewData] public string PageTitle => "Tutorials";

    [BindProperty(SupportsGet = true)]
    public TutorialId Id { get; set; } = TutorialId.Empty;

    public TutorialDetailsResponse Tutorial { get; set; }

    public async Task OnGet()
    {
        Result<TutorialDetailsResponse> response = await _mediator.Send(new GetTutorialDetailsQuery(Id));

        if (response.IsFailure)
        {

        }

        Tutorial = response.Value;
    }
}