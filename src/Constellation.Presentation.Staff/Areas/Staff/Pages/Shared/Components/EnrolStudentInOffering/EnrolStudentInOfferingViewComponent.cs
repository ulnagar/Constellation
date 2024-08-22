namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Shared.Components.EnrolStudentInOffering;

using Constellation.Application.Offerings.GetOfferingDetails;
using Constellation.Application.Students.GetCurrentStudentsAsDictionary;
using Constellation.Core.Models.Offerings.Identifiers;
using Constellation.Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Presentation.Shared.Helpers.ModelBinders;

public class EnrolStudentInOfferingViewComponent : ViewComponent
{
    private readonly ISender _mediator;

    public EnrolStudentInOfferingViewComponent(
        ISender mediator)
    {
        _mediator = mediator;
    }

    public async Task<IViewComponentResult> InvokeAsync(
        [ModelBinder(typeof(ConstructorBinder))] OfferingId id)
    {
        Result<OfferingDetailsResponse> offering = await _mediator.Send(new GetOfferingDetailsQuery(id));
        Result<Dictionary<string, string>> studentResult = await _mediator.Send(new GetCurrentStudentsAsDictionaryQuery());

        EnrolStudentInOfferingSelection viewModel = new()
        {
            OfferingId = id,
            CourseName = offering.Value.CourseName,
            OfferingName = offering.Value.Name,
            Students = studentResult.Value
        };

        return View(viewModel);
    }
}