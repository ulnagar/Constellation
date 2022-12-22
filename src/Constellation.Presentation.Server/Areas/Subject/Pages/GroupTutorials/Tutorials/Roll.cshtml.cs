namespace Constellation.Presentation.Server.Areas.Subject.Pages.GroupTutorials.Tutorials;

using Constellation.Application.GroupTutorials.GetTutorialRollWithDetailsById;
using Constellation.Presentation.Server.BaseModels;
using MediatR;
using Microsoft.AspNetCore.Mvc;

public class RollModel : BasePageModel
{
    private readonly IMediator _mediator;

    public RollModel(IMediator mediator)
    {
        _mediator = mediator;
    }

    [BindProperty(SupportsGet = true)]
    public Guid TutorialId { get; set; }

    [BindProperty(SupportsGet = true)]
    public Guid RollId { get; set; }

    [BindProperty(SupportsGet = true)]
    public string Mode { get; set; }

    public async Task<IActionResult> OnGet()
    {
        await GetClasses(_mediator);

        var roll = await _mediator.Send(new GetTutorialRollWithDetailsByIdQuery(TutorialId, RollId));

        return Page();
    }
}
