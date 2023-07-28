namespace Constellation.Presentation.Server.Areas.SchoolAdmin.Pages.Awards.Nominations;

using Constellation.Presentation.Server.BaseModels;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

public class DetailsModel : BasePageModel
{
    private readonly IMediator _mediator;

    public DetailsModel(
        IMediator mediator)
    {
        _mediator = mediator;
    }

    [ViewData]
    public string ActivePage => "Nominations";


    public async Task OnGet()
    {
        await GetClasses(_mediator);
    }
}
