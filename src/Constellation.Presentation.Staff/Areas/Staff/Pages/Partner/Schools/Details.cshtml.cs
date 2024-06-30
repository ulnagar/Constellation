namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Partner.Schools;

using Application.DTOs;
using Application.Models.Auth;
using Application.Schools.GetSchoolDetails;
using Constellation.Application.Features.API.Schools.Queries;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System.Net.Http.Json;

[Authorize(Policy = AuthPolicies.IsStaffMember)]
public class DetailsModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly LinkGenerator _linkGenerator;

    public DetailsModel(
        ISender mediator,
        LinkGenerator linkGenerator)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.Partner_Schools_Schools;

    [BindProperty(SupportsGet = true)]
    public string Id { get; set; }

    public SchoolDetailsResponse School { get; set; }

    public async Task OnGet()
    {
        Result<SchoolDetailsResponse> request = await _mediator.Send(new GetSchoolDetailsQuery(Id));

        if (request.IsFailure)
        {
            Error = new()
            {
                Error = request.Error,
                RedirectPath = _linkGenerator.GetPathByPage("/Partner/Schools/Index", values: new { area = "Staff" })
            };

            return;
        }

        School = request.Value;
    }

    public async Task<IActionResult> OnGetAjaxGetGraphData(string id, int day)
    {
        GraphData data = await _mediator.Send(new GetGraphDataForSchoolQuery { SchoolCode = id, Day = day });

        return new JsonResult(data);
    }
}