namespace Constellation.Infrastructure.Identity.Authorization;

using Application.Models.Identity;
using Constellation.Core.Models.SchoolContacts;
using Constellation.Core.Shared;
using Core.Models.SchoolContacts.Repositories;
using Core.ValueObjects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

public sealed record CanAccessSchoolPortalRequirement : IAuthorizationRequirement;

public sealed class HasActiveContactAssignmentToCurrentPartnerSchool : AuthorizationHandler<CanAccessSchoolPortalRequirement>
{
    private readonly ISchoolContactRepository _contactRepository;

    public HasActiveContactAssignmentToCurrentPartnerSchool(
        ISchoolContactRepository contactRepository)
    {
        _contactRepository = contactRepository;
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, CanAccessSchoolPortalRequirement requirement)
    {
        Claim emailClaim = context.User.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.Email);

        if (emailClaim is null)
            return;

        Result<EmailAddress> emailAddress = EmailAddress.Create(emailClaim.Value);

        if (emailAddress.IsFailure)
            return;

        SchoolContact? response = await _contactRepository.GetWithRolesByEmailAddress(emailAddress.Value);

        if (response is null)
            return;

        if (response.Assignments.Any(entry => !entry.IsDeleted))
            context.Succeed(requirement);
    }
}

public sealed class HasAdminUserPrivileges : AuthorizationHandler<CanAccessSchoolPortalRequirement>
{
    private readonly UserManager<AppUser> _userManager;

    public HasAdminUserPrivileges(
        UserManager<AppUser> userManager)
    {
        _userManager = userManager;
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, CanAccessSchoolPortalRequirement requirement)
    {
        Claim? emailClaim = context.User.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.Email);

        if (emailClaim is null)
            return;

        AppUser? user = await _userManager.FindByEmailAsync(emailClaim.Value);

        if (user is null)
            return;

        bool isAdmin = await _userManager.IsInRoleAsync(user, AppRole.SuperAdminRole);

        if (isAdmin)
            context.Succeed(requirement);
    }
}