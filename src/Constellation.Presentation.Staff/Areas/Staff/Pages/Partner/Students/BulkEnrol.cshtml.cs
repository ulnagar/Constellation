namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Partner.Students;

using Constellation.Application.Enrolments.EnrolStudent;
using Constellation.Application.Enrolments.GetStudentEnrolmentsWithDetails;
using Constellation.Application.Models.Auth;
using Constellation.Application.Offerings.GetOfferingsForBulkEnrol;
using Constellation.Application.Students.GetStudentById;
using Constellation.Application.Students.Models;
using Constellation.Core.Models.Offerings.Identifiers;
using Constellation.Core.Shared;
using Constellation.Presentation.Staff.Areas;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

[Authorize(Policy = AuthPolicies.CanEditStudents)]
public class BulkEnrolModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly LinkGenerator _linkGenerator;

    public BulkEnrolModel(
        ISender mediator,
        LinkGenerator linkGenerator)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
    }

    [ViewData] public string ActivePage => Presentation.Staff.Pages.Shared.Components.StaffSidebarMenu.ActivePage.Partner_Students_Students;

    [BindProperty(SupportsGet = true)]
    public string Id { get; set; }

    public StudentResponse Student { get; set; }
    public List<BulkEnrolOfferingResponse> Offerings { get; set; } = new();
    public List<BulkEnrolOfferingResponse> ExistingEnrolments { get; set; } = new();

    [BindProperty]
    public List<Guid> SelectedOfferingIds { get; set; } = new();

    public async Task OnGet()
    {
        Result<StudentResponse> studentRequest = await _mediator.Send(new GetStudentByIdQuery(Id));

        if (studentRequest.IsFailure)
        {
            return;
        }

        Student = studentRequest.Value;

        Result<List<BulkEnrolOfferingResponse>> offeringRequest = await _mediator.Send(new GetOfferingsForBulkEnrolQuery(Student.CurrentGrade));

        if (offeringRequest.IsFailure)
        {
            return;
        }

        Offerings = offeringRequest.Value;

        Result<List<StudentEnrolmentResponse>> enrolmentRequest = await _mediator.Send(new GetStudentEnrolmentsWithDetailsQuery(Id));

        if (enrolmentRequest.IsFailure)
        {
            return;
        }

        foreach (StudentEnrolmentResponse enrolment in enrolmentRequest.Value)
        {
            BulkEnrolOfferingResponse offeringEntry = Offerings.FirstOrDefault(entry => entry.OfferingId == enrolment.OfferingId);

            if (offeringEntry is not null)
            {
                ExistingEnrolments.Add(offeringEntry);
            }
        }
    }

    public async Task<IActionResult> OnPost()
    {
        foreach (Guid offeringGuid in SelectedOfferingIds)
        {
            OfferingId offeringId = OfferingId.FromValue(offeringGuid);

            Result response = await _mediator.Send(new EnrolStudentCommand(Id, offeringId));

            if (response.IsFailure)
            {
                Error = new()
                {
                    Error = response.Error,
                    RedirectPath = _linkGenerator.GetPathByPage("/Partner/Students/Details", values: new { area = "Staff", id = Id })
                };

                return Page();
            }
        }

        return RedirectToPage("/Partner/Students/Details", new { area = "Staff", Id = Id });
    }
}