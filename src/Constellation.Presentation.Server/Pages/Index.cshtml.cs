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
        AuthorizationResult parentPortal = await _authService.AuthorizeAsync(User, AuthPolicies.IsParent);
        AuthorizationResult schoolPortal = await _authService.AuthorizeAsync(User, AuthPolicies.IsSchoolContact);
        AuthorizationResult staffPortal = await _authService.AuthorizeAsync(User, AuthPolicies.IsStaffMember);
        AuthorizationResult studentPortal = await _authService.AuthorizeAsync(User, AuthPolicies.IsStudent);

        CanAccessParentPortal = parentPortal.Succeeded;
        CanAccessSchoolPortal = schoolPortal.Succeeded;
        CanAccessStaffPortal = staffPortal.Succeeded;
        CanAccessStudentPortal = studentPortal.Succeeded;

        if (CanAccessParentPortal && !CanAccessSchoolPortal && !CanAccessStaffPortal && !CanAccessStudentPortal)
            return RedirectToPage("/Index", new { area = "Parents" });

        if (!CanAccessParentPortal && CanAccessSchoolPortal && !CanAccessStaffPortal && !CanAccessStudentPortal)
            return RedirectToPage("/Dashboard", new { area = "Schools" });

        if (!CanAccessParentPortal && !CanAccessSchoolPortal && CanAccessStaffPortal && !CanAccessStudentPortal)
            return RedirectToPage("/Dashboard", new { area = "Staff" });

        if (!CanAccessParentPortal && !CanAccessSchoolPortal && !CanAccessStaffPortal && CanAccessStudentPortal)
            return RedirectToPage("/Index", new { area = "Students" });

        return Page();
    }
}
