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
    private readonly ConstellationDbContext _context;

    public IsCurrentTeacherAddedToTutorial(ConstellationDbContext context)
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

        if (teachers.Select(teacher => teacher.StaffId.ToString()).Contains(userStaffId))
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
        IEnumerable<string> userPermissions = context.User.Claims
            .Where(c => c.Type == AuthClaimType.Permission)
            .Select(c => c.Value);

        if (userPermissions.Contains(AuthPermission.Subjects_GroupTutorials_Edit))
            context.Succeed(requirement);

        return Task.CompletedTask;
    }
}

