namespace Constellation.Presentation.Server.Areas.Subject.Pages.GroupTutorials.Tutorials;

using Constellation.Application.GroupTutorials.GetAllTutorials;
using Constellation.Application.Models.Auth;
using Constellation.Presentation.Server.BaseModels;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize(Policy = AuthPolicies.CanViewGroupTutorials)]
public class IndexModel : BasePageModel
{
    private readonly IMediator _mediator;

    public IndexModel(IMediator mediator)
    {
        _mediator = mediator;
    }

    [BindProperty(SupportsGet = true)]
    public FilterDto Filter { get; set; } = FilterDto.Active;

    public List<GroupTutorialSummaryResponse> Tutorials { get; set; } = new();

    public async Task OnGet()
    {
        await GetClasses(_mediator);

        var tutorialResponse = await _mediator.Send(new GetAllTutorialsQuery());

        if (tutorialResponse.IsFailure) 
        {
            // Something has seriously gone wrong since this method can't return failure!
            return;
        }

        Tutorials = tutorialResponse.Value;

        Tutorials = Filter switch
        {
            FilterDto.All => Tutorials,
            FilterDto.Active => Tutorials.Where(tutorial => tutorial.StartDate <= DateOnly.FromDateTime(DateTime.Today) && tutorial.EndDate >= DateOnly.FromDateTime(DateTime.Today)).ToList(),
            FilterDto.Inactive => Tutorials.Where(tutorial => tutorial.EndDate < DateOnly.FromDateTime(DateTime.Today)).ToList(),
            FilterDto.Future => Tutorials.Where(tutorial => tutorial.StartDate > DateOnly.FromDateTime(DateTime.Today)).ToList(),
            _ => Tutorials
        };

        Tutorials = Tutorials.OrderBy(tutorial => tutorial.StartDate).ToList();
    }

    public enum FilterDto
    {
        All,
        Active,
        Inactive,
        Future
    }
}
