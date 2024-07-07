namespace Constellation.Infrastructure.Identity.Authorization;

using Core.Abstractions.Repositories;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

public sealed record IsParentOfCurrentStudentRequirement : IAuthorizationRequirement
{

}

public sealed class HasActiveParentRecord : AuthorizationHandler<IsParentOfCurrentStudentRequirement>
{
    private readonly IFamilyRepository _familyRepository;

    public HasActiveParentRecord(
        IFamilyRepository familyRepository)
    {
        _familyRepository = familyRepository;
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, IsParentOfCurrentStudentRequirement requirement)
    {
        Claim emailClaim = context.User.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.Email);

        if (emailClaim is null)
            return;

        bool isParent = await _familyRepository.DoesEmailBelongToParentOrFamily(emailClaim.Value);

        if (isParent)
            context.Succeed(requirement);
    }
}