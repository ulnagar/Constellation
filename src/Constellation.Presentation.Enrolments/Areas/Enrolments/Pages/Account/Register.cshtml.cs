namespace Constellation.Presentation.Enrolments.Areas.Enrolments.Pages.Account;

using Core.Shared;
using Core.ValueObjects;
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

    [BindProperty]
    public string Email { get; set; }
    [BindProperty]
    public string FirstName { get; set; }
    [BindProperty]
    public string? PreferredName { get; set; }
    [BindProperty]
    public string LastName { get; set; }
    [BindProperty]
    public string? Phone { get; set; }

    public async Task OnGet()
    {
    }

    public async Task<IActionResult> OnPost()
    {
        if (!string.IsNullOrWhiteSpace(Phone))
        {
            Result<PhoneNumber> phoneNumber = PhoneNumber.Create(Phone);

            if (phoneNumber.IsFailure)
                ModelState.AddModelError(nameof(Phone), phoneNumber.Error.Message);
        }

        Result<EmailAddress> emailAddress = EmailAddress.Create(Email);

        if (emailAddress.IsFailure)
            ModelState.AddModelError(nameof(Email), emailAddress.Error.Message);

        if (!ModelState.IsValid)
            return Page();

        // Process the registration.
        // Send the email confirmation.

        return Page();
    }
}