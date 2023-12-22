namespace Constellation.Presentation.Server.Areas.Admin.Pages.Auth;

using Constellation.Application.Models.Auth;
using Constellation.Application.Models.Identity;
using Constellation.Presentation.Server.BaseModels;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

[Authorize(Policy = AuthPolicies.IsSiteAdmin)]
public class EditModel : BasePageModel
{
    private readonly IMediator _mediator;
    private readonly UserManager<AppUser> _userManager;

    public EditModel(IMediator mediator, UserManager<AppUser> userManager)
    {
        _mediator = mediator;
        _userManager = userManager;
    }

    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    [BindProperty]
    public string FirstName { get; set; }

    [BindProperty]
    public string LastName { get; set; }

    [BindProperty]
    public bool IsStaffMember { get; set; }

    [BindProperty]
    public string StaffId { get; set; }

    [BindProperty]
    public bool IsSchoolContact { get; set; }

    [BindProperty]
    public int SchoolContactId { get; set; }

    [BindProperty]
    public bool IsParent { get; set; }

    public async Task<IActionResult> OnGet()
    {
        var user = await _userManager.FindByIdAsync(Id.ToString());

        if (user is null)
        {
            return RedirectToPage("Index");
        }

        FirstName = user.FirstName;
        LastName = user.LastName;
        IsStaffMember = user.IsStaffMember;
        StaffId = user.StaffId;
        IsSchoolContact = user.IsSchoolContact;
        SchoolContactId = user.SchoolContactId;
        IsParent = user.IsParent;

        return Page();
    }

    public async Task<IActionResult> OnPostUpdate()
    {
        var user = await _userManager.FindByIdAsync(Id.ToString());

        if (user is null)
        {
            return RedirectToPage("Index");
        }

        user.FirstName = FirstName;
        user.LastName = LastName;
        user.IsStaffMember = IsStaffMember;
        user.StaffId = StaffId;
        user.IsSchoolContact = IsSchoolContact;
        user.SchoolContactId = SchoolContactId;
        user.IsParent = IsParent;

        await _userManager.UpdateAsync(user);

        return RedirectToPage("Info", new { EmailAddress = user.Email });
    }
}
