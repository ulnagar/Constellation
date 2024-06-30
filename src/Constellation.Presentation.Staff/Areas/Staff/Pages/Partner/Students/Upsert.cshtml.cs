namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Partner.Students;

using Application.Models.Auth;
using Application.Schools.GetSchoolsForSelectionList;
using Application.Schools.Models;
using Application.Students.CreateStudent;
using Application.Students.GetStudentById;
using Application.Students.Models;
using Application.Students.UpdateStudent;
using Core.Enums;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;

[Authorize(Policy = AuthPolicies.CanEditStudents)]
public class UpsertModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly LinkGenerator _linkGenerator;

    public UpsertModel(
        ISender mediator,
        LinkGenerator linkGenerator)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.Partner_Students_Students;

    [BindProperty(SupportsGet = true)]
    public string? Id { get; set; } = string.Empty;


    [BindProperty]
    public string StudentId { get; set; } = string.Empty;

    [BindProperty]
    public string FirstName { get; set; } = string.Empty;

    [BindProperty]
    public string LastName { get; set; } = string.Empty;

    [BindProperty]
    public string Gender { get; set; } = string.Empty;

    [BindProperty]
    public Grade Grade { get; set; }

    [BindProperty]
    public string PortalUsername { get; set; } = string.Empty;

    [BindProperty]
    public string SchoolCode { get; set; } = string.Empty;

    public SelectList SchoolList { get; set; }

    public SelectList GenderList { get; set; }

    public async Task OnGet()
    {
        List<SelectListItem> genders = new()
        {
            new() { Text ="Male", Value = "M" },
            new() { Text = "Female", Value = "F" }
        };

        Result<List<SchoolSelectionListResponse>> schools = await _mediator.Send(new GetSchoolsForSelectionListQuery());

        if (schools.IsFailure)
        {
            Error = new()
            {
                Error = schools.Error,
                RedirectPath = _linkGenerator.GetPathByPage("/Partner/Students/Index", values: new { area = "Staff" })
            };

            return;
        }
        
        if (string.IsNullOrWhiteSpace(Id))
        {
            GenderList = new(genders, "Value", "Text");
            SchoolList = new(schools.Value, "Code", "Name");

            return;
        }

        Result<StudentResponse>? student = await _mediator.Send(new GetStudentByIdQuery(Id));

        if (student.IsFailure)
        {
            Error = new()
            {
                Error = student.Error,
                RedirectPath = _linkGenerator.GetPathByPage("/Partner/Students/Index", values: new { area = "Staff"})
            };

            return;
        }

        StudentId = student.Value.StudentId;
        FirstName = student.Value.Name.FirstName;
        LastName = student.Value.Name.LastName;
        Gender = student.Value.Gender;
        Grade = student.Value.CurrentGrade;
        PortalUsername = student.Value.PortalUsername;
        SchoolCode = student.Value.SchoolCode;

        GenderList = new(genders, "Value", "Text", student.Value.Gender);
        SchoolList = new(schools.Value, "Code", "Name", student.Value.SchoolCode);
    }

    public async Task<IActionResult> OnPost()
    {
        if (!ModelState.IsValid)
        {
            List<SelectListItem> genders = new()
            {
                new() { Text ="Male", Value = "M" },
                new() { Text = "Female", Value = "F" }
            };

            Result<List<SchoolSelectionListResponse>> schools = await _mediator.Send(new GetSchoolsForSelectionListQuery());

            GenderList = new(genders, "Value", "Text", Gender);
            SchoolList = new(schools.Value, "Code", "Name", SchoolCode);

            return Page();
        }

        if (string.IsNullOrWhiteSpace(Id))
        {
            // Create new student
            CreateStudentCommand createCommand = new(
                StudentId,
                FirstName,
                LastName,
                Gender,
                Grade,
                PortalUsername,
                SchoolCode);

            Result createResult = await _mediator.Send(createCommand);

            if (createResult.IsFailure)
            {
                Error = new()
                {
                    Error = createResult.Error,
                    RedirectPath = null
                };

                List<SelectListItem> genders = new()
                {
                    new() { Text ="Male", Value = "M" },
                    new() { Text = "Female", Value = "F" }
                };

                Result<List<SchoolSelectionListResponse>> schools = await _mediator.Send(new GetSchoolsForSelectionListQuery());

                GenderList = new(genders, "Value", "Text", Gender);
                SchoolList = new(schools.Value, "Code", "Name", SchoolCode);

                return Page();
            }

            return RedirectToPage("/Partner/Students/Index", new { area = "Staff" });
        }

        // Edit existing student
        UpdateStudentCommand updateCommand = new(
            StudentId,
            FirstName,
            LastName,
            PortalUsername,
            Grade,
            Gender,
            SchoolCode);

        Result updateResult = await _mediator.Send(updateCommand);

        if (updateResult.IsFailure)
        {
            Error = new()
            {
                Error = updateResult.Error,
                RedirectPath = null
            };

            List<SelectListItem> genders = new()
            {
                new() { Text ="Male", Value = "M" },
                new() { Text = "Female", Value = "F" }
            };

            Result<List<SchoolSelectionListResponse>> schools = await _mediator.Send(new GetSchoolsForSelectionListQuery());

            GenderList = new(genders, "Value", "Text", Gender);
            SchoolList = new(schools.Value, "Code", "Name", SchoolCode);

            return Page();
        }

        return RedirectToPage("/Partner/Students/Index", new { area = "Staff" });
    }
}