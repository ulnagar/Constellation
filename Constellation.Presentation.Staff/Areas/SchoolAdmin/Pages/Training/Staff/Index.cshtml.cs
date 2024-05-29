namespace Constellation.Presentation.Staff.Areas.SchoolAdmin.Pages.Training.Staff;

using Constellation.Application.Common.PresentationModels;
using Constellation.Application.Models.Auth;
using Constellation.Application.Training.Modules.GetModuleStatusByStaffMember;
using Constellation.Core.Shared;
using Constellation.Presentation.Staff.Areas;
using Constellation.Presentation.Staff.Areas.SchoolAdmin.Pages.Training;
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

    [BindProperty(SupportsGet = true)]
    [ViewData]
    public string StaffId { get; set; }

    public List<ModuleStatusResponse> Modules { get; set; }

    [ViewData] public string ActivePage { get; set; } = TrainingPages.Staff;

    public async Task OnGet()
    {
        Result<List<ModuleStatusResponse>> completionRequest = await _mediator.Send(new GetModuleStatusByStaffMemberQuery(StaffId));

        if (completionRequest.IsFailure)
        {
            Error = new ErrorDisplay
            {
                Error = completionRequest.Error,
                RedirectPath = _linkGenerator.GetPathByPage("/Dashboard", values: new { area = "Home" })
            };

            return;
        }

        Modules = completionRequest.Value;
    }
}
