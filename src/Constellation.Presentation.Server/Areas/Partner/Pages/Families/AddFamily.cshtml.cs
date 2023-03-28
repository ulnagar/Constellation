namespace Constellation.Presentation.Server.Areas.Partner.Pages.Families;

using Constellation.Application.Families.CreateFamily;
using Constellation.Application.Models.Auth;
using Constellation.Core.ValueObjects;
using Constellation.Presentation.Server.BaseModels;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

    public async Task<IActionResult> OnGet()
    {
        await GetClasses(_mediator);

        return Page();
    }

    public async Task<IActionResult> OnPostAdd(CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var email = EmailAddress.Create(FamilyEmail);

        if (email.IsFailure)
        {
            ModelState.AddModelError("FamilyEmail", email.Error.Message);

            return Page();
        }

        var result = await _mediator.Send(new CreateFamilyCommand(
            FamilyTitle,
            AddressLine1,
            AddressLine2,
            AddressTown,
            AddressPostCode,
            email.Value), cancellationToken);

        if (result.IsSuccess)
            return RedirectToPage("/Families/Details", new { area = "Partner", Id = result.Value.Id.Value });

        Error = new()
        {
            Error = result.Error,
            RedirectPath = _linkGenerator.GetPathByPage("/Families/Index", values: new { area = "Partner" })
        };

        return Page();
    }
}
