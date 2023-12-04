namespace Constellation.Infrastructure.Identity.Authorization;

using Constellation.Application.Models.Auth;
using Core.Models.Training.Contexts.Modules;
using Core.Models.Training.Identifiers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Persistence.ConstellationContext;
using System;
using System.Linq;
using System.Threading.Tasks;

public class CanViewTrainingCompletionRecordRequirement : IAuthorizationRequirement
{
}

public class OwnsTrainingCompletionRecordByRoute : AuthorizationHandler<CanViewTrainingCompletionRecordRequirement>
{
    private readonly AppDbContext _context;

    public OwnsTrainingCompletionRecordByRoute(AppDbContext context)
    {
        _context = context;
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, CanViewTrainingCompletionRecordRequirement requirement)
    {
        HttpContext httpContext = context.Resource switch
        {
            AuthorizationFilterContext mvcContext => mvcContext.HttpContext,
            HttpContext razorPageContext => razorPageContext,
            _ => null
        };

        if (httpContext is null)
        {
            return;
        }

        string recordId = httpContext.Request.Path.ToString().Split('/').Last();

        string userStaffId = context.User.Claims.FirstOrDefault(claim => claim.Type == AuthClaimType.StaffEmployeeId)?.Value;

        if (!string.IsNullOrWhiteSpace(recordId) && userStaffId is not null)
        {
            TrainingCompletionId completionId = TrainingCompletionId.FromValue(Guid.Parse(recordId));

            bool userOwns = await _context
                .Set<TrainingCompletion>()
                .AnyAsync(record => 
                    record.Id == completionId && 
                    record.StaffId == userStaffId);

            if (userOwns)
                context.Succeed(requirement);
        }

        return;
    }
}

public class OwnsTrainingCompletionRecordByResource : AuthorizationHandler<CanViewTrainingCompletionRecordRequirement, Guid>
{
    private readonly AppDbContext _context;

    public OwnsTrainingCompletionRecordByResource(AppDbContext context)
    {
        _context = context;
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, CanViewTrainingCompletionRecordRequirement requirement, Guid resource)
    {
        string userStaffId = context.User.Claims.FirstOrDefault(claim => claim.Type == AuthClaimType.StaffEmployeeId)?.Value;

        if (userStaffId is not null)
        {
            TrainingCompletionId completionId = TrainingCompletionId.FromValue(resource);

            bool userOwns = await _context
                .Set<TrainingCompletion>()
                .AnyAsync(record => 
                    record.Id == completionId && 
                    record.StaffId == userStaffId);

            if (userOwns)
                context.Succeed(requirement);
        }

        return;
    }
}

public class HasRequiredMandatoryTrainingModulePermissions : AuthorizationHandler<CanViewTrainingCompletionRecordRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, CanViewTrainingCompletionRecordRequirement requirement)
    {
        if (context.User.HasClaim(claim => claim.Type == AuthClaimType.Permission && claim.Value == AuthPermissions.MandatoryTrainingDetailsView))
            context.Succeed(requirement);

        return Task.CompletedTask;
    }
}
