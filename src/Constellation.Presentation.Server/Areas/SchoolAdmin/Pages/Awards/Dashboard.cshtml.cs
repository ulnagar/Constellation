namespace Constellation.Presentation.Server.Areas.SchoolAdmin.Pages.Awards;

using Constellation.Application.Awards.GetAwardCountsByTypeByGrade;
using Constellation.Application.Models.Auth;
using Constellation.Presentation.Server.BaseModels;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize(Policy = AuthPolicies.IsStaffMember)]
public class DashboardModel : BasePageModel
{
    private readonly IMediator _mediator;

    public DashboardModel(
        IMediator mediator)
    {
        _mediator = mediator;
    }

    [ViewData] public string ActivePage => AwardsPages.Dashboard;


    public async Task OnGet() { }

    public async Task<IActionResult> OnPostGetData(CancellationToken cancellationToken = default)
    {
        Result<List<AwardCountByTypeByGradeResponse>> request = await _mediator.Send(new GetAwardCountsByTypeByGradeQuery(DateTime.Today.Year), cancellationToken);

        if (request.IsFailure)
        {
            return new JsonResult(null);
        }

        return new JsonResult(request.Value);
    }
}
