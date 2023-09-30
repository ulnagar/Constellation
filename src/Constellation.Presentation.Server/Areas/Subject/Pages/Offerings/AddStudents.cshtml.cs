namespace Constellation.Presentation.Server.Areas.Subject.Pages.Offerings;

using Application.Enrolments.EnrolMultipleStudentsInOffering;
using Application.Enrolments.GetCurrentEnrolmentsForOffering;
using Application.Models.Auth;
using Application.Offerings.GetOfferingSummary;
using Application.Offerings.Models;
using Application.Students.GetStudentsFromOfferingGrade;
using BaseModels;
using Core.Models.Offerings.Identifiers;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize(Policy = AuthPolicies.CanEditSubjects)]
public class AddStudentsModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly LinkGenerator _linkGenerator;

    public AddStudentsModel(
        ISender mediator,
        LinkGenerator linkGenerator)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
    }

    [ViewData]
    public string ActivePage => "Offerings";

    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    public string CourseName { get; set; }
    public string OfferingName { get; set; }

    [BindProperty]
    public List<string> SelectedStudentIds { get; set; } = new();
    public List<StudentFromGradeResponse> Students { get; set; } = new();
    public List<EnrolmentResponse> ExistingEnrolments { get; set; } = new();

    public async Task OnGet()
    {
        await PreparePage();
    }

    public async Task<IActionResult> OnPost()
    {
        OfferingId offeringId = OfferingId.FromValue(Id);

        Result request = await _mediator.Send(new EnrolMultipleStudentsInOfferingCommand(offeringId, SelectedStudentIds));

        if (request.IsFailure)
        {
            Error = new()
            {
                Error = request.Error,
                RedirectPath = _linkGenerator.GetPathByPage("/Offerings/Details", values: new { area = "Subject", Id = Id })
            };

            await PreparePage();

            return Page();
        }

        return RedirectToPage("/Offerings/Details", new { area = "Subject", Id = Id });
    }

    private async Task PreparePage()
    {
        await GetClasses(_mediator);

        OfferingId offeringId = OfferingId.FromValue(Id);

        Result<List<EnrolmentResponse>> enrolmentRequest =
            await _mediator.Send(new GetCurrentEnrolmentsForOfferingQuery(offeringId));

        if (enrolmentRequest.IsFailure)
        {
            Error = new()
            {
                Error = enrolmentRequest.Error,
                RedirectPath = _linkGenerator.GetPathByPage("/Offerings/Details", values: new { area = "Subject", Id = Id })
            };

            return;
        }

        ExistingEnrolments = enrolmentRequest.Value;

        Result<List<StudentFromGradeResponse>> studentsRequest =
            await _mediator.Send(new GetStudentsFromOfferingGradeQuery(offeringId));

        if (studentsRequest.IsFailure)
        {
            Error = new()
            {
                Error = studentsRequest.Error,
                RedirectPath = _linkGenerator.GetPathByPage("/Offerings/Details", values: new { area = "Subject", Id = Id })
            };

            return;
        }

        Students = studentsRequest.Value;

        Result<OfferingSummaryResponse> offeringRequest =
            await _mediator.Send(new GetOfferingSummaryQuery(offeringId));

        if (offeringRequest.IsFailure)
        {
            Error = new()
            {
                Error = offeringRequest.Error,
                RedirectPath = _linkGenerator.GetPathByPage("/Offerings/Details", values: new { area = "Subject", Id = Id })
            };

            return;
        }

        CourseName = offeringRequest.Value.CourseName;
        OfferingName = offeringRequest.Value.Name;
    }

}