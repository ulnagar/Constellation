namespace Constellation.Presentation.Enrolments.Areas.Enrolments.Pages;

using Constellation.Presentation.Enrolments.Areas.Enrolments.Models;
using Microsoft.AspNetCore.Authorization;

[AllowAnonymous]
public class IndexModel : BasePageModel
{
    public IndexModel()
    {
        
    }
    
    public async Task OnGet()
    {
    }
}