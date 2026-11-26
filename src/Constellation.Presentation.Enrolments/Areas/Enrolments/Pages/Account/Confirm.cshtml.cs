namespace Constellation.Presentation.Enrolments.Areas.Enrolments.Pages.Account;

using Application.Domains.EnrolmentContext.Interfaces;
using Application.Extensions;
using Application.Interfaces.Services;
using Core.Models.Auth.Enums;
using Core.Models.EnrolmentContext.EnrolmentUser;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OfficeOpenXml.FormulaParsing.LexicalAnalysis;
using Serilog;

[AllowAnonymous]
public class ConfirmModel : PageModel
{
    private readonly UserManager<EnrolmentUser> _userManager;
    private readonly SignInManager<EnrolmentUser> _signInManager;
    private readonly IEnrolmentUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public ConfirmModel(
        UserManager<EnrolmentUser> userManager,
        SignInManager<EnrolmentUser> signInManager,
        IEnrolmentUnitOfWork unitOfWork,
        ILogger logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _unitOfWork = unitOfWork;
        _logger = logger
            .ForContext<IAuthService>();
    }

    public async Task<IActionResult> OnGet(Guid userId, string token)
    {
        EnrolmentUser? user = await _userManager.FindByIdAsync(userId.ToString());

        if (user is null)
            return InvalidLink("Enrolment User Not Found", null);

        if (!user.EmailConfirmed)
        {
            IdentityResult result = await _userManager.ConfirmEmailAsync(user, token);

            if (!result.Succeeded)
                return InvalidLink("Failed to Confirm Email", user);
        }
        else
        {
            bool valid = await _userManager.VerifyEmailLoginTokenAsync(user, token);

            if (!valid)
                return InvalidLink("Invalid Magic Link Token", user);
        }

        user.AddLogin(DateTime.UtcNow, LoginStatus.Success);
        await _signInManager.SignInAsync(user, isPersistent: true);
        await _unitOfWork.CompleteAsync();

        return RedirectToPage("/Index", routeValues: new { area = "Enrolments" });
    }

    public async Task<IActionResult> OnGetBypass(string email)
    {
        EnrolmentUser? user = await _userManager.FindByEmailAsync(email);

        if (user is null)
            return InvalidLink("Enrolment User Not Found", null);

        await _signInManager.SignInAsync(user, isPersistent: true);

        return RedirectToPage("/Index", routeValues: new { area = "Enrolments" });
    }

    public async Task<IActionResult> OnPostSmsToken(Guid userId, string token)
    {
        EnrolmentUser? user = await _userManager.FindByIdAsync(userId.ToString());

        if (user is null)
            return InvalidToken("Enrolment User Not Found", null);

        bool valid = await _userManager.VerifyPhoneLoginTokenAsync(user, token);

        if (!valid)
            return InvalidToken("Invalid SMS Token", user);

        // Process the SMS token validation and login
        user.AddLogin(DateTime.UtcNow, LoginStatus.SmsToken);
        await _signInManager.SignInAsync(user, isPersistent: true);
        await _unitOfWork.CompleteAsync();

        return RedirectToPage("/Index", routeValues: new { area = "Enrolments" });
    }

    private IActionResult InvalidLink(string reason, EnrolmentUser? user = null)
    {
        _logger
            .ForContext(nameof(EnrolmentUser), user, true)
            .Warning("Invalid enrolment portal link: {Reason}", reason);
        
        return RedirectToPage("/Account/InvalidLink", routeValues: new { area = "Enrolments" });
    }

    private IActionResult InvalidToken(string reason, EnrolmentUser? user = null)
    {
        _logger
            .ForContext(nameof(EnrolmentUser), user, true)
            .Warning("Invalid enrolment portal token: {Reason}", reason);

        return RedirectToPage("/Account/InvalidToken", routeValues: new { area = "Enrolments" });
    }
}