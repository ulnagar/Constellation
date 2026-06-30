namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Partner.Enrolments.Applications;

using Application.Common.PresentationModels;
using Application.Domains.EnrolmentContext.Applications.Queries.CreateEnrolmentApplication;
using Application.Domains.EnrolmentContext.Applications.Queries.GetEnrolmentApplicationById;
using Application.Models.Auth;
using Constellation.Application.Domains.Schools.Models;
using Constellation.Application.Domains.Schools.Queries.GetSchoolsForSelectionList;
using Constellation.Core.Abstractions.Services;
using Constellation.Core.Enums;
using Constellation.Core.Models.EnrolmentContext.Offer.Enums;
using Constellation.Core.Models.Identifiers;
using Constellation.Core.Models.Students.Enums;
using Core.Models.EnrolmentContext.EnrolmentPeriod.Identifiers;
using Core.Models.Students.ValueObjects;
using Core.Shared;
using Core.ValueObjects;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;
using Presentation.Shared.Helpers.Attributes;
using Presentation.Shared.Helpers.Logging;
using Presentation.Shared.Helpers.ModelBinders;
using Serilog;
using System.ComponentModel.DataAnnotations;
using ApplicationId = Core.Models.EnrolmentContext.Application.Identifiers.ApplicationId;

[HasPermission(AuthPermission.Partners_Enrolments_Applications_Edit_Value)]
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

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.Partner_Enrolments_Applications;
    [ViewData] public string PageTitle => "Enrolment Applications";

    [BindProperty(SupportsGet = true)]
    public ApplicationId Id { get; set; } = ApplicationId.Empty;

    [BindProperty]
    public string? StudentReferenceNumber { get; set; }
    [BindProperty]
    [Required(ErrorMessage = "First Name is required")]
    public string StudentFirstName { get; set; }
    [BindProperty]
    public string? StudentPreferredName { get; set; }
    [BindProperty]
    [Required(ErrorMessage = "Last Name is required")]
    public string StudentLastName { get; set; }
    [BindProperty]
    public Gender StudentGender { get; set; } = Gender.Empty;
    [BindProperty]
    [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd}")]
    public DateOnly? DateOfBirth { get; set; }
    [BindProperty]
    public string? StudentEmailAddress { get; set; }
    [BindProperty]
    public string? ParentFirstName { get; set; }
    [BindProperty]
    public string? ParentLastName { get; set; }
    [BindProperty]
    public string? ParentEmailAddress { get; set; }
    [BindProperty]
    public string? ParentPhoneNumber { get; set; }
    [BindProperty]
    public string? MailingAddressStreet { get; set; }
    [BindProperty]
    public string? MailingAddressTown { get; set; }
    [BindProperty]
    public string? MailingAddressState { get; set; }
    [BindProperty]
    public string? MailingAddressPostCode { get; set; }
    [BindProperty]
    public string? ApplicationReference { get; set; }
    [BindProperty]
    public SchoolCode CurrentSchoolCode { get; set; } = SchoolCode.Empty;
    [BindProperty]
    public string? CurrentSchool { get; set; }
    [BindProperty]
    public SchoolCode DestinationSchoolCode { get; set; } = SchoolCode.Empty;
    [BindProperty]
    public string? DestinationSchool { get; set; }

    [BindProperty]
    [ModelBinder(typeof(BaseFromValueBinder))]
    public Program Program { get; set; } = Program.Empty;
    [BindProperty]
    public Grade Grade { get; set; }

    public IEnumerable<SelectListItem> SchoolList { get; set; }
    public SelectList ProgramList { get; set; }
    public SelectList GenderList { get; set; }

    public async Task OnGet()
    {
        if (Id == ApplicationId.Empty)
        {
            await PreparePage();
            return;
        }

        Result<EnrolmentApplicationResponse> application = await _mediator.Send(new GetEnrolmentApplicationByIdQuery(Id));

        if (application.IsFailure)
        {
            ModalContent = ErrorDisplay.Create(
                application.Error,
                _linkGenerator.GetPathByPage("/Partner/Enrolments/Applications/Index", values: new { area = "Staff" }));

            return;
        }

        StudentReferenceNumber = application.Value.StudentReferenceNumber?.Value;
        StudentFirstName = application.Value.StudentName.FirstName;
        StudentPreferredName = application.Value.StudentName.PreferredName;
        StudentLastName = application.Value.StudentName.LastName;
        StudentGender = application.Value.StudentGender;
        DateOfBirth = application.Value.DateOfBirth;
        StudentEmailAddress = application.Value.StudentEmailAddress?.Email;
        ParentFirstName = application.Value.ParentName?.FirstName;
        ParentLastName = application.Value.ParentName?.LastName;
        ParentEmailAddress = application.Value.ParentEmailAddress?.Email;
        ParentPhoneNumber = application.Value.ParentPhoneNumber?.Value;
        MailingAddressStreet = application.Value.MailingAddress?.Street;
        MailingAddressTown = application.Value.MailingAddress?.Town;
        MailingAddressState = application.Value.MailingAddress?.State;
        MailingAddressPostCode = application.Value.MailingAddress?.Postcode;
        ApplicationReference = application.Value.ApplicationReference;
        CurrentSchoolCode = application.Value.CurrentSchoolCode ?? SchoolCode.Empty;
        CurrentSchool = application.Value.CurrentSchool;
        DestinationSchoolCode = application.Value.DestinationSchoolCode ?? SchoolCode.Empty;
        DestinationSchool = application.Value.DestinationSchool;
        Program = application.Value.Program;
        Grade = application.Value.Grade;

        await PreparePage();
    }

    private async Task PreparePage()
    {
        Result<List<SchoolSelectionListResponse>> schools = await _mediator.Send(new GetSchoolsForSelectionListQuery());

        if (schools.IsFailure)
        {
            ModalContent = ErrorDisplay.Create(
                schools.Error,
                _linkGenerator.GetPathByPage("/Partner/Enrolments/Applications/Index", values: new { area = "Staff" }));

            return;
        }

        SchoolList = schools.Value
            .OrderBy(s => s.Name)
            .Select(s => new SelectListItem { Value = s.Code, Text = s.Name })
            .ToList();

        ProgramList = new SelectList(
            Program.GetOptions,
            nameof(Program.Value),
            nameof(Program.Name),
            Program?.Value);

        GenderList = new SelectList(
            Gender.GetOptions,
            nameof(Gender.Value),
            nameof(Gender.Name),
            StudentGender.Value);
    }

    public async Task<IActionResult> OnPostCreate()
    {
        await ValidateForm();

        if (!ModelState.IsValid)
        {
            await PreparePage();
            return Page();
        }

        Result<StudentReferenceNumber> srnResult = Core.Models.Students.ValueObjects.StudentReferenceNumber.Create(StudentReferenceNumber);
        StudentReferenceNumber? srn = srnResult.IsFailure ? null : srnResult.Value;

        Result<Name> studentNameResult = Name.Create(StudentFirstName, StudentPreferredName, StudentLastName);
        if (studentNameResult.IsFailure)
        {
            ModalContent = ErrorDisplay.Create(studentNameResult.Error);

            await PreparePage();
            return Page();
        }

        Result<EmailAddress> studentEmailResult = EmailAddress.Create(StudentEmailAddress);
        EmailAddress? studentEmail = studentEmailResult.IsFailure ? null : studentEmailResult.Value;

        Result<Name> parentNameResult = Name.Create(ParentFirstName, ParentFirstName, ParentLastName);
        Name? parentName = parentNameResult.IsFailure ? null : parentNameResult.Value;

        Result<EmailAddress> parentEmailResult = EmailAddress.Create(ParentEmailAddress);
        EmailAddress? parentEmail = parentEmailResult.IsFailure ? null : parentEmailResult.Value;

        Result<PhoneNumber> parentPhoneResult = PhoneNumber.Create(ParentPhoneNumber);
        PhoneNumber? parentPhone = parentPhoneResult.IsFailure ? null : parentPhoneResult.Value;

        Result<MailingAddress> mailingAddressResult = MailingAddress.Create(MailingAddressStreet, MailingAddressTown, MailingAddressState, MailingAddressPostCode);
        MailingAddress? mailingAddress = mailingAddressResult.IsFailure ? null : mailingAddressResult.Value;

        CreateEnrolmentApplicationCommand command = new(
            EnrolmentPeriodId.Empty,
            srn,
            studentNameResult.Value,
            StudentGender,
            DateOfBirth,
            studentEmail,
            parentName,
            parentEmail,
            parentPhone,
            mailingAddress,
            ApplicationReference ?? string.Empty,
            CurrentSchoolCode,
            CurrentSchool ?? string.Empty,
            DestinationSchoolCode,
            DestinationSchool ?? string.Empty,
            Program,
            Grade);

        Result result = await _mediator.Send(command);

        if (result.IsFailure)
        {
            ModalContent = ErrorDisplay.Create(result.Error);

            await PreparePage();
            return Page();
        }

        return RedirectToPage("/Partner/Enrolments/Applications/Index", new { area = "Staff" });
    }

    public async Task<IActionResult> OnPostUpdate()
    {
        return Page();
    }

    private async Task ValidateForm()
    {
        if (StudentGender == Gender.Empty)
            ModelState.AddModelError(nameof(StudentGender), "Gender is required");

        if (Program == Program.Empty)
            ModelState.AddModelError(nameof(Program), "Program is required");

        if (Grade == 0)
        {
            ModelState.Remove(nameof(Grade));
            ModelState.AddModelError(nameof(Grade), "Grade is required");
        }

        bool isValidProgramGradeCombination = (Program, Grade) switch
        {
            ({ Value: "OC" }, Grade.Y05) => true,
            ({ Value: "SHS" }, Grade.Y07 or Grade.Y08 or Grade.Y09 or Grade.Y10) => true,
            ({ Value: "YDM" }, Grade.Y05 or Grade.Y06 or Grade.Y07 or Grade.Y08 or Grade.Y09 or Grade.Y10) => true,
            ({ Value: "S6R" }, Grade.Y11 or Grade.Y12) => true,
            ({ Value: "S6M" }, Grade.Y11 or Grade.Y12) => true,
            _ => false
        };

        if (!isValidProgramGradeCombination)
            ModelState.AddModelError(nameof(Grade), "Grade is not valid for Program");
    }
}