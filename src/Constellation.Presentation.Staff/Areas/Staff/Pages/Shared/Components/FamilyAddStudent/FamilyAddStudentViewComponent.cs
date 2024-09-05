namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Shared.Components.FamilyAddStudent;

using Constellation.Application.Families.GetFamilyById;
using Constellation.Application.Families.Models;
using Constellation.Application.Students.GetCurrentStudentsAsDictionary;
using Constellation.Core.Models.Identifiers;
using Constellation.Core.Shared;
using Core.Models.Students.Identifiers;
using MediatR;
using Microsoft.AspNetCore.Mvc;

public class FamilyAddStudentViewComponent : ViewComponent
{
    private readonly ISender _mediator;

    public FamilyAddStudentViewComponent(
        ISender mediator)
    {
        _mediator = mediator;
    }

    public async Task<IViewComponentResult> InvokeAsync(FamilyId familyId)
    {
        Result<FamilyResponse> familyResult = await _mediator.Send(new GetFamilyByIdQuery(familyId));

        if (familyResult.IsFailure)
        {
            return Content(string.Empty);
        }

        Result<Dictionary<StudentId, string>> studentResult = await _mediator.Send(new GetCurrentStudentsAsDictionaryQuery());

        if (studentResult.IsFailure)
        {
            return Content(string.Empty);
        }

        FamilyAddStudentSelection viewModel = new()
        {
            FamilyId = familyId,
            FamilyName = familyResult.Value.FamilyName,
            Students = studentResult.Value
        };

        return View(viewModel);
    }
}
