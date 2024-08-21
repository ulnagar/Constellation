namespace Constellation.Presentation.Server.Areas.Test.Pages;
using BaseModels;
using MediatR;

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

    }

}