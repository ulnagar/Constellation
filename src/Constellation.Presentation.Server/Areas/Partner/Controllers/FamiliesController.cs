namespace Constellation.Presentation.Server.Areas.Partner.Controllers;

using Application.Models.Auth;
using Constellation.Application.Families.GetFamilyById;
using Constellation.Application.Families.Models;
using Constellation.Core.Models.Identifiers;
using Constellation.Presentation.Server.Areas.Partner.Models.Families;
using Constellation.Presentation.Server.BaseModels;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize(Policy = AuthPolicies.IsStaffMember)]
[Area("Partner")]
public class FamiliesController : BaseController
{
    private readonly IMediator _mediator;
    private readonly LinkGenerator _linkGenerator;

    public FamiliesController(
        IMediator mediator,
        LinkGenerator linkGenerator) 
        : base(mediator)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
    }

    [HttpPost]
    public async Task<IActionResult> GetFamilyDeleteSelection(Guid familyId, Guid parentId)
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

    [HttpPost]
    public async Task<IActionResult> GetStudentDeleteConfirmation(string studentName, string studentId, string familyName, Guid familyId)
    {
        var link = _linkGenerator.GetPathByPage("/Families/Details", "RemoveStudent", new { area = "Partner", Id = familyId, StudentId = studentId });

        var viewModel = new DeleteConfirmation(
            "Remove Student from Family",
            studentName,
            familyName,
            link);

        return PartialView("DeleteConfirmation", viewModel);
    }

    [HttpPost]
    public async Task<IActionResult> GetParentDeleteConfirmation(string parentName, Guid parentId, string familyName, Guid familyId)
    {
        var link = _linkGenerator.GetPathByPage("/Families/Details", "RemoveParent", new { area = "Partner", Id = familyId, ParentId = parentId });

        var viewModel = new DeleteConfirmation(
            "Remove Parent from Family",
            parentName,
            familyName,
            link);

        return PartialView("DeleteConfirmation", viewModel);
    }
}
