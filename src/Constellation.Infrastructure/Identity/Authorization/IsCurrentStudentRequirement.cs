namespace Constellation.Infrastructure.Identity.Authorization;

using Constellation.Core.Models.Students;
using Constellation.Core.Models.Students.Repositories;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

public sealed record IsCurrentStudentRequirement : IAuthorizationRequirement;

public sealed class IsActiveStudent : AuthorizationHandler<IsCurrentStudentRequirement>
{
    private readonly IStudentRepository _studentRepository;

    public IsActiveStudent(
        IStudentRepository studentRepository)
    {
        _studentRepository = studentRepository;
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, IsCurrentStudentRequirement requirement)
    {
        Claim emailClaim = context.User.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.Email);

        if (emailClaim is null)
            return;

        Student? isStudent = await _studentRepository.GetCurrentByEmailAddress(emailClaim.Value);

        if (isStudent is not null)
            context.Succeed(requirement);
    }
}