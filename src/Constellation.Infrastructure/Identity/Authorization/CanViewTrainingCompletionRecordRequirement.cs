namespace Constellation.Infrastructure.Identity.Authorization;

using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Models.Auth;
using Constellation.Core.Models.Identifiers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

public class CanViewTrainingCompletionRecordRequirement : IAuthorizationRequirement
{
}

public class OwnsTrainingCompletionRecordByRoute : AuthorizationHandler<CanViewTrainingCompletionRecordRequirement>
{
    private readonly IAppDbContext _context;

    public OwnsTrainingCompletionRecordByRoute(IAppDbContext context)
    {
        _context = context;
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, CanViewTrainingCompletionRecordRequirement requirement)
    {
        var httpContext = context.Resource switch
        {
            AuthorizationFilterContext mvcContext => mvcContext.HttpContext,
            HttpContext razorPageContext => razorPageContext,
            _ => null
        };

        if (httpContext is null)
        {
            return;
        }

        var recordId = httpContext.Request.Path.ToString().Split('/').Last();

        var userStaffId = context.User.Claims.FirstOrDefault(claim => claim.Type == AuthClaimType.StaffEmployeeId)?.Value;

        if (recordId is not null && userStaffId is not null)
        {
            var userOwns = await _context.MandatoryTraining.CompletionRecords
                .AnyAsync(record => record.Id == TrainingCompletionId.FromValue(Guid.Parse(recordId)) && record.StaffId == userStaffId);

            if (userOwns)
                context.Succeed(requirement);
        }

        return;
    }
}

public class OwnsTrainingCompletionRecordByResource : AuthorizationHandler<CanViewTrainingCompletionRecordRequirement, Guid>
{
    private readonly IAppDbContext _context;

    public OwnsTrainingCompletionRecordByResource(IAppDbContext context)
    {
        _context = context;
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, CanViewTrainingCompletionRecordRequirement requirement, Guid resource)
    {
        var userStaffId = context.User.Claims.FirstOrDefault(claim => claim.Type == AuthClaimType.StaffEmployeeId)?.Value;

        if (userStaffId is not null)
        {
            var userOwns = await _context.MandatoryTraining.CompletionRecords
                .AnyAsync(record => record.Id == TrainingCompletionId.FromValue(resource) && record.StaffId == userStaffId);

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
