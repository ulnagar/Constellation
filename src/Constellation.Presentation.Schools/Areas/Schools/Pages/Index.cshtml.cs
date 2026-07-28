namespace Constellation.Presentation.Schools.Areas.Schools.Pages;

using Constellation.Application.Models.Auth;
using Constellation.Presentation.Shared.Helpers.Attributes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

[HasPermission(AuthPermission.SchoolsPortal_View_Value)]
public class IndexModel : PageModel
{
    // Redirect links to /schools to the new /Schools/Dashboard page
    public IActionResult OnGet() => RedirectToPage("/Dashboard", new { area = "Schools" });
}