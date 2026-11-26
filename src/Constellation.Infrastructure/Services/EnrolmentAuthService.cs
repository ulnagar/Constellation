namespace Constellation.Infrastructure.Services;

using Application.Abstractions;
using Application.Domains.EnrolmentContext.Interfaces;
using Application.Extensions;
using Application.Interfaces.Services;
using Constellation.Application.DTOs.EmailRequests;
using Constellation.Core.Enums;
using Constellation.Core.Models.Auth.Enums;
using Core.Models.EnrolmentContext.EnrolmentUser;
using Core.Models.EnrolmentContext.EnrolmentUser.Errors;
using Core.Shared;
using Core.ValueObjects;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using System;

internal sealed class EnrolmentAuthService
: IEnrolmentAuthService
{
    private readonly UserManager<EnrolmentUser> _userManager;
    private readonly SignInManager<EnrolmentUser> _signInManager;
    private readonly IEmailService _emailService;
    private readonly ILogger _logger;

    public EnrolmentAuthService(
        UserManager<EnrolmentUser> userManager,
        SignInManager<EnrolmentUser> signInManager,
        IEmailService emailService,
        ILogger logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _emailService = emailService;
        _logger = logger
            .ForContext<EnrolmentAuthService>();
    }

    public async Task<Result> SendLoginTokenViaEmail(EmailAddress email, CancellationToken cancellationToken = default)
    {
        EnrolmentUser? user = await _userManager.FindByEmailAsync(email);

        if (user is null)
        {
            _logger
                .ForContext(nameof(EmailAddress), email)
                .ForContext(nameof(Error), EnrolmentUserErrors.NotFoundByEmail(email), true)
                .Warning("Failed to send login token via email");

            return Result.Failure(EnrolmentUserErrors.NotFoundByEmail(email));
        }

        string token = await _userManager.GenerateEmailLoginTokenAsync(user);

        // Create login url with embedded token
        string link = QueryHelpers.AddQueryString(
            $"{AppConstants.BaseUrl}/Enrolments/Account/Confirm",
            new Dictionary<string, string?> { ["userId"] = user.Id.ToString(), ["token"] = token });

        // Email login url to user
        MagicLinkEmail notification = new()
        {
            Link = link,
            Name = user.Name.DisplayName
        };

        Result<EmailRecipient> recipient = EmailRecipient.Create(user.Name.DisplayName, user.Email!);

        if (recipient.IsFailure)
        {
            _logger
                .ForContext(nameof(EnrolmentUser), user, true)
                .ForContext(nameof(Error), recipient.Error, true)
                .Warning("Failed to send login token via email");

            return Result.Failure(recipient.Error);
        }

        notification.Recipients.Add(recipient.Value);

        await _emailService.SendMagicLinkLoginEmail(notification);
        
        return Result.Success();
    }

    public Task<Result> SendLoginTokenViaSms(PhoneNumber number, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
