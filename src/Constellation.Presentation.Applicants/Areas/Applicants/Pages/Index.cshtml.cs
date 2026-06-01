namespace Constellation.Presentation.Applicants.Areas.Applicants.Pages;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

[AllowAnonymous]
public class IndexModel : PageModel
{
    public IndexModel()
    {
        
    }

    public async Task OnGet()
    {
    }
}