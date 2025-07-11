namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Partner.Students;

using Application.Common.PresentationModels;
using Application.Domains.Schools.Models;
using Application.Domains.Schools.Queries.GetSchoolsForSelectionList;
using Application.Models.Auth;
using Constellation.Application.Domains.Students.Commands.CreateStudent;
using Constellation.Application.Domains.Students.Commands.UpdateStudent;
using Constellation.Application.Domains.Students.Models;
using Constellation.Application.Domains.Students.Queries.GetStudentById;
using Constellation.Presentation.Shared.Helpers.ModelBinders;
using Core.Abstractions.Services;
using Core.Enums;
using Core.Models.Students.Enums;
using Core.Models.Students.Identifiers;
using Core.Models.Students.ValueObjects;
using Core.Shared;
using Core.ValueObjects;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;
using Presentation.Shared.Helpers.Logging;
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
            .ForContext(LogDefaults.Application, LogDefaults.StaffPortal);
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.Partner_Students_Students;
    [ViewData] public string PageTitle { get; set; } = "New Student";

    [BindProperty(SupportsGet = true)]
    public StudentId Id { get; set; } = StudentId.Empty;

    [BindProperty]
    public string? StudentReferenceNumber { get; set; } = string.Empty;

    [BindProperty]
    public string FirstName { get; set; } = string.Empty;

    [BindProperty] 
    public string? PreferredName { get; set; } = string.Empty;

    [BindProperty]
    public string LastName { get; set; } = string.Empty;

    [BindProperty]
    [ModelBinder(typeof(BaseFromValueBinder))]
    public Gender Gender { get; set; }

    [BindProperty]
    public Grade Grade { get; set; }

    [BindProperty]
    public string? EmailAddress { get; set; } = string.Empty;

    [BindProperty]
    public string? SchoolCode { get; set; } = string.Empty;

    public SelectList SchoolList { get; set; }

    public SelectList GenderList { get; set; }

    public async Task OnGet()
    {
        await PreparePage();

        if (Id == StudentId.Empty)
            return;

        GetStudentByIdQuery command = new(Id);

        _logger
            .ForContext(nameof(GetStudentByIdQuery), command, true)
            .Information("Requested to retrieve Student with id {Id} for edit by user {User}", Id, _currentUserService.UserName);

        Result<StudentResponse> student = await _mediator.Send(command);

        if (student.IsFailure)
        {
            ModalContent = ErrorDisplay.Create(
                student.Error,
                _linkGenerator.GetPathByPage("/Partner/Students/Index", values: new { area = "Staff"}));

            _logger
                .ForContext(nameof(Error), student.Error, true)
                .Warning("Failed to retrieve Student with id {Id} for edit by user {User}", Id, _currentUserService.UserName);

            return;
        }

        StudentReferenceNumber = student.Value.StudentReferenceNumber.Number;
        FirstName = student.Value.Name.FirstName;
        PreferredName = student.Value.Name.PreferredName;
        LastName = student.Value.Name.LastName;
        Gender = student.Value.Gender;
        Grade = student.Value.Grade!.Value;
        EmailAddress = student.Value.EmailAddress.Email;
        SchoolCode = student.Value.SchoolCode;

        PageTitle = $"Edit - {student.Value.Name.DisplayName}";
    }

    public async Task<IActionResult> OnPostCreate()
    {
        if (!ModelState.IsValid)
        {
            await PreparePage();

            return Page();
        }

        // Create new student
        CreateStudentCommand createCommand = new(
            StudentReferenceNumber,
            FirstName,
            PreferredName,
            LastName,
            Gender,
            Grade,
            EmailAddress,
            SchoolCode);

        _logger
            .ForContext(nameof(CreateStudentCommand), createCommand, true)
            .Information("Requested to create new Student by user {User}", _currentUserService.UserName);

        Result createResult = await _mediator.Send(createCommand);

        if (createResult.IsFailure)
        {
            ModalContent = ErrorDisplay.Create(createResult.Error);

            _logger
                .ForContext(nameof(Error), createResult.Error, true)
                .Warning("Failed to create new Student by user {User}", _currentUserService.UserName);

            await PreparePage();

            return Page();
        }

        return RedirectToPage("/Partner/Students/Index", new { area = "Staff" });
    }

    public async Task<IActionResult> OnPostUpdate()
    {
        if (!ModelState.IsValid)
        {
            await PreparePage();

            return Page();
        }

        GetStudentByIdQuery command = new(Id);
        
        Result<StudentResponse> student = await _mediator.Send(command);

        if (student.IsFailure)
        {
            ModalContent = ErrorDisplay.Create(
                student.Error,
                _linkGenerator.GetPathByPage("/Partner/Students/Index", values: new { area = "Staff" }));

            _logger
                .ForContext(nameof(Error), student.Error, true)
                .Warning("Failed to retrieve Student with id {Id} for edit by user {User}", Id, _currentUserService.UserName);

            return Page();
        }

        StudentReferenceNumber? srn = null;

        if (student.Value.StudentReferenceNumber != Core.Models.Students.ValueObjects.StudentReferenceNumber.Empty || StudentReferenceNumber is not null)
        {
            Result<StudentReferenceNumber> srnRequest = Core.Models.Students.ValueObjects.StudentReferenceNumber.Create(StudentReferenceNumber);

            if (srnRequest.IsFailure)
            {
                ModalContent = ErrorDisplay.Create(srnRequest.Error);

                _logger
                    .ForContext(nameof(Error), srnRequest.Error, true)
                    .Warning("Failed to create new Student by user {User}", _currentUserService.UserName);

                await PreparePage();
                return Page();
            }

            srn = srnRequest.Value;
        }
        
        Result<Name> name = Name.Create(FirstName, PreferredName, LastName);

        if (name.IsFailure)
        {
            ModalContent = ErrorDisplay.Create(name.Error);

            _logger
                .ForContext(nameof(Error), name.Error, true)
                .Warning("Failed to create new Student by user {User}", _currentUserService.UserName);

            await PreparePage();
            return Page();
        }

        EmailAddress email = Core.ValueObjects.EmailAddress.None;

        if (student.Value.EmailAddress != Core.ValueObjects.EmailAddress.None || EmailAddress is not null)
        {
            Result<EmailAddress> emailRequest = Core.ValueObjects.EmailAddress.Create(EmailAddress);

            if (emailRequest.IsFailure)
            {
                ModalContent = ErrorDisplay.Create(emailRequest.Error);

                _logger
                    .ForContext(nameof(Error), emailRequest.Error, true)
                    .Warning("Failed to create new Student by user {User}", _currentUserService.UserName);

                await PreparePage();
                return Page();
            }

            email = emailRequest.Value;
        }

        // Edit existing student
        UpdateStudentCommand updateCommand = new(
            Id,
            srn,
            name.Value,
            Grade,
            email,
            Gender);

        _logger
            .ForContext(nameof(UpdateStudentCommand), updateCommand, true)
            .Information("Requested to update Student with id {Id} by user {User}", Id, _currentUserService.UserName);

        Result updateResult = await _mediator.Send(updateCommand);

        if (updateResult.IsFailure)
        {
            ModalContent = ErrorDisplay.Create(updateResult.Error);
            
            _logger
                .ForContext(nameof(Error), updateResult.Error, true)
                .Warning("Failed to update Student with id {Id} by user {User}", Id, _currentUserService.UserName);

            await PreparePage();
            return Page();
        }

        return RedirectToPage("/Partner/Students/Index", new { area = "Staff" });
    }

    private async Task PreparePage()
    {
        IEnumerable<Gender> genders = Gender.GetOptions;

        GenderList = new(genders, "Value", "Value");

        Result<List<SchoolSelectionListResponse>> schools = await _mediator.Send(new GetSchoolsForSelectionListQuery());

        if (schools.IsFailure)
        {
            ModalContent = ErrorDisplay.Create(
                schools.Error,
                _linkGenerator.GetPathByPage("/Partner/Students/Index", values: new { area = "Staff" }));

            return;
        }

        SchoolList = new(schools.Value, "Code", "Name");
    }
}