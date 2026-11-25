namespace Constellation.Presentation.Enrolments.Areas.Enrolments.Pages.Account;

using Application.Domains.EnrolmentContext.EnrolmentUser.Commands.RegisterNewUser;
using Core.Shared;
using Core.ValueObjects;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Models;
using Serilog;

[AllowAnonymous]
public class RegisterModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly ILogger _logger;

    public RegisterModel(
        ISender mediator,
        ILogger logger)
    {
        _mediator = mediator;
        _logger = logger
            .ForContext<RegisterModel>();
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

    public string Message { get; set; } = string.Empty;

    public async Task OnGet()
    {
    }

    public async Task<IActionResult> OnPost()
    {
        PhoneNumber mobile = PhoneNumber.Empty;

        if (!string.IsNullOrWhiteSpace(Phone))
        {
            Result<PhoneNumber> phoneNumber = PhoneNumber.Create(Phone);

            if (phoneNumber.IsFailure)
                ModelState.AddModelError(nameof(Phone), phoneNumber.Error.Message);
            else
                mobile = phoneNumber.Value;
        }

        Result<EmailAddress> emailAddress = EmailAddress.Create(Email);

        if (emailAddress.IsFailure)
            ModelState.AddModelError(nameof(Email), emailAddress.Error.Message);

        Result<Name> name = Name.Create(FirstName, PreferredName, LastName);

        if (name.IsFailure)
            ModelState.AddModelError(nameof(FirstName), name.Error.Message);
        
        if (!ModelState.IsValid)
            return Page();
        
        // Process the registration.
        RegisterNewUserCommand command = new(
            name.Value,
            emailAddress.Value,
            mobile);

        Result result = await _mediator.Send(command);

        if (result.IsFailure)
        {
            _logger
                .ForContext(nameof(RegisterNewUserCommand), command, true)
                .ForContext(nameof(Error), result.Error, true)
                .Warning("Failed to process new user registration");

            Message = "An error occured during user registration. Please try again.";
            
            return Page();
        }

        Message = "Please check your email for a confirmation link.";

        return Page();
    }
}