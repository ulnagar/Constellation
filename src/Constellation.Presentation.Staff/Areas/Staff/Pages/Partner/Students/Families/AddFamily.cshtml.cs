namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Partner.Students.Families;

using Application.Common.PresentationModels;
using Constellation.Application.Families.CreateFamily;
using Constellation.Application.Models.Auth;
using Constellation.Core.ValueObjects;
using Constellation.Presentation.Staff.Areas;
using Core.Models.Families;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System.ComponentModel.DataAnnotations;

[Authorize(Policy = AuthPolicies.CanEditStudents)]
public class AddFamilyModel : BasePageModel
{
    private readonly IMediator _mediator;
    private readonly LinkGenerator _linkGenerator;

    public AddFamilyModel(
        IMediator mediator,
        LinkGenerator linkGenerator)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.Partner_Students_Families;
    [ViewData] public string PageTitle => "Add New Family";

    [BindProperty]
    [Display(Name = "Family Title")]
    [Required]
    public string FamilyTitle { get; set; } = string.Empty;
    
    [BindProperty]
    [Required]
    [Display(Name = "Address Line 1")]
    public string AddressLine1 { get; set; } = string.Empty;
    
    [BindProperty]
    [Display(Name = "Address Line 2")]
    public string? AddressLine2 { get; set; } = string.Empty;

    [BindProperty]
    [Required]
    [Display(Name = "Town")]
    public string AddressTown { get; set; } = string.Empty;
    
    [BindProperty]
    [Required]
    [Display(Name = "Post Code")]
    public string AddressPostCode { get; set; } = string.Empty;
    
    [BindProperty]
    [EmailAddress]
    [Required]
    [Display(Name = "Family Email Address")]
    public string FamilyEmail { get; set; } = string.Empty;

    public IActionResult OnGet() => Page();

    public async Task<IActionResult> OnPostAdd(CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return Page();

        Result<EmailAddress> email = EmailAddress.Create(FamilyEmail);

        if (email.IsFailure)
        {
            ModelState.AddModelError("FamilyEmail", email.Error.Message);

            return Page();
        }

        Result<Family> result = await _mediator.Send(new CreateFamilyCommand(
            FamilyTitle,
            AddressLine1,
            AddressLine2,
            AddressTown,
            AddressPostCode,
            email.Value), cancellationToken);

        if (result.IsSuccess)
            return RedirectToPage("/Partner/Students/Families/Details", new { area = "Staff", Id = result.Value.Id.Value });

        ModalContent = new ErrorDisplay(
            result.Error,
            _linkGenerator.GetPathByPage("/Partner/Students/Families/Index", values: new { area = "Staff" }));

        return Page();
    }
}
