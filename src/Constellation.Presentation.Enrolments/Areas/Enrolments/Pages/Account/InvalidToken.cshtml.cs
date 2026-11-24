namespace Constellation.Presentation.Enrolments.Areas.Enrolments.Pages.Account;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

[AllowAnonymous]
public class InvalidTokenModel : PageModel
{
    public InvalidTokenModel()
    {
        
    }

    public void OnGet()
    {
    }
}