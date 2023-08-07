namespace Constellation.Presentation.Server.Areas.Subject.Pages.SciencePracs.Lessons;

using Constellation.Presentation.Server.BaseModels;
using MediatR;
using Microsoft.AspNetCore.Mvc;

public class UpsertModel : BasePageModel
{
    private readonly IMediator _mediator;

    public UpsertModel(
        IMediator mediator)
    {
        _mediator = mediator;
    }

    [ViewData]
    public string ActivePage => "Lessons";

    public async Task OnGet()
    {
        await GetClasses(_mediator);
    }
}
