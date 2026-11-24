namespace Constellation.Presentation.Enrolments.Areas.Enrolments.Pages.Account;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

[AllowAnonymous]
public class InvalidLinkModel : PageModel
{
    public InvalidLinkModel()
    {
        
    }

    public async Task OnGet()
    {
    }
}