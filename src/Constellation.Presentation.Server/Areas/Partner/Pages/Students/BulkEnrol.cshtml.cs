namespace Constellation.Presentation.Server.Areas.Partner.Pages.Students;

using Application.Enrolments.EnrolStudent;
using Application.Enrolments.GetStudentEnrolmentsWithDetails;
using Application.Models.Auth;
using Application.Offerings.GetOfferingsForBulkEnrol;
using Application.Students.GetStudentById;
using Application.Students.Models;
using BaseModels;
using Core.Models.Offerings.Identifiers;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
                    RedirectPath = _linkGenerator.GetPathByPage("/Students/Details", values: new { area = "Partner", id = Id })
                };

                return Page();
            }
        }

        return RedirectToPage("/Students/Details", new { area = "Partner", Id = Id });
    }
}