namespace Constellation.Presentation.Server.Areas.Partner.Pages.Faculties;

using Application.Faculties.GetFacultiesSummary;
using Constellation.Application.Models.Auth;
using Constellation.Presentation.Server.BaseModels;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;

[Authorize(Policy = AuthPolicies.IsStaffMember)]
public class IndexModel : BasePageModel
{
    private readonly IMediator _mediator;

    public IndexModel(IMediator mediator)
    {
        _mediator = mediator;
    }

    public List<FacultySummaryResponse> Faculties { get; set; } = new();

    public async Task OnGet()
    {
        await GetClasses(_mediator);

        Result<List<FacultySummaryResponse>> faculties = await _mediator.Send(new GetFacultiesSummaryQuery());

        if (faculties.IsFailure)
        {
            Error = new()
            {
                Error = faculties.Error,
                RedirectPath = null
            };

            return;
        }

        Faculties = faculties.Value;
    }
}
