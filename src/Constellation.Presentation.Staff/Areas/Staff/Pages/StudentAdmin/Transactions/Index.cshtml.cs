namespace Constellation.Presentation.Staff.Areas.Staff.Pages.StudentAdmin.Transactions;

using Core.Abstractions.Services;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Serilog;

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
    public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.StudentAdmin_Consent_Transactions;

    [ViewData] 
    public string PageTitle => "Consent Transactions";


    public async Task OnGet()
    {

    }
}