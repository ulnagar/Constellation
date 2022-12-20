namespace Constellation.Presentation.Server.Areas.Subject.Pages.GroupTutorials.Tutorials;

using Constellation.Presentation.Server.BaseModels;
using MediatR;

public class IndexModel : BasePageModel
{
    private readonly IMediator _mediator;

    public IndexModel(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task OnGet()
    {
        await GetClasses(_mediator);
    }
}
