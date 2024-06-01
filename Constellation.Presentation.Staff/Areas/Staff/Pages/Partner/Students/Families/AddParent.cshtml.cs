namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Partner.Students.Families;

using Constellation.Application.Families.CreateParent;
using Constellation.Application.Helpers;
using Constellation.Application.Models.Auth;
using Constellation.Core.Models.Identifiers;
using Constellation.Presentation.Staff.Areas;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System.ComponentModel.DataAnnotations;

[Authorize(Policy = AuthPolicies.CanEditStudents)]
public class AddParentModel : BasePageModel
{
    private readonly IMediator _mediator;
    private readonly LinkGenerator _linkGenerator;

    public AddParentModel(
        IMediator mediator,
        LinkGenerator linkGenerator)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
    }

    [ViewData] public string ActivePage => Presentation.Staff.Pages.Shared.Components.StaffSidebarMenu.ActivePage.Partner_Students_Families;

    [BindProperty(SupportsGet = true)]
    public Guid FamilyIdentifier { get; set; }

    [BindProperty]
    public string Title { get; set; } = string.Empty;
    [BindProperty]
    [Required]
    [Display(Name = DisplayNameDefaults.FirstName)]
    public string FirstName { get; set; } = string.Empty;
    [BindProperty]
    [Required]
    [Display(Name = DisplayNameDefaults.LastName)]
    public string LastName { get; set; } = string.Empty;
    [BindProperty]
    [Phone]
    [Display(Name = DisplayNameDefaults.MobileNumber)]
    public string MobileNumber { get; set; } = string.Empty;
    [BindProperty]
    [EmailAddress]
    [Required]
    [Display(Name = DisplayNameDefaults.EmailAddress)]
    public string EmailAddress { get; set; } = string.Empty;

    public async Task<IActionResult> OnGet()
    {
        return Page();
    }

    public async Task<IActionResult> OnPostCreate(CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) 
            return Page();

        var familyId = FamilyId.FromValue(FamilyIdentifier);

        var command = new CreateParentCommand(
            familyId,
            Title,
            FirstName,
            LastName,
            MobileNumber,
            EmailAddress);

        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsSuccess)
            return RedirectToPage("/Partner/Students/Families/Details", new { area = "Staff", Id = familyId.Value });

        Error = new()
        {
            Error = result.Error,
            RedirectPath = _linkGenerator.GetPathByPage("/Partner/Students/Families/Details", values: new { area = "Staff", Id = familyId.Value })
        };

        return Page();
    }
}
