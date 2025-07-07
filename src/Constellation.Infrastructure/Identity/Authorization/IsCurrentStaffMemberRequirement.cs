namespace Constellation.Infrastructure.Identity.Authorization;

using Core.Models;
using Core.Models.StaffMembers;
using Core.Models.StaffMembers.Repositories;
using Core.Shared;
using Core.ValueObjects;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

public sealed record IsCurrentStaffMemberRequirement : IAuthorizationRequirement;

public sealed class HasActiveStaffRecord : AuthorizationHandler<IsCurrentStaffMemberRequirement>
{
    private readonly IStaffRepository _staffRepository;

    public HasActiveStaffRecord(
        IStaffRepository staffRepository)
    {
        _staffRepository = staffRepository;
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, IsCurrentStaffMemberRequirement requirement)
    {
        Claim emailClaim = context.User.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.Email);

        if (emailClaim is null)
            return;

        Result<EmailAddress> emailAddress = EmailAddress.Create(emailClaim.Value);

        StaffMember? isStaffMember = emailAddress.IsSuccess 
            ? await _staffRepository.GetCurrentByEmailAddress(emailAddress.Value)
            : null;

        if (isStaffMember is not null)
            context.Succeed(requirement);
    }
}