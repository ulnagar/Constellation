namespace Constellation.Presentation.Enrolments.Areas.Enrolments.Pages.Account;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Models;

[AllowAnonymous]
public class LoginModel : BasePageModel
{
    public LoginModel()
    {
        
    }

    [BindProperty(SupportsGet=true)]
    public LoginStatus Status { get; set; } = LoginStatus.Start;

    public string Message { get; set; } = string.Empty;

    public Guid UserId { get; set; }

    public void OnGet()
    {
    }

    public async Task OnPostLoginViaEmail(string email)
    {
        // Send the login link to email

        Status = LoginStatus.ShowMessage;
        Message = "A login link has been sent to your email.";
    }

    public async Task OnPostLoginViaSms(string phoneNumber)
    {
        // Lookup the user by phone number

        // Send the SMS token to the user's phone

        // Update the UserId with the user's ID for the next step

        Status = LoginStatus.PromptForToken;
    }

    public async Task OnPostValidateSmsToken(Guid userId, string token)
    {
        // Validate the token and process login

    }


    public enum LoginStatus
    {
        Start,
        PromptForEmail,
        PromptForPhone,
        PromptForToken,
        ShowMessage
    }
}