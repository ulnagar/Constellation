namespace Constellation.Presentation.Server.Areas.Test.Pages;

using BaseModels;
using MediatR;

public class IndexModel : BasePageModel
{
    private readonly IMediator _mediator;

    public IndexModel(
        IMediator mediator)
    {
        _mediator = mediator;
    }

    public void OnGet() { }

    public async Task OnGetRunCode()
    {
        return;
    }
}