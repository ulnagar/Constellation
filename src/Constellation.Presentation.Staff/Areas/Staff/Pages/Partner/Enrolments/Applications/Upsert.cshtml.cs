namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Partner.Enrolments.Applications;

using Application.Common.PresentationModels;
using Application.Domains.EnrolmentContext.Applications.Queries.GetEnrolmentApplicationById;
using Application.Models.Auth;
using Constellation.Application.Domains.Schools.Models;
using Constellation.Application.Domains.Schools.Queries.GetSchoolsForSelectionList;
using Constellation.Core.Abstractions.Services;
using Constellation.Core.Enums;
using Constellation.Core.Models.EnrolmentContext.Offer.Enums;
using Constellation.Core.Models.Identifiers;
using Constellation.Core.Models.Students.Enums;
using Core.Shared;
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
    public string StudentFirstName { get; set; }
    [BindProperty]
    public string? StudentPreferredName { get; set; }
    [BindProperty]
    public string StudentLastName { get; set; }
    [BindProperty]
    public Gender StudentGender { get; set; }
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
    }

    public async Task<IActionResult> OnPostCreate()
    {
        await PreparePage();

        return Page();
    }

    public async Task<IActionResult> OnPostUpdate()
    {
        return Page();
    }
}