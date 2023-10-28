namespace Constellation.Presentation.Server.Areas.Admin.Pages.Rollover;

using Application.Models.Auth;
using BaseModels;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize(Policy = AuthPolicies.IsSiteAdmin)]
public class ImportModel : BasePageModel
{
    private readonly ISender _mediator;

    public ImportModel(
        ISender mediator)
    {
        _mediator = mediator;
    }

    public async Task OnGet()
    {
        await GetClasses(_mediator);
    }

    public async Task<IActionResult> OnPost()
    {

    }
}