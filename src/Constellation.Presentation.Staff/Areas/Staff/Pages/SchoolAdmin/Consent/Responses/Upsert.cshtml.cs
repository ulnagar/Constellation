namespace Constellation.Presentation.Staff.Areas.Staff.Pages.SchoolAdmin.Consent.Responses;

using Application.Common.PresentationModels;
using Application.Families.GetFamilyContactsForStudent;
using Application.Families.Models;
using Application.Models.Auth;
using Application.Students.GetCurrentStudentsAsDictionary;
using Application.ThirdPartyConsent.CreateTransaction;
using Application.ThirdPartyConsent.GetApplications;
using Core.Errors;
using Core.Models.Identifiers;
using Core.Models.ThirdPartyConsent.Enums;
using Core.Models.ThirdPartyConsent.Identifiers;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System.ComponentModel.DataAnnotations;
using System.Linq;

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

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.SchoolAdmin_Consent_Transactions;

    [BindProperty]
    [Required(ErrorMessage = "You must select a student")]
    public string StudentId { get; set; }

    [BindProperty]
    [Required(ErrorMessage = "You must select a parent")]
    public Guid Submitter { get; set; }
    [BindProperty]
    public string Method { get; set; }
    [BindProperty]
    public string Notes { get; set; }

    [BindProperty]
    [Required(ErrorMessage = "You must select an application")]
    [MinLength(1, ErrorMessage = "You must select an application")]
    public List<ConsentResponse> Responses { get; set; } = new();

    public List<ApplicationSummaryResponse> Applications { get; set; } = new();

    public List<FamilyContactResponse> Parents { get; set; } = new();

    public Dictionary<string, string> Students { get; set; }

    public async Task<IActionResult> OnGet()
    {
        return await PreparePage();
    }

    public async Task<IActionResult> OnPost()
    {
        Dictionary<ApplicationId, bool> responses = new();

        foreach (ConsentResponse entry in Responses)
        {
            if (entry.ApplicationId == Guid.Empty)
                continue;

            ApplicationId applicationId = ApplicationId.FromValue(entry.ApplicationId);

            responses[applicationId] = entry.Consent;
        }

        if (!responses.Any())
        {
            ModelState.AddModelError("Responses", "You must include at least one valid application");

            ModalContent = new ErrorDisplay(new("Consent Required", "No valid consent responses were entered"));

            return await PreparePage();
        }

        Result<List<FamilyContactResponse>> family = await _mediator.Send(new GetFamilyContactsForStudentQuery(StudentId));

        if (family.IsFailure)
        {
            ModalContent = new ErrorDisplay(family.Error);

            return await PreparePage();
        }

        ParentId parentId = ParentId.FromValue(Submitter);

        FamilyContactResponse contact = family.Value.FirstOrDefault(entry => entry.ParentId == parentId);

        if (contact is null)
        {
            ModalContent = new ErrorDisplay(DomainErrors.Families.Parents.NotFoundInFamily(parentId, family.Value.First().FamilyId.Value));

            return await PreparePage();
        }

        string contactName = $"{contact.Name} ({contact.EmailAddress})";

        Result attempt = await _mediator.Send(new CreateTransactionCommand(
            StudentId,
            contactName,
            ConsentMethod.FromValue(Method),
            Notes,
            responses));

        if (attempt.IsFailure)
        {
            ModalContent = new ErrorDisplay(attempt.Error);

            return await PreparePage();
        }

        return RedirectToPage("/SchoolAdmin/Consent/Responses/Index", new { area = "Staff" });
    }

    public async Task<IActionResult> OnPostAjaxGetParents(string studentId)
    {
        Result<List<FamilyContactResponse>> family = await _mediator.Send(new GetFamilyContactsForStudentQuery(studentId));

        if (family.IsSuccess)
            return new JsonResult(family.Value);

        return new JsonResult(null);
    }

    private async Task<IActionResult> PreparePage()
    {
        Result<List<ApplicationSummaryResponse>> applications = await _mediator.Send(new GetApplicationsQuery());

        if (applications.IsFailure)
        {
            ModalContent = new ErrorDisplay(
                applications.Error,
                _linkGenerator.GetPathByPage("/SchoolAdmin/Consent/Responses/Index", values: new { area = "Staff" }));

            return Page();
        }
            
        Applications = applications.Value;

        Result<Dictionary<string, string>> students = await _mediator.Send(new GetCurrentStudentsAsDictionaryQuery());

        if (students.IsFailure)
        {
            ModalContent = new ErrorDisplay(
                students.Error,
                _linkGenerator.GetPathByPage("/SchoolAdmin/Consent/Responses/Index", values: new { area = "Staff" }));

            return Page();
        }

        Students = students.Value;

        if (StudentId is not null)
        {
            Result<List<FamilyContactResponse>> family = await _mediator.Send(new GetFamilyContactsForStudentQuery(StudentId));

            if (family.IsSuccess)
                Parents = family.Value;
        }

        return Page();
    }

    public class ConsentResponse
    {
        public Guid ApplicationId { get; set; }
        public bool Consent { get; set; }
    }
}

