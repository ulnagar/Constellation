namespace Constellation.Presentation.Server.ViewComponents;

using Constellation.Application.Families.GetFamilyById;
using Constellation.Application.Families.Models;
using Constellation.Core.Models.Identifiers;
using Constellation.Presentation.Server.Pages.Shared.Components.FamilyDelete;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

public class FamilyDeleteViewComponent : ViewComponent
{
    private readonly IMediator _mediator;

    public FamilyDeleteViewComponent(
        IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task<IViewComponentResult> InvokeAsync(ParentId parentId, FamilyId familyId)
    {
        var family = await _mediator.Send(new GetFamilyByIdQuery(familyId));

        if (family.IsFailure)
        {
            return View(new FamilyDeleteSelection());
        }

        var parent = family.Value.Parents.FirstOrDefault(parent => parent.ParentId == parentId);

        var viewModel = new FamilyDeleteSelection
        {
            FamilyId = familyId,
            FamilyName = family.Value.FamilyName,
            ParentId = parentId,
            ParentName = parent?.ParentName,
            OtherParentNames = family.Value.Parents.Except(new List<ParentResponse> { parent }).Select(parent => parent.ParentName).ToList()
        };

        return View(viewModel);
    }
}
