namespace Constellation.Presentation.Enrolments.Areas.Enrolments.Pages.Account;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Models;

[AllowAnonymous]
public class RegisterModel : BasePageModel
{
    public RegisterModel()
    {
        
    }

    public async Task OnGet()
    {
    }
}