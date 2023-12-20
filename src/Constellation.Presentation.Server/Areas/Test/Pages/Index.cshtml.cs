namespace Constellation.Presentation.Server.Areas.Test.Pages;

using BaseModels;
using Core.Models.Awards.Events;
using Core.Models.Identifiers;
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

        await _mediator.Publish(new AwardMatchedToIncidentDomainEvent(new(),
            StudentAwardId.FromValue(Guid.Parse("d30d3332-9b47-4d55-9ed9-fb7d56be7e0c"))));
    }
}