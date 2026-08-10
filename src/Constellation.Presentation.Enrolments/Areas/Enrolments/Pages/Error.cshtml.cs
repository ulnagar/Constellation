namespace Constellation.Presentation.Enrolments.Areas.Enrolments.Pages;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Models;

[AllowAnonymous]
public class ErrorModel : BasePageModel
{
    public ErrorModel()
    {
        
    }


    public async Task OnGet()
    {
    }
}