namespace Constellation.Presentation.Staff.Areas.Staff.Pages.StudentAdmin.Consent.Responses;

using Application.Common.PresentationModels;
using Application.Families.GetFamilyContactsForStudent;
using Application.Families.Models;
using Application.Models.Auth;
using Application.Students.GetCurrentStudentsAsDictionary;
using Application.ThirdPartyConsent.CreateTransaction;
using Constellation.Application.ThirdPartyConsent.GetRequiredApplicationsForStudent;
using Core.Abstractions.Services;
using Core.Errors;
using Core.Models.Families.Errors;
using Core.Models.Identifiers;
using Core.Models.Students.Identifiers;
using Core.Models.ThirdPartyConsent.Enums;
using Core.Models.ThirdPartyConsent.Identifiers;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Models;
using Presentation.Shared.Helpers.Logging;
using Serilog;
using System.ComponentModel.DataAnnotations;
using System.Linq;

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

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.StudentAdmin_Consent_Applications;
    [ViewData] public string PageTitle { get; set; } = "New Consent Response";

    [BindProperty]
    [Required(ErrorMessage = "You must select a student")]
    public StudentId StudentId { get; set; } = StudentId.Empty;

    [BindProperty]
    [Required(ErrorMessage = "You must select a parent")]
    public ParentId Submitter { get; set; }

    [BindProperty]
    public string Method { get; set; }

    [BindProperty]
    public string Notes { get; set; }

    [BindProperty(Name = nameof(Consents))]
    [Required(ErrorMessage = "You must select an application")]
    [MinLength(1, ErrorMessage = "You must select an application")]
    public Dictionary<ApplicationId, bool> Consents { get; set; } = new();
    
    public List<FamilyContactResponse> Parents { get; set; } = new();

    public Dictionary<StudentId, string> Students { get; set; }

    public async Task<IActionResult> OnGet() => await PreparePage();

    public async Task<IActionResult> OnPost()
    {
        _logger.Information("Requested to create new Consent Response by user {User}", _currentUserService.UserName);

        if (Consents.Count == 0)
        {
            Error error = new("Consent Required", "No valid consent responses were entered");

            _logger
                .ForContext(nameof(Error), error, true)
                .Warning("Failed to create new Consent Response by user {User}", _currentUserService.UserName);

            ModelState.AddModelError("Responses", "You must include at least one valid application");

            ModalContent = new ErrorDisplay(error);

            return await PreparePage();
        }

        Result<List<FamilyContactResponse>> family = await _mediator.Send(new GetFamilyContactsForStudentQuery(StudentId));

        if (family.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), family.Error, true)
                .Warning("Failed to create new Consent Response by user {User}", _currentUserService.UserName);

            ModalContent = new ErrorDisplay(family.Error);

            return await PreparePage();
        }
        
        FamilyContactResponse? contact = family.Value.FirstOrDefault(entry => entry.ParentId == Submitter);

        if (contact is null)
        {
            _logger
                .ForContext(nameof(Error), ParentErrors.NotFoundInFamily(Submitter, family.Value.First().FamilyId.Value), true)
                .Warning("Failed to create new Consent Response by user {User}", _currentUserService.UserName);

            ModalContent = new ErrorDisplay(ParentErrors.NotFoundInFamily(Submitter, family.Value.First().FamilyId.Value));

            return await PreparePage();
        }

        string contactName = $"{contact.Name} ({contact.EmailAddress})";

        CreateTransactionCommand command = new(
            StudentId,
            contactName,
            contact.EmailAddress?.Email,
            ConsentMethod.FromValue(Method),
            Notes,
            Consents);

        _logger
            .ForContext(nameof(CreateTransactionCommand), command, true)
            .Information("Requested to create new Content Response by user {User}", _currentUserService.UserName);

        Result attempt = await _mediator.Send(command);

        if (attempt.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), attempt.Error, true)
                .Warning("Failed to create new Content Response by user {User}", _currentUserService.UserName);

            ModalContent = new ErrorDisplay(attempt.Error);

            return await PreparePage();
        }

        return RedirectToPage("/StudentAdmin/Consent/Applications/Index", new { area = "Staff" });
    }

    public async Task<IActionResult> OnPostAjaxGetParents(StudentId studentId)
    {
        Result<List<FamilyContactResponse>> family = await _mediator.Send(new GetFamilyContactsForStudentQuery(studentId));

        if (family.IsSuccess)
            return new JsonResult(family.Value);

        return new JsonResult(null);
    }

    public async Task<IActionResult> OnPostAjaxGetRequiredApplications(StudentId studentId)
    {
        Result<List<RequiredApplicationResponse>> applications = await _mediator.Send(new GetRequiredApplicationsForStudentQuery(StudentId));

        if (applications.IsSuccess)
        {
            return Partial("ApplicationConsentCard", applications.Value);
        }

        return Content(string.Empty);
    }

    private async Task<IActionResult> PreparePage()
    {
        _logger.Information("Requested to retrieve options for new Consent Response by user {User}", _currentUserService.UserName);

        Result<Dictionary<StudentId, string>> students = await _mediator.Send(new GetCurrentStudentsAsDictionaryQuery());

        if (students.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), students.Error, true)
                .Warning("Failed to retrieve options for new Consent Response by user {User}", _currentUserService.UserName);

            ModalContent = new ErrorDisplay(
                students.Error,
                _linkGenerator.GetPathByPage("/StudentAdmin/Consent/Applications/Index", values: new { area = "Staff" }));

            return Page();
        }

        Students = students.Value;

        if (StudentId != StudentId.Empty)
        {
            Result<List<FamilyContactResponse>> family = await _mediator.Send(new GetFamilyContactsForStudentQuery(StudentId));

            if (family.IsSuccess)
                Parents = family.Value;
        }

        return Page();
    }
}

