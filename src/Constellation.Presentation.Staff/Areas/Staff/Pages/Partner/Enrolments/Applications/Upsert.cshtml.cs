namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Partner.Enrolments.Applications;

using Application.Common.PresentationModels;
using Application.Domains.EnrolmentContext.Applications.Queries.GetEnrolmentApplicationById;
using Application.Models.Auth;
using Constellation.Core.Abstractions.Services;
using Constellation.Core.Enums;
using Constellation.Core.Models.EnrolmentContext.Offer.Enums;
using Constellation.Core.Models.Identifiers;
using Constellation.Core.Models.Students.Enums;
using Constellation.Core.Models.Students.ValueObjects;
using Constellation.Core.ValueObjects;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Presentation.Shared.Helpers.Attributes;
using Presentation.Shared.Helpers.Logging;
using Presentation.Staff.Pages.Partner.Enrolments.Applications;
using Serilog;
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

    public string? StudentReferenceNumber { get; private set; }
    public string StudentFirstName { get; private set; }
    public string? StudentPreferredName { get; private set; }
    public string StudentLastName { get; private set; }
    public Gender StudentGender { get; private set; }
    public DateOnly? DateOfBirth { get; private set; }
    public string? StudentEmailAddress { get; private set; }
    public string? ParentFirstName { get; private set; }
    public string? ParentLastName { get; private set; }
    public string? ParentEmailAddress { get; private set; }
    public string? ParentPhoneNumber { get; private set; }
    public string? MailingAddressStreet { get; private set; }
    public string? MailingAddressTown { get; private set; }
    public string? MailingAddressState { get; private set; }
    public string? MailingAddressPostCode { get; private set; }
    public string? ApplicationReference { get; private set; }
    public SchoolCode? CurrentSchoolCode { get; private set; }
    public string? CurrentSchool { get; private set; }
    public SchoolCode? DestinationSchoolCode { get; private set; }
    public string? DestinationSchool { get; private set; }
    public Program Program { get; private set; }
    public Grade Grade { get; private set; }
    
    public async Task OnGet()
    {
        if (Id == ApplicationId.Empty)
            return;

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
        CurrentSchoolCode = application.Value.CurrentSchoolCode;
        CurrentSchool = application.Value.CurrentSchool;
        DestinationSchoolCode = application.Value.DestinationSchoolCode;
        DestinationSchool = application.Value.DestinationSchool;
        Program = application.Value.Program;
        Grade = application.Value.Grade;
    }
}