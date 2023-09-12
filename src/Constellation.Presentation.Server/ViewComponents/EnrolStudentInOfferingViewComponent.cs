namespace Constellation.Presentation.Server.ViewComponents;

using Constellation.Application.Offerings.GetOfferingDetails;
using Constellation.Application.Students.GetCurrentStudentsAsDictionary;
using Constellation.Core.Models.Offerings.Identifiers;
using Constellation.Core.Models.Subjects.Identifiers;
using Constellation.Core.Shared;
using Constellation.Presentation.Server.Pages.Shared.Components.EnrolStudentInOffering;
using MediatR;
using Microsoft.AspNetCore.Mvc;

public class EnrolStudentInOfferingViewComponent : ViewComponent
{
    private readonly IMediator _mediator;

    public EnrolStudentInOfferingViewComponent(
        IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task<IViewComponentResult> InvokeAsync(Guid Id)
    {
        OfferingId offeringId = OfferingId.FromValue(Id);
        Result<OfferingDetailsResponse> offering = await _mediator.Send(new GetOfferingDetailsQuery(offeringId));
        Result<Dictionary<string,string>> studentResult =  await _mediator.Send(new GetCurrentStudentsAsDictionaryQuery());

        var viewModel = new EnrolStudentInOfferingSelection
        {
            OfferingId = offeringId,
            CourseName = offering.Value.CourseName,
            OfferingName = offering.Value.Name,
            Students = studentResult.Value
        };

        return View(viewModel);
    }
}
