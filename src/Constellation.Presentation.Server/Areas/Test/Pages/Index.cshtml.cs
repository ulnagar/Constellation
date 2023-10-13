namespace Constellation.Presentation.Server.Areas.Test.Pages;

using Constellation.Presentation.Server.BaseModels;
using MediatR;

public class IndexModel : BasePageModel
{
    private readonly ISender _mediator;

    public IndexModel(ISender mediator)
    {
        _mediator = mediator;
    }

    public async Task OnGetAsync()
    {
        await GetClasses(_mediator);
    }
}
