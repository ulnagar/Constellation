namespace Constellation.Presentation.Shared.ViewComponents;

using Application.Families.GetFamilyById;
using Application.Families.Models;
using Application.Students.GetCurrentStudentsAsDictionary;
using Constellation.Presentation.Shared.Pages.Shared.Components.FamilyAddStudent;
using Core.Models.Identifiers;
using Core.Shared;
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

    public async Task<IViewComponentResult> InvokeAsync(FamilyId familyId)
    {
        Result<FamilyResponse> familyResult = await _mediator.Send(new GetFamilyByIdQuery(familyId));
        Result<Dictionary<string, string>> studentResult =  await _mediator.Send(new GetCurrentStudentsAsDictionaryQuery());

        FamilyAddStudentSelection viewModel = new()
        {
            FamilyId = familyId,
            FamilyName = familyResult.Value?.FamilyName,
            Students = studentResult.Value
        };

        return View(viewModel);
    }
}
