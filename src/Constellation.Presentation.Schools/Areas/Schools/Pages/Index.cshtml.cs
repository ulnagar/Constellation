namespace Constellation.Presentation.Schools.Areas.Schools.Pages;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

public class IndexModel : PageModel
{
    // Redirect links to /schools to the new /Schools/Dashboard page
    public IActionResult OnGet() => RedirectToPage("/Dashboard", new { area = "Schools" });
}