namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Partner.Students.Families;

using Application.Common.PresentationModels;
using Application.Models.Auth;
using Areas;
using Constellation.Application.Families.GetFamilyEditContext;
using Constellation.Application.Families.UpdateFamily;
using Core.Models.Identifiers;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Presentation.Shared.Helpers.ModelBinders;
using System.ComponentModel.DataAnnotations;

[Authorize(Policy = AuthPolicies.CanEditStudents)]
public class EditFamilyModel : BasePageModel
{
    private readonly IMediator _mediator;
    private readonly LinkGenerator _linkGenerator;

    public EditFamilyModel(
        IMediator mediator,
        LinkGenerator linkGenerator)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.Partner_Students_Families;
    [ViewData] public string PageTitle => "Edit Family";
    // TODO: R1.15.2: Continue with logging updates

    [BindProperty(SupportsGet = true)]
    [ModelBinder(typeof(StrongIdBinder))]
    public FamilyId Id { get; set; }

    [BindProperty]
    [Display(Name = "Family Title")]
    [Required]
    public string FamilyTitle { get; set; } = string.Empty;

    [BindProperty]
    [Display(Name = "Address Line 1")]
    public string? AddressLine1 { get; set; } = string.Empty;

    [BindProperty]
    [Display(Name = "Address Line 2")]
    public string? AddressLine2 { get; set; } = string.Empty;

    [BindProperty]
    [Display(Name = "Town")]
    public string? AddressTown { get; set; } = string.Empty;

    [BindProperty]
    [Display(Name = "Post Code")]
    public string? AddressPostCode { get; set; } = string.Empty;

    [BindProperty]
    [EmailAddress]
    [Required]
    [Display(Name = "Family Email Address")]
    public string FamilyEmail { get; set; } = string.Empty;

    public List<string> Students { get; set; } = new();
    public List<string> Parents { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        Result<FamilyEditContextResponse> familyResult = await _mediator.Send(new GetFamilyEditContextQuery(Id), cancellationToken);

        if (familyResult.IsFailure)
        {
            ModalContent = new ErrorDisplay(
                familyResult.Error,
                _linkGenerator.GetPathByPage("/Partner/Students/Families/Index", values: new { area = "Staff" }));

            return Page();
        }

        FamilyTitle = familyResult.Value.FamilyTitle;
        AddressLine1 = familyResult.Value.AddressLine1;
        AddressLine2 = familyResult.Value.AddressLine2;
        AddressTown = familyResult.Value.AddressTown;
        AddressPostCode = familyResult.Value.AddressPostCode;
        FamilyEmail = familyResult.Value.FamilyEmail;
        Students = familyResult.Value.Students;
        Parents = familyResult.Value.Parents;

        return Page();
    }

    public async Task<IActionResult> OnPostUpdate(CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return Page();

        UpdateFamilyCommand command = new(
            Id,
            FamilyTitle,
            AddressLine1,
            AddressLine2,
            AddressTown,
            AddressPostCode,
            FamilyEmail);

        Result result = await _mediator.Send(command, cancellationToken);

        if (result.IsSuccess)
            return RedirectToPage("/Partner/Students/Families/Index", new { area = "Staff" });

        ModalContent = new ErrorDisplay(
            result.Error,
            _linkGenerator.GetPathByPage("/Partner/Students/Families/EditFamily", values: new { area = "Staff", Id = Id }));

        return Page();
    }
}
