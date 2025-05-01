namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Shared.Components.EnrolStudentInOffering;

using Application.Domains.Students.Queries.GetCurrentStudentsAsDictionary;
using Constellation.Application.Domains.Offerings.Queries.GetOfferingDetails;
using Constellation.Core.Models.Offerings.Identifiers;
using Constellation.Core.Shared;
using Core.Models.Students.Identifiers;
using MediatR;
using Microsoft.AspNetCore.Mvc;

public class EnrolStudentInOfferingViewComponent : ViewComponent
{
    private readonly ISender _mediator;

    public EnrolStudentInOfferingViewComponent(
        ISender mediator)
    {
        _mediator = mediator;
    }

    public async Task<IViewComponentResult> InvokeAsync(
        OfferingId id)
    {
        Result<OfferingDetailsResponse> offering = await _mediator.Send(new GetOfferingDetailsQuery(id));
        Result<Dictionary<StudentId, string>> studentResult = await _mediator.Send(new GetCurrentStudentsAsDictionaryQuery());

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