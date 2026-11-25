namespace Constellation.Presentation.Server.Pages;

using Application.Models.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

[Authorize]
public class IndexModel : PageModel
{
    private readonly IAuthorizationService _authService;

    public IndexModel(
        IAuthorizationService authService)
    {
        _authService = authService;
    }

    public bool CanAccessParentPortal { get; set; }
    public bool CanAccessSchoolPortal { get; set; }
    public bool CanAccessStaffPortal { get; set; }
    public bool CanAccessStudentPortal { get; set; }

    public async Task<IActionResult> OnGet()
    {
        CanAccessParentPortal = await IsAuthorizedAsync(AuthPolicies.IsParent);
        CanAccessSchoolPortal = await IsAuthorizedAsync(AuthPolicies.IsSchoolContact);
        CanAccessStaffPortal = await IsAuthorizedAsync(AuthPolicies.IsStaffMember);
        CanAccessStudentPortal = await IsAuthorizedAsync(AuthPolicies.IsStudent);

        var portals = new List<(string Page, string Area)>();

        if (CanAccessParentPortal) portals.Add(("/Index", "Parents"));
        if (CanAccessSchoolPortal) portals.Add(("/Dashboard", "Schools"));
        if (CanAccessStaffPortal) portals.Add(("/Dashboard", "Staff"));
        if (CanAccessStudentPortal) portals.Add(("/Index", "Students"));

        // Only one portal available: skip the chooser and go straight there
        if (portals.Count == 1)
        {
            var (page, area) = portals[0];
            return RedirectToPage(page, new { area });
        }

        return Page();
    }

    private async Task<bool> IsAuthorizedAsync(string policy)
    {
        var result = await _authService.AuthorizeAsync(User, policy);
        return result.Succeeded;
    }
}
