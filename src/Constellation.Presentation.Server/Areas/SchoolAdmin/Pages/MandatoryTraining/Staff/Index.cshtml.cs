namespace Constellation.Presentation.Server.Areas.SchoolAdmin.Pages.MandatoryTraining.Staff;

using Constellation.Application.Interfaces.Services;
using Constellation.Application.MandatoryTraining.GetListOfCertificatesForStaffMemberWithNotCompletedModules;
using Constellation.Application.MandatoryTraining.Models;
using Constellation.Application.Models.Auth;
using Constellation.Presentation.Server.BaseModels;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

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
    public string StaffId { get; set; }

    public StaffCompletionListDto CompletionList { get; set; }

    public async Task OnGet()
    {
        ViewData["ActivePage"] = "Staff";
        ViewData["StaffId"] = User.Claims.First(claim => claim.Type == AuthClaimType.StaffEmployeeId)?.Value;

        await GetClasses(_mediator);

        var completionRequest = await _mediator.Send(new GetListOfCertificatesForStaffMemberWithNotCompletedModulesQuery(StaffId));

        if (completionRequest.IsFailure)
        {
            Error = new ErrorDisplay
            {
                Error = completionRequest.Error,
                RedirectPath = _linkGenerator.GetPathByPage("/Dashboard", values: new { area = "Home" })
            };

            return;
        }

        CompletionList = completionRequest.Value;
    }
}
