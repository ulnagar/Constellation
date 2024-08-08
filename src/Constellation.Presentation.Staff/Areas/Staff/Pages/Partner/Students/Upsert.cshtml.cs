namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Partner.Students;

using Application.Common.PresentationModels;
using Application.Models.Auth;
using Application.Schools.GetSchoolsForSelectionList;
using Application.Schools.Models;
using Application.Students.CreateStudent;
using Application.Students.GetStudentById;
using Application.Students.Models;
using Application.Students.UpdateStudent;
using Core.Abstractions.Services;
using Core.Enums;
using Core.Models.Students;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;
using Models;
using Serilog;

[Authorize(Policy = AuthPolicies.CanEditStudents)]
public class UpsertModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly LinkGenerator _linkGenerator;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public UpsertModel(
        ISender mediator,
        LinkGenerator linkGenerator,
        ICurrentUserService currentUserService,
        ILogger logger)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
        _currentUserService = currentUserService;
        _logger = logger
            .ForContext<UpsertModel>()
            .ForContext(StaffLogDefaults.Application, StaffLogDefaults.StaffPortal);
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.Partner_Students_Students;
    [ViewData] public string PageTitle { get; set; } = "New Student";

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
            ModalContent = new ErrorDisplay(
                schools.Error,
                _linkGenerator.GetPathByPage("/Partner/Students/Index", values: new { area = "Staff" }));

            return;
        }
        
        if (string.IsNullOrWhiteSpace(Id))
        {
            GenderList = new(genders, "Value", "Text");
            SchoolList = new(schools.Value, "Code", "Name");

            return;
        }

        GetStudentByIdQuery command = new(Id);

        _logger
            .ForContext(nameof(GetStudentByIdQuery), command, true)
            .Information("Requested to retrieve Student with id {Id} for edit by user {User}", Id, _currentUserService.UserName);

        Result<StudentResponse> student = await _mediator.Send(command);

        if (student.IsFailure)
        {
            ModalContent = new ErrorDisplay(
                student.Error,
                _linkGenerator.GetPathByPage("/Partner/Students/Index", values: new { area = "Staff"}));

            _logger
                .ForContext(nameof(Error), student.Error, true)
                .Warning("Failed to retrieve Student with id {Id} for edit by user {User}", Id, _currentUserService.UserName);

            return;
        }

        StudentId = student.Value.StudentId;
        FirstName = student.Value.Name.FirstName;
        LastName = student.Value.Name.LastName;
        Gender = student.Value.Gender;
        Grade = student.Value.CurrentGrade;
        PortalUsername = student.Value.PortalUsername;
        SchoolCode = student.Value.SchoolCode;

        PageTitle = $"Edit - {student.Value.Name.DisplayName}";

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

            _logger
                .ForContext(nameof(CreateStudentCommand), createCommand, true)
                .Information("Requested to create new Student by user {User}", _currentUserService.UserName);

            Result createResult = await _mediator.Send(createCommand);

            if (createResult.IsFailure)
            {
                ModalContent = new ErrorDisplay(createResult.Error);
                
                _logger
                    .ForContext(nameof(Error), createResult.Error, true)
                    .Warning("Failed to create new Student by user {User}", _currentUserService.UserName);

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

        _logger
            .ForContext(nameof(UpdateStudentCommand), updateCommand, true)
            .Information("Requested to update Student with id {Id} by user {User}", Id, _currentUserService.UserName);

        Result updateResult = await _mediator.Send(updateCommand);

        if (updateResult.IsFailure)
        {
            ModalContent = new ErrorDisplay(updateResult.Error);
            
            _logger
                .ForContext(nameof(Error), updateResult.Error, true)
                .Warning("Failed to update Student with id {Id} by user {User}", Id, _currentUserService.UserName);

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