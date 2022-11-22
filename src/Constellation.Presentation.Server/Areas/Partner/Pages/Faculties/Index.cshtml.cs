namespace Constellation.Presentation.Server.Areas.Partner.Pages.Faculties;

using Constellation.Application.Features.Faculties.Models;
using Constellation.Application.Features.Faculties.Queries;
using Constellation.Application.Models.Auth;
using Constellation.Presentation.Server.BaseModels;
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

    public List<FacultySummaryDto> Faculties { get; set; } = new();

    public async Task OnGet()
    {
        await GetClasses(_mediator);

        Faculties = await _mediator.Send(new GetListOfFacultySummaryQuery());
    }
}
