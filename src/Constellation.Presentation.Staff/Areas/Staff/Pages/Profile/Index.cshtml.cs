namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Profile;

using Application.Domains.Auth.Queries.GetUserDetails;
using Application.Domains.Messaging.EmergencyConsole.Commands.UpdateEmergencyConsoleMessageTemplate;
using Constellation.Application.Common.PresentationModels;
using Constellation.Application.Domains.StaffMembers.Commands.UpdateStaffMemberPhoneNumber;
using Constellation.Core.Models.StaffMembers.Identifiers;
using Constellation.Core.Shared;
using Constellation.Presentation.Shared.Helpers.ModelBinders;
using Core.Abstractions.Services;
using Core.Errors;
using Core.Models.Auth;
using Core.ValueObjects;
using Infrastructure.Caches.AuthenticatorMetadata;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Presentation.Shared.Extensions;
using Serilog;

[Authorize]
public class IndexModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly LinkGenerator _linkGenerator;
    private readonly IAuthenticatorMetadataCache _metadata;
    private readonly UserManager<AppUser> _userManager;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public IndexModel(
        ISender mediator,
        LinkGenerator linkGenerator,
        IAuthenticatorMetadataCache metadata,
        UserManager<AppUser> userManager,
        ICurrentUserService currentUserService,
        ILogger logger)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
        _metadata = metadata;
        _userManager = userManager;
        _currentUserService = currentUserService;
        _logger = logger
            .ForContext<IndexModel>();
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.Staff_Profile;
    [ViewData] public string PageTitle => "User Profile";

    public UserResponse CurrentUser { get; set; }

    public List<PasskeyDisplay> Passkeys { get; set; } = [];

    public async Task OnGet()
    {
        await PreparePage();
    }

    public async Task<IActionResult> OnPostStaffPhoneUpdate(StaffId staffId, [ModelBinder(typeof(FromValueBinder))] PhoneNumber phoneNumber)
    {
        if (phoneNumber == PhoneNumber.Empty)
        {
            ModalContent = ErrorDisplay.Create(
                DomainErrors.ValueObjects.PhoneNumber.NumberEmpty);

            await PreparePage();
            return Page();
        }

        Result update = await _mediator.Send(new UpdateStaffMemberPhoneNumberCommand(staffId, phoneNumber));

        if (update.IsFailure)
        {
            ModalContent = ErrorDisplay.Create(update.Error);

            await PreparePage();
            return Page();
        }

        return RedirectToPage();
    }

    private async Task PreparePage()
    {
        Result<UserResponse> user = await _mediator.Send(new GetUserDetailsQuery(User.GetUserId()));

        if (user.IsFailure)
        {
            ModalContent = ErrorDisplay.Create(user.Error);

            return;
        }

        CurrentUser = user.Value;

        Passkeys = CurrentUser.Passkeys.Select(passkey => new PasskeyDisplay(
                passkey.CredentialId.ToString(),
                passkey.Name,
                _metadata?.Get(passkey.aaGuid)?.Name ?? "Unknown Authenticator",
                _metadata?.Get(passkey.aaGuid)?.IconUrl,
                passkey.CreatedAt.ToLocalTime().DateTime))
            .ToList();
    }

    public record PasskeyDisplay(
        string CredentialId,
        string Name,
        string AuthenticatorName,
        string? IconUrl,
        DateTime CreatedAt);
}