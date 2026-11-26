namespace Constellation.Application.Domains.EnrolmentContext.EnrolmentUser.Commands.RegisterNewUser;

using Abstractions;
using Abstractions.Messaging;
using Application.Interfaces.Services;
using Constellation.Core.ValueObjects;
using Core.Models.EnrolmentContext.EnrolmentUser;
using Core.Shared;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.WebUtilities;
using Serilog;

internal sealed class RegisterNewUserCommandHandler
: ICommandHandler<RegisterNewUserCommand>
{
    private readonly UserManager<EnrolmentUser> _userManager;
    private readonly IEmailService _emailService;
    private readonly ILogger _logger;

    public RegisterNewUserCommandHandler(
        UserManager<EnrolmentUser> userManager,
        IEmailService emailService,
        ILogger logger)
    {
        _userManager = userManager;
        _emailService = emailService;
        _logger = logger
            .ForContext<RegisterNewUserCommandHandler>();
    }

    public async Task<Result> Handle(RegisterNewUserCommand request, CancellationToken cancellationToken)
    {
        EnrolmentUser user = new EnrolmentUser()
        {
            UserName = request.Email,
            Email = request.Email,
            Name = request.UserName
        };

        if (request.Mobile is not null && request.Mobile != PhoneNumber.Empty)
            user.PhoneNumber = request.Mobile.Value;
        
        IdentityResult result = await _userManager.CreateAsync(user);

        if (!result.Succeeded)
        {
            _logger
                .ForContext(nameof(Name), request.UserName, true)
                .ForContext(nameof(EmailAddress), request.Email, true)
                .ForContext(nameof(IdentityResult), result, true)
                .Warning("Failed to create new user");

            return Result.Failure(Error.FromIdentityResult(result));
        }

        string token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

        string link = QueryHelpers.AddQueryString(
            $"{AppConstants.BaseUrl}/Enrolments/Account/Confirm",
            new Dictionary<string, string?> { ["userId"] = user.Id.ToString(), ["token"] = token });

        Result emailResult = await _emailService.SendRegistrationConfirmationEmail(request.UserName, request.Email, link, cancellationToken);

        if (emailResult.IsFailure)
        {
            _logger
                .ForContext(nameof(Name), request.UserName, true)
                .ForContext("Token", token)
                .ForContext(nameof(Error), emailResult.Error, true)
                .Error("Failed to send registration confirmation email");
        }

        return Result.Success();
    }
}
