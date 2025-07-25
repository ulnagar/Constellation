namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Shared.Components.AddTeamToTutorial;

using Application.Domains.LinkedSystems.Teams.Models;
using Application.Domains.LinkedSystems.Teams.Queries.GetAllTeams;
using Constellation.Core.Shared;
using Core.Models.Tutorials.Identifiers;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public sealed class AddTeamToTutorialViewComponent : ViewComponent
{
    private readonly ISender _mediator;

    public AddTeamToTutorialViewComponent(
        ISender mediator)
    {
        _mediator = mediator;
    }

    public async Task<IViewComponentResult> InvokeAsync(TutorialId id)
    {
        Result<List<TeamResource>> teamResult = await _mediator.Send(new GetAllTeamsQuery());

        AddTeamToTutorialSelection viewModel = new()
        {
            TutorialId = id
        };

        foreach (TeamResource team in teamResult.Value.OrderBy(team => team.Name))
        {
            viewModel.Teams.Add(team.Id, team.Name);
        }

        return View(viewModel);
    }
}
