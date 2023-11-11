namespace Constellation.Presentation.Server.Areas.SchoolAdmin.Pages.Compliance.Wellbeing;

using Application.Compliance.GetWellbeingReportFromSentral;
using Application.Models.Auth;
using Constellation.Application.DTOs;
using Constellation.Presentation.Server.BaseModels;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize(Policy = AuthPolicies.IsStaffMember)]
public class IndexModel : BasePageModel
{
    private readonly ISender _mediator;

    public IndexModel(
        ISender mediator)
    {
        _mediator = mediator;
    }

    [ViewData] public string ActivePage { get; set; } = CompliancePages.Wellbeing_Index;

    public List<SentralIncidentDetails> Data { get; set; } = new();

    public async Task OnGet() => await GetClasses(_mediator);

    public async Task OnPost()
    {
        Result<List<SentralIncidentDetails>> request = await _mediator.Send(new GetWellbeingReportFromSentralQuery());

        if (request.IsFailure)
        {
            Error = new()
            {
                Error = request.Error,
                RedirectPath = null
            };

            return;
        }

        Data = request.Value;
    }
}