namespace Constellation.Presentation.Applicants.Areas.Applicants.Pages.Error;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

[AllowAnonymous]
public class InvalidApplicationIdModel : PageModel
{
    public void OnGet()
    {
    }
}