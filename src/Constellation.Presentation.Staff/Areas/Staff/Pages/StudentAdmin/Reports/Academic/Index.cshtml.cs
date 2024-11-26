namespace Constellation.Presentation.Staff.Areas.Staff.Pages.StudentAdmin.Reports.Academic;

using Application.Models.Auth;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Models;
using Serilog;
using System.Threading.Tasks;

[Authorize(Policy = AuthPolicies.IsStaffMember)]
public class IndexModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly LinkGenerator _linkGenerator;
    private readonly ILogger _logger;

    public IndexModel(
        ISender mediator,
        LinkGenerator linkGenerator,
        ILogger logger)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
        _logger = logger
            .ForContext<IndexModel>()
            .ForContext(StaffLogDefaults.Application, StaffLogDefaults.StaffPortal);
    }

    [ViewData] 
    public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.StudentAdmin_Reports_Academic;

    [ViewData] 
    public string PageTitle => "Academic Report List";


    public async Task OnGet()
    {

    }
}
