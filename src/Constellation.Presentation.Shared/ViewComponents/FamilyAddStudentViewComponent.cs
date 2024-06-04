namespace Constellation.Presentation.Shared.ViewComponents;

using Constellation.Application.Families.GetFamilyById;
using Constellation.Application.Students.GetCurrentStudentsAsDictionary;
using Constellation.Core.Models.Identifiers;
using Constellation.Presentation.Shared.Pages.Shared.Components.FamilyAddStudent;
using MediatR;
using Microsoft.AspNetCore.Mvc;

public class FamilyAddStudentViewComponent : ViewComponent
{
    private readonly IMediator _mediator;

    public FamilyAddStudentViewComponent(
        IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task<IViewComponentResult> InvokeAsync(Guid familyId)
    {
        var familyIdent = FamilyId.FromValue(familyId);
        var familyResult = await _mediator.Send(new GetFamilyByIdQuery(familyIdent));
        var studentResult =  await _mediator.Send(new GetCurrentStudentsAsDictionaryQuery());

        var viewModel = new FamilyAddStudentSelection
        {
            FamilyId = familyIdent,
            FamilyName = familyResult.Value?.FamilyName,
            Students = studentResult.Value
        };

        return View(viewModel);
    }
}
