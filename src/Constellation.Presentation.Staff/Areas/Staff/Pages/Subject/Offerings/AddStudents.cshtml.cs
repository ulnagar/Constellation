namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Subject.Offerings;

using Constellation.Application.Enrolments.EnrolMultipleStudentsInOffering;
using Constellation.Application.Enrolments.GetCurrentEnrolmentsForOffering;
using Constellation.Application.Models.Auth;
using Constellation.Application.Offerings.GetOfferingSummary;
using Constellation.Application.Offerings.Models;
using Constellation.Application.Students.GetStudentsFromOfferingGrade;
using Constellation.Core.Models.Offerings.Identifiers;
using Constellation.Core.Shared;
using Constellation.Presentation.Staff.Areas;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

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

    [ViewData] public string ActivePage => Presentation.Staff.Pages.Shared.Components.StaffSidebarMenu.ActivePage.Subject_Offerings_Offerings;

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
                RedirectPath = _linkGenerator.GetPathByPage("/Subject/Offerings/Details", values: new { area = "Staff", Id = Id })
            };

            await PreparePage();

            return Page();
        }

        return RedirectToPage("/Subject/Offerings/Details", new { area = "Staff", Id = Id });
    }

    private async Task PreparePage()
    {
        OfferingId offeringId = OfferingId.FromValue(Id);

        Result<List<EnrolmentResponse>> enrolmentRequest =
            await _mediator.Send(new GetCurrentEnrolmentsForOfferingQuery(offeringId));

        if (enrolmentRequest.IsFailure)
        {
            Error = new()
            {
                Error = enrolmentRequest.Error,
                RedirectPath = _linkGenerator.GetPathByPage("/Subject/Offerings/Details", values: new { area = "Staff", Id = Id })
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
                RedirectPath = _linkGenerator.GetPathByPage("/Subject/Offerings/Details", values: new { area = "Staff", Id = Id })
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
                RedirectPath = _linkGenerator.GetPathByPage("/Subject/Offerings/Details", values: new { area = "Staff", Id = Id })
            };

            return;
        }

        CourseName = offeringRequest.Value.CourseName;
        OfferingName = offeringRequest.Value.Name;
    }

}