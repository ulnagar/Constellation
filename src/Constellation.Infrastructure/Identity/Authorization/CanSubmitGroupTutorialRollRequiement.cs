namespace Constellation.Infrastructure.Identity.Authorization;

using Constellation.Application.Models.Auth;
using Constellation.Core.Models.GroupTutorials;
using Constellation.Core.Models.Identifiers;
using Constellation.Infrastructure.Persistence.ConstellationContext;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

public sealed record CanSubmitGroupTutorialRollRequirement : IAuthorizationRequirement
{
}

public sealed class IsCurrentTeacherAddedToTutorial : AuthorizationHandler<CanSubmitGroupTutorialRollRequirement, Guid>
{
    private readonly AppDbContext _context;

    public IsCurrentTeacherAddedToTutorial(AppDbContext context)
    {
        _context = context;
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, CanSubmitGroupTutorialRollRequirement requirement, Guid resource)
    {
        var userStaffId = context.User.Claims.FirstOrDefault(claim => claim.Type == AuthClaimType.StaffEmployeeId)?.Value;

        if (userStaffId is null)
        {
            return;
        }

        var tutorialId = GroupTutorialId.FromValue(resource);

        var teachers = await _context
            .Set<GroupTutorial>()
            .Where(tutorial => tutorial.Id == tutorialId)
            .SelectMany(tutorial => tutorial.Teachers.Where(teacher => !teacher.IsDeleted))
            .ToListAsync();

        if (teachers.Select(teacher => teacher.StaffId).Contains(userStaffId))
        {
            context.Succeed(requirement);
        }

        return;
    }
}

public sealed class HasRequiredGroupTutorialModulePermissions : AuthorizationHandler<CanSubmitGroupTutorialRollRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, CanSubmitGroupTutorialRollRequirement requirement)
    {
        if (context.User.HasClaim(claim => claim.Type == AuthClaimType.Permission && claim.Value == AuthPermissions.GroupTutorialsEdit))
            context.Succeed(requirement);

        return Task.CompletedTask;
    }
}
