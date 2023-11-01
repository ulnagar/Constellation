namespace Constellation.Presentation.Server.Areas.Admin.Pages.Rollover;

using Application.Models.Auth;
using BaseModels;
using MediatR;
using Microsoft.AspNetCore.Authorization;

[Authorize(Policy = AuthPolicies.IsSiteAdmin)]
public class IndexModel : BasePageModel
{
    private readonly ISender _mediator;

    public IndexModel(
        ISender mediator)
    {
        _mediator = mediator;
    }

    public async Task OnGet()
    {
        await GetClasses(_mediator);
    }
}