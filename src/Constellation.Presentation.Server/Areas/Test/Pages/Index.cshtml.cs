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
    
    public async Task OnGet()
    {
        await GetClasses(_mediator);
    }
}