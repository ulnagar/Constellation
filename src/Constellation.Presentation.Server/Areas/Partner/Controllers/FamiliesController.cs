namespace Constellation.Presentation.Server.Areas.Partner.Controllers;

using Constellation.Application.Families.GetFamilyById;
using Constellation.Application.Families.Models;
using Constellation.Core.Models.Identifiers;
using Constellation.Presentation.Server.BaseModels;
using Constellation.Presentation.Server.Pages.Shared.Components.FamilyDelete;
using MediatR;
using Microsoft.AspNetCore.Mvc;

[Area("Partner")]
public class FamiliesController : BaseController
{
    private readonly IMediator _mediator;

    public FamiliesController(IMediator mediator) 
        : base(mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("DeleteFamily")]
    public async Task<IActionResult> GetFamilyDeleteViewComponent(Guid familyId, Guid parentId)
    {
        var familyIdent = FamilyId.FromValue(familyId);
        var parentIdent = ParentId.FromValue(parentId);

        var family = await _mediator.Send(new GetFamilyByIdQuery(familyIdent));

        if (family.IsFailure)
        {
            return ViewComponent("FamilyDelete", new FamilyDeleteSelection());
        }

        var parent = family.Value.Parents.FirstOrDefault(parent => parent.ParentId == parentIdent);

        var viewModel = new FamilyDeleteSelection
        {
            FamilyId = familyIdent,
            FamilyName = family.Value.FamilyName,
            ParentId = parentIdent,
            ParentName = parent?.ParentName,
            OtherParentNames = family.Value.Parents.Except(new List<ParentResponse> { parent }).Select(parent => parent.ParentName).ToList()
        };

        return ViewComponent("FamilyDelete", viewModel);
    }

    [HttpPost]
    public async Task<IActionResult> GetFamilyDeleteContent(Guid familyId, Guid parentId)
    {
        var familyIdent = FamilyId.FromValue(familyId);
        var parentIdent = ParentId.FromValue(parentId);

        var family = await _mediator.Send(new GetFamilyByIdQuery(familyIdent));

        if (family.IsFailure)
        {
            return PartialView("FamilyDeleteSelection", new FamilyDeleteSelection());
        }

        var parent = family.Value.Parents.FirstOrDefault(parent => parent.ParentId == parentIdent);

        var viewModel = new FamilyDeleteSelection
        {
            FamilyId = familyIdent,
            FamilyName = family.Value.FamilyName,
            ParentId = parentIdent,
            ParentName = parent?.ParentName,
            OtherParentNames = family.Value.Parents.Except(new List<ParentResponse> { parent }).Select(parent => parent.ParentName).ToList()
        };

        return PartialView("FamilyDeleteSelection", viewModel);
    }

    [HttpPost]
    public async Task<IActionResult> GetFamilyDeleteConfirmation(Guid familyId)
    {
        var familyIdent = FamilyId.FromValue(familyId);

        var family = await _mediator.Send(new GetFamilyByIdQuery(familyIdent));

        if (family.IsFailure)
        {
            return PartialView("FamilyDeleteConfirmation", new FamilyDeleteSelection());
        }

        var viewModel = new FamilyDeleteSelection
        {
            FamilyId = familyIdent,
            FamilyName = family.Value.FamilyName,
            OtherParentNames = family.Value.Parents.Select(parent => parent.ParentName).ToList()
        };

        return PartialView("FamilyDeleteConfirmation", viewModel);
    }
}
