namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Partner.Enrolments.Applications;

using Application.Common.PresentationModels;
using Application.Domains.EnrolmentContext.Applications.Models;
using Application.Domains.EnrolmentContext.Applications.Queries.GetEnrolmentApplicationById;
using Application.Domains.EnrolmentContext.EnrolmentPeriods.Models;
using Application.Domains.EnrolmentContext.EnrolmentPeriods.Queries.GetCurrentEnrolmentPeriods;
using Application.Models.Auth;
using Constellation.Application.Domains.EnrolmentContext.Applications.Commands.CreateEnrolmentApplication;
using Constellation.Application.Domains.EnrolmentContext.Applications.Commands.UpdateEnrolmentApplication;
using Constellation.Application.Domains.Schools.Models;
using Constellation.Application.Domains.Schools.Queries.GetSchoolsForSelectionList;
using Constellation.Core.Abstractions.Services;
using Constellation.Core.Enums;
using Constellation.Core.Models.EnrolmentContext.Offer.Enums;
using Constellation.Core.Models.Identifiers;
using Constellation.Core.Models.Students.Enums;
using Core.Models.EnrolmentContext.Application;
using Core.Models.EnrolmentContext.EnrolmentPeriod.Identifiers;
using Core.Models.Students.ValueObjects;
using Core.Shared;
using Core.ValueObjects;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;
using Presentation.Shared.Extensions;
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
            .ForStaffPortal();
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.Partner_Enrolments_Applications;
    [ViewData] public string PageTitle => "Enrolment Applications";

    [BindProperty(SupportsGet = true)]
    public ApplicationId Id { get; set; } = ApplicationId.Empty;

    [BindProperty]
    public EnrolmentPeriodId PeriodId { get; set; } = EnrolmentPeriodId.Empty;
    public string PeriodName { get; set; }

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

    public SelectList PeriodList { get; set; }
    public IEnumerable<SelectListItem> SchoolList { get; set; }
    public SelectList ProgramList { get; set; }
    public SelectList GenderList { get; set; }

    public async Task OnGet()
    {
        if (Id == ApplicationId.Empty)
        {
            _logger
                .Information("Requested to load defaults for creation of new Enrolment Application by user {User}", _currentUserService.UserName);

            await PreparePage();
            return;
        }

        _logger
            .Information("Requested to load Enrolment Application for update by user {User}", _currentUserService.UserName);

        Result<EnrolmentApplicationResponse> application = await _mediator.Send(new GetEnrolmentApplicationByIdQuery(Id));

        if (application.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), application.Error, true)
                .Warning("Failed to load Enrolment Application for update by user {User}", _currentUserService.UserName);

            ModalContent = ErrorDisplay.Create(
                application.Error,
                _linkGenerator.GetPathByPage("/Partner/Enrolments/Applications/Index", values: new { area = "Staff" }));

            return;
        }

        PeriodId = application.Value.PeriodId;
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
            Program.Value);

        GenderList = new SelectList(
            Gender.GetOptions,
            nameof(Gender.Value),
            nameof(Gender.Name),
            StudentGender.Value);

        Result<List<EnrolmentPeriodResponse>> periods = await _mediator.Send(new GetCurrentEnrolmentPeriodsQuery());

        if (periods.IsFailure)
        {
            ModalContent = ErrorDisplay.Create(
                periods.Error,
                _linkGenerator.GetPathByPage("/Partner/Enrolments/Applications/Index", values: new { area = "Staff" }));

            return;
        }

        if (PeriodId != EnrolmentPeriodId.Empty)
        {
            PeriodName = periods.Value
                .FirstOrDefault(entry => entry.Id == PeriodId)?.Label
                 ?? string.Empty;
        }

        PeriodList = new SelectList(
            periods.Value,
            nameof(EnrolmentPeriodResponse.Id),
            nameof(EnrolmentPeriodResponse.Label),
            PeriodId);
    }

    public async Task<IActionResult> OnPostCreate()
    {
        ValidateForm();
        TryBuildFormValues(out ApplicationFormValues values);
        
        if (!ModelState.IsValid)
        {
            _logger
                .Warning("Failed to validate new Enrolment Application form by user {User}", _currentUserService.UserName);

            await PreparePage();
            return Page();
        }

        CreateEnrolmentApplicationCommand command = new(
            PeriodId,
            values.StudentReferenceNumber,
            values.StudentName,
            StudentGender,
            DateOfBirth,
            values.StudentEmailAddress,
            values.ParentName,
            values.ParentEmailAddress,
            values.ParentPhoneNumber,
            values.MailingAddress,
            ApplicationReference ?? string.Empty,
            CurrentSchoolCode,
            CurrentSchool ?? string.Empty,
            DestinationSchoolCode,
            DestinationSchool ?? string.Empty,
            Program,
            Grade);

        _logger
            .ForContext(nameof(CreateEnrolmentApplicationCommand), command, true)
            .Information("Requested to create new Enrolment Application by user {User}", _currentUserService.UserName);

        Result result = await _mediator.Send(command);

        if (result.IsFailure)
        {
            _logger
                .ForContext(nameof(CreateEnrolmentApplicationCommand), command, true)
                .ForContext(nameof(Error), result.Error, true)
                .Warning("Failed to create new Enrolment Application by user {User}", _currentUserService.UserName);

            ModalContent = ErrorDisplay.Create(result.Error);

            await PreparePage();
            return Page();
        }

        return RedirectToPage("/Partner/Enrolments/Applications/Index", new { area = "Staff" });
    }

    public async Task<IActionResult> OnPostUpdate()
    {
        ValidateForm();
        TryBuildFormValues(out ApplicationFormValues values);

        if (!ModelState.IsValid)
        {
            _logger
                .Warning("Failed to validate Enrolment Application update form by user {User}", _currentUserService.UserName);

            await PreparePage();
            return Page();
        }

        UpdateEnrolmentApplicationCommand command = new(
            Id,
            values.StudentReferenceNumber,
            values.StudentName,
            StudentGender,
            DateOfBirth,
            values.StudentEmailAddress,
            values.ParentName,
            values.ParentEmailAddress,
            values.ParentPhoneNumber,
            values.MailingAddress,
            ApplicationReference ?? string.Empty,
            CurrentSchoolCode,
            CurrentSchool ?? string.Empty,
            DestinationSchoolCode,
            DestinationSchool ?? string.Empty,
            Program,
            Grade);

        _logger
            .ForContext(nameof(UpdateEnrolmentApplicationCommand), command, true)
            .Information("Requested to update Enrolment Application by user {User}", _currentUserService.UserName);

        Result result = await _mediator.Send(command);

        if (result.IsFailure)
        {
            _logger
                .ForContext(nameof(UpdateEnrolmentApplicationCommand), command, true)
                .ForContext(nameof(Error), result.Error, true)
                .Warning("Failed to update Enrolment Application by user {User}", _currentUserService.UserName);

            ModalContent = ErrorDisplay.Create(result.Error);

            await PreparePage();
            return Page();
        }

        return RedirectToPage("/Partner/Enrolments/Applications/Index", new { area = "Staff" });
    }

    private void ValidateForm()
    {
        if (PeriodId == EnrolmentPeriodId.Empty)
        {
            ModelState.Remove(nameof(PeriodId));
            ModelState.AddModelError(nameof(PeriodId), "You must select an Enrolment Period");
        }

        if (StudentGender == Gender.Empty)
            ModelState.AddModelError(nameof(StudentGender), "Gender is required");

        if (Program == Program.Empty)
            ModelState.AddModelError(nameof(Program), "Program is required");

        if (Grade == 0)
        {
            ModelState.Remove(nameof(Grade));
            ModelState.AddModelError(nameof(Grade), "Grade is required");
        }

        if (!Application.IsValidProgramGradeCombination(Program, Grade))
            ModelState.AddModelError(nameof(Grade), "Grade is not valid for Program");
    }

    private bool TryBuildFormValues(out ApplicationFormValues values)
    {
        bool isValid = true;

        StudentReferenceNumber? srn = null;
        if (!string.IsNullOrWhiteSpace(StudentReferenceNumber))
        {
            Result<StudentReferenceNumber> srnResult = Core.Models.Students.ValueObjects.StudentReferenceNumber.Create(StudentReferenceNumber);

            if (srnResult.IsFailure)
            {
                ModelState.AddModelError(nameof(StudentReferenceNumber), srnResult.Error.Message);
                isValid = false;
            }
            else
            {
                srn = srnResult.Value;
            }
        }

        // Required — always validated regardless of blank/non-blank, since the form already
        // enforces StudentFirstName/StudentLastName via [Required].
        Result<Name> studentNameResult = Name.Create(StudentFirstName, StudentPreferredName, StudentLastName);
        if (studentNameResult.IsFailure)
        {
            ModelState.AddModelError(nameof(StudentFirstName), studentNameResult.Error.Message);
            isValid = false;
        }

        EmailAddress? studentEmail = null;
        if (!string.IsNullOrWhiteSpace(StudentEmailAddress))
        {
            Result<EmailAddress> studentEmailResult = EmailAddress.Create(StudentEmailAddress);

            if (studentEmailResult.IsFailure)
            {
                ModelState.AddModelError(nameof(StudentEmailAddress), studentEmailResult.Error.Message);
                isValid = false;
            }
            else
            {
                studentEmail = studentEmailResult.Value;
            }
        }

        Name? parentName = null;
        if (!string.IsNullOrWhiteSpace(ParentFirstName) || !string.IsNullOrWhiteSpace(ParentLastName))
        {
            Result<Name> parentNameResult = Name.Create(ParentFirstName, ParentFirstName, ParentLastName);

            if (parentNameResult.IsFailure)
            {
                ModelState.AddModelError(nameof(ParentFirstName), parentNameResult.Error.Message);
                isValid = false;
            }
            else
            {
                parentName = parentNameResult.Value;
            }
        }

        EmailAddress? parentEmail = null;
        if (!string.IsNullOrWhiteSpace(ParentEmailAddress))
        {
            Result<EmailAddress> parentEmailResult = EmailAddress.Create(ParentEmailAddress);

            if (parentEmailResult.IsFailure)
            {
                ModelState.AddModelError(nameof(ParentEmailAddress), parentEmailResult.Error.Message);
                isValid = false;
            }
            else
            {
                parentEmail = parentEmailResult.Value;
            }
        }

        PhoneNumber? parentPhone = null;
        if (!string.IsNullOrWhiteSpace(ParentPhoneNumber))
        {
            Result<PhoneNumber> parentPhoneResult = PhoneNumber.Create(ParentPhoneNumber);

            if (parentPhoneResult.IsFailure)
            {
                ModelState.AddModelError(nameof(ParentPhoneNumber), parentPhoneResult.Error.Message);
                isValid = false;
            }
            else
            {
                parentPhone = parentPhoneResult.Value;
            }
        }

        MailingAddress? mailingAddress = null;
        bool mailingAddressProvided =
            !string.IsNullOrWhiteSpace(MailingAddressStreet) ||
            !string.IsNullOrWhiteSpace(MailingAddressTown) ||
            !string.IsNullOrWhiteSpace(MailingAddressState) ||
            !string.IsNullOrWhiteSpace(MailingAddressPostCode);

        if (mailingAddressProvided)
        {
            Result<MailingAddress> mailingAddressResult = MailingAddress.Create(MailingAddressStreet, MailingAddressTown, MailingAddressState, MailingAddressPostCode);

            if (mailingAddressResult.IsFailure)
            {
                ModelState.AddModelError(nameof(MailingAddressStreet), mailingAddressResult.Error.Message);
                isValid = false;
            }
            else
            {
                mailingAddress = mailingAddressResult.Value;
            }
        }

        values = isValid
            ? new ApplicationFormValues(srn, studentNameResult.Value, studentEmail, parentName, parentEmail, parentPhone, mailingAddress)
            : null!;

        return isValid;
    }

    private sealed record ApplicationFormValues(
        StudentReferenceNumber? StudentReferenceNumber,
        Name StudentName,
        EmailAddress? StudentEmailAddress,
        Name? ParentName,
        EmailAddress? ParentEmailAddress,
        PhoneNumber? ParentPhoneNumber,
        MailingAddress? MailingAddress);
}