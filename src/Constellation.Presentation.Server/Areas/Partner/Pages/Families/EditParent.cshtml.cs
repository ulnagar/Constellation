namespace Constellation.Presentation.Server.Areas.Partner.Pages.Families;

using Constellation.Application.Families.GetParentEditContext;
using Constellation.Application.Families.UpdateParent;
using Constellation.Application.Helpers;
using Constellation.Application.Models.Auth;
using Constellation.Core.Models.Identifiers;
using Constellation.Presentation.Server.BaseModels;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

[Authorize(Policy = AuthPolicies.CanEditStudents)]
public class EditParentModel : BasePageModel
{
    private readonly IMediator _mediator;
    private readonly LinkGenerator _linkGenerator;

    public EditParentModel(
        IMediator mediator,
        LinkGenerator linkGenerator)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
    }

    [BindProperty(SupportsGet = true)]
    public Guid ParentIdentifier { get; set; }
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

    public async Task<IActionResult> OnGet(CancellationToken cancellationToken)
    {
        var parentId = ParentId.FromValue(ParentIdentifier);
        var familyId = FamilyId.FromValue(FamilyIdentifier);

        var parentResult = await _mediator.Send(new GetParentEditContextQuery(familyId, parentId), cancellationToken);

        if (parentResult.IsFailure)
        {
            Error = new()
            {
                Error = parentResult.Error,
                RedirectPath = _linkGenerator.GetPathByPage("/Families/Index", values: new { area = "Partner" })
            };

            return Page();
        }

        Title = parentResult.Value.Title;
        FirstName = parentResult.Value.FirstName;
        LastName = parentResult.Value.LastName;
        MobileNumber = parentResult.Value.MobileNumber;
        EmailAddress = parentResult.Value.EmailAddress;

        return Page();
    }

    public async Task<IActionResult> OnPostUpdate(CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var parentId = ParentId.FromValue(ParentIdentifier);
        var familyId = FamilyId.FromValue(FamilyIdentifier);

        var command = new UpdateParentCommand(
            parentId,
            familyId,
            Title,
            FirstName,
            LastName,
            MobileNumber,
            EmailAddress);

        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsSuccess)
            return RedirectToPage("/Families/Index", new { area = "Partner" });

        Error = new()
        {
            Error = result.Error,
            RedirectPath = _linkGenerator.GetPathByPage("/Families/Index", values: new { area = "Partner" })
        };

        return Page();
    }
}
