namespace Constellation.Presentation.Server.Areas.Admin.Pages.Rollover;

using Application.Models.Auth;
using BaseModels;
using Constellation.Presentation.Shared.Helpers.Attributes;
using MediatR;
using Microsoft.AspNetCore.Mvc;

[HasPermission(AuthPermission.Admin_Rollover_Edit_Value)]
public class IndexModel : BasePageModel
{
    private readonly ISender _mediator;

    public IndexModel(
        ISender mediator)
    {
        _mediator = mediator;
    }

    [ViewData] public string ActivePage => Models.ActivePage.Rollover;
    [ViewData] public string PageTitle => "Annual Rollover";

    public async Task OnGet() { }
}