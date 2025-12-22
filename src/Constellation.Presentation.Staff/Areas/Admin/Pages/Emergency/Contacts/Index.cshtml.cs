namespace Constellation.Presentation.Staff.Areas.Admin.Pages.Emergency.Contacts;

using Application.Domains.EmergencyConsole.Queries.GetContactDetails;
using Application.Models.Auth;
using Constellation.Core.Abstractions.Services;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Serilog;

[Authorize(Policy = AuthPolicies.CanUseEmergencyConsole)]
public class IndexModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly LinkGenerator _linkGenerator;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public IndexModel(
        ISender mediator,
        LinkGenerator linkGenerator,
        ICurrentUserService currentUserService,
        ILogger logger)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
        _currentUserService = currentUserService;
        _logger = logger
            .ForContext<IndexModel>();
    }

    [ViewData]
    public string ActivePage => Staff.Pages.Shared.Components.StaffSidebarMenu.ActivePage.Admin_Emergency_Contacts;

    [ViewData]
    public string PageTitle => "Contacts";

    public List<ContactDetail> Contacts { get; set; } = [];

    public async Task OnGet()
    {
        Result<List<ContactDetail>> contacts = await _mediator.Send(new GetContactDetailsQuery());

        if (contacts.IsFailure)
        {
            return;
        }

        Contacts = contacts.Value;
    }
}