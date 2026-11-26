namespace Constellation.Presentation.Enrolments.Areas.Enrolments.Pages.Account;

using Application.Domains.EnrolmentContext.Interfaces;
using Core.Shared;
using Core.ValueObjects;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models;
using Serilog;

[AllowAnonymous]
public class LoginModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly IEnrolmentAuthService _authService;
    private readonly ILogger _logger;

    public LoginModel(
        ISender mediator,
        IEnrolmentAuthService authService,
        ILogger logger)
    {
        _mediator = mediator;
        _authService = authService;
        _logger = logger
            .ForContext<LoginModel>();
    }

    [BindProperty(SupportsGet=true)]
    public LoginStatus Status { get; set; } = LoginStatus.Start;

    public string Message { get; set; } = string.Empty;

    public Guid UserId { get; set; }

    public void OnGet()
    {
    }

    public async Task OnPostLoginViaEmail(string email, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            ModelState.AddModelError("email", "Email is required");

            return;
        }

        Result<EmailAddress> emailAddress = EmailAddress.Create(email);

        if (emailAddress.IsFailure)
        {
            ModelState.AddModelError("email", "Email must be valid");

            return;
        }

        // Send the login link to email
        Result login = await _authService.SendLoginTokenViaEmail(emailAddress.Value, cancellationToken);

        if (login.IsFailure)
        {
            _logger
                .ForContext(nameof(EmailAddress), emailAddress.Value)
                .ForContext(nameof(Error), login.Error, true)
                .Warning("Failed to log in user");
        }

        Status = LoginStatus.ShowMessage;
        Message = "If there's an account for that address, we've sent a link to it. It can take a few minutes to arrive.";
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