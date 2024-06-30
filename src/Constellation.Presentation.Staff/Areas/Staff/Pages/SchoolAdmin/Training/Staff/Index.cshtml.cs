namespace Constellation.Presentation.Staff.Areas.Staff.Pages.SchoolAdmin.Training.Staff;

using Constellation.Application.Models.Auth;
using Constellation.Application.Training.GetModuleStatusByStaffMember;
using Constellation.Core.Shared;
using Constellation.Presentation.Staff.Areas;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

[Authorize(Policy = AuthPolicies.IsStaffMember)]
public class IndexModel : BasePageModel
{
    private readonly IMediator _mediator;
    private readonly LinkGenerator _linkGenerator;

    public IndexModel(
        IMediator mediator,
        LinkGenerator linkGenerator)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.SchoolAdmin_Training_Staff;
    [ViewData] public string PageTitle => "Staff Training Dashboard";


    [BindProperty(SupportsGet = true)]
    public string StaffId { get; set; }

    public List<ModuleStatusResponse> Modules { get; set; }

    public async Task OnGet()
    {
        Result<List<ModuleStatusResponse>> completionRequest = await _mediator.Send(new GetModuleStatusByStaffMemberQuery(StaffId));

        if (completionRequest.IsFailure)
        {
            Error = new()
            {
                Error = completionRequest.Error,
                RedirectPath = _linkGenerator.GetPathByPage("/Dashboard", values: new { area = "Staff" })
            };

            return;
        }

        Modules = completionRequest.Value;
    }
}
