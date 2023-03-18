namespace Constellation.Presentation.Server.Areas.Partner.Pages.Students;

using Constellation.Application.Enrolments.GetStudentEnrolmentsWithDetails;
using Constellation.Application.Families.GetFamilyContactsForStudent;
using Constellation.Application.Models.Auth;
using Constellation.Application.Offerings.GetSessionDetailsForStudent;
using Constellation.Application.Students.GetStudentById;
using Constellation.Core.Models.Families;
using Constellation.Presentation.Server.BaseModels;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize(Policy = AuthPolicies.IsStaffMember)]
public class DetailsModel : BasePageModel
{
    private readonly IMediator _mediator;
    private readonly LinkGenerator _linkGenerator;

    public DetailsModel(
        IMediator mediator,
        LinkGenerator linkGenerator)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
    }

    [BindProperty(SupportsGet = true)]
    public string Id { get; set; }

    public StudentResponse Student { get; set; }

    public List<FamilyContactResponse> FamilyContacts { get; set; } = new();

    public List<StudentEnrolmentResponse> Enrolments { get; set; } = new();

    public List<StudentSessionDetailsResponse> Sessions { get; set; } = new();
    public int MinPerFn => CalculateTotalSessionDuration();
    // Sessions
    // Equipment
    // Absences

    public async Task OnGet()
    {
        await GetClasses(_mediator);

        if (string.IsNullOrWhiteSpace(Id))
        {
            Error = new ErrorDisplay
            {
                Error = new("Page.Parameter.NotFound", "You must specify a value for the Student Id parameter"),
                RedirectPath = _linkGenerator.GetPathByAction("Index", "Students", new { area = "Partner" })
            };

            Student = new("", "", Core.Enums.Grade.SpecialProgram, "", "", "", false);

            return;
        }
    }

    private int CalculateTotalSessionDuration()
    {
        if (Sessions is null || !Sessions.Any())
            return 0;

        return Sessions.Sum(session => session.Duration);
    }
}
