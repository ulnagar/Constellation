namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Partner.Students.Families;

using Application.Helpers;
using Application.Models.Auth;
using Areas;
using Constellation.Application.Families.CreateParent;
using Core.Models.Families;
using Core.Models.Identifiers;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Presentation.Shared.Helpers.ModelBinders;
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

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.Partner_Students_Families;
    [ViewData] public string PageTitle => "Add New Parent";


    [BindProperty(SupportsGet = true)]
    [ModelBinder(typeof(StrongIdBinder))]
    public FamilyId FamilyId { get; set; }

    [BindProperty]
    public string? Title { get; set; } = string.Empty;

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
    public string? MobileNumber { get; set; } = string.Empty;

    [BindProperty]
    [EmailAddress]
    [Required]
    [Display(Name = DisplayNameDefaults.EmailAddress)]
    public string EmailAddress { get; set; } = string.Empty;

    public IActionResult OnGet()=> Page();

    public async Task<IActionResult> OnPostCreate(CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) 
            return Page();
        
        CreateParentCommand command = new(
            FamilyId,
            Title,
            FirstName,
            LastName,
            MobileNumber,
            EmailAddress);

        Result<Parent> result = await _mediator.Send(command, cancellationToken);

        if (result.IsSuccess)
            return RedirectToPage("/Partner/Students/Families/Details", new { area = "Staff", Id = FamilyId.Value });

        Error = new()
        {
            Error = result.Error,
            RedirectPath = _linkGenerator.GetPathByPage("/Partner/Students/Families/Details", values: new { area = "Staff", Id = FamilyId.Value })
        };

        return Page();
    }
}
