namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Subject.GroupTutorials.Tutorials;

using Constellation.Application.GroupTutorials.GetAllTutorials;
using Constellation.Application.Models.Auth;
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

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.Subject_GroupTutorials_Tutorials;

    [BindProperty(SupportsGet = true)]
    public GetAllTutorialsQuery.FilterEnum Filter { get; set; } = GetAllTutorialsQuery.FilterEnum.Active;

    public List<GroupTutorialSummaryResponse> Tutorials { get; set; } = new();

    public async Task OnGet()
    {
        var tutorialResponse = await _mediator.Send(new GetAllTutorialsQuery { Filter = Filter });

        if (tutorialResponse.IsFailure) 
        {
            // Something has seriously gone wrong since this method can't return failure!
            return;
        }

        Tutorials = tutorialResponse.Value.OrderBy(tutorial => tutorial.StartDate).ToList();
    }
}
