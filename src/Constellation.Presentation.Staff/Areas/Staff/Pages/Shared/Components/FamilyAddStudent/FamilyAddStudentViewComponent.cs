namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Shared.Components.FamilyAddStudent;

using Constellation.Application.Families.GetFamilyById;
using Constellation.Application.Families.Models;
using Constellation.Application.Students.GetCurrentStudentsAsDictionary;
using Constellation.Core.Models.Identifiers;
using Constellation.Core.Shared;
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
        Result<Dictionary<string, string>> studentResult = await _mediator.Send(new GetCurrentStudentsAsDictionaryQuery());

        FamilyAddStudentSelection viewModel = new()
        {
            FamilyId = familyId,
            FamilyName = familyResult.Value?.FamilyName,
            Students = studentResult.Value
        };

        return View(viewModel);
    }
}
