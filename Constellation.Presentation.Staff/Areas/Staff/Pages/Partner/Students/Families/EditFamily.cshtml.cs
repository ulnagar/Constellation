namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Partner.Students.Families;

using Constellation.Application.Families.GetFamilyEditContext;
using Constellation.Application.Families.UpdateFamily;
using Constellation.Application.Models.Auth;
using Constellation.Core.Models.Identifiers;
using Constellation.Presentation.Staff.Areas;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
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

    [ViewData] public string ActivePage => Presentation.Staff.Pages.Shared.Components.StaffSidebarMenu.ActivePage.Partner_Students_Families;

    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    [BindProperty]
    [Display(Name = "Family Title")]
    [Required]
    public string FamilyTitle { get; set; } = string.Empty;
    [BindProperty]
    [Display(Name = "Address Line 1")]
    public string AddressLine1 { get; set; } = string.Empty;
    [BindProperty]
    [Display(Name = "Address Line 2")]
    public string AddressLine2 { get; set; } = string.Empty;
    [BindProperty]
    [Display(Name = "Town")]
    public string AddressTown { get; set; } = string.Empty;
    [BindProperty]
    [Display(Name = "Post Code")]
    public string AddressPostCode { get; set; } = string.Empty;
    [BindProperty]
    [EmailAddress]
    [Required]
    [Display(Name = "Family Email Address")]
    public string FamilyEmail { get; set; } = string.Empty;

    public List<string> Students { get; set; } = new();
    public List<string> Parents { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        var familyId = FamilyId.FromValue(Id);

        var familyResult = await _mediator.Send(new GetFamilyEditContextQuery(familyId), cancellationToken);

        if (familyResult.IsFailure)
        {
            Error = new()
            {
                Error = familyResult.Error,
                RedirectPath = _linkGenerator.GetPathByPage("/Partner/Students/Families/Index", values: new { area = "Staff" })
            };

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

        var command = new UpdateFamilyCommand(
            FamilyId.FromValue(Id),
            FamilyTitle,
            AddressLine1,
            AddressLine2,
            AddressTown,
            AddressPostCode,
            FamilyEmail);

        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsSuccess)
            return RedirectToPage("/Partner/Students/Families/Index", new { area = "Staff" });

        Error = new()
        {
            Error = result.Error,
            RedirectPath = _linkGenerator.GetPathByPage("/Partner/Students/Families/EditFamily", values: new { area = "Staff", Id = Id })
        };

        return Page();
    }
}
