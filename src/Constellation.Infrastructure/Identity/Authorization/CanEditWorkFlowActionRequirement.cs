namespace Constellation.Infrastructure.Identity.Authorization;

using Application.Models.Auth;
using Core.Models.StaffMembers.Identifiers;
using Core.Models.WorkFlow.Identifiers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Persistence.ConstellationContext;
using System;
using System.Linq;
using System.Threading.Tasks;
using Action = Core.Models.WorkFlow.Action;

public sealed class CanEditWorkFlowActionRequirement : IAuthorizationRequirement  { }

public sealed class IsAssignedToActionByRoute : AuthorizationHandler<CanEditWorkFlowActionRequirement>
{
    private readonly AppDbContext _context;

    public IsAssignedToActionByRoute(
        AppDbContext context)
    {
        _context = context;
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, CanEditWorkFlowActionRequirement requirement)
    {
        HttpContext httpContext = context.Resource switch
        {
            AuthorizationFilterContext mvcContext => mvcContext.HttpContext,
            HttpContext razorPageContext => razorPageContext,
            _ => null
        };

        if (httpContext is null)
            return;

        string recordId = httpContext.Request.Path.ToString().Split('/').Last();

        if (string.IsNullOrWhiteSpace(recordId))
            return;

        string userStaffId = context.User
            .Claims
            .FirstOrDefault(claim => 
                claim.Type == AuthClaimType.StaffEmployeeId)
            ?.Value;

        if (userStaffId is null)
            return;

        StaffId staffId = StaffId.FromValue(Guid.Parse(userStaffId));

        ActionId actionId = ActionId.FromValue(Guid.Parse(recordId));
        
        bool assignedToUser = await _context
            .Set<Action>()
            .AnyAsync(record =>
                record.Id == actionId &&
                record.AssignedToId == staffId);

        if (assignedToUser)
            context.Succeed(requirement);
    }
}

public sealed class IsAssignedToActionByResource : AuthorizationHandler<CanEditWorkFlowActionRequirement, Guid>
{
    private readonly AppDbContext _context;
    private readonly ILogger _logger;

    public IsAssignedToActionByResource(
        AppDbContext _context,
        ILogger logger)
    {
        this._context = _context;
        _logger = logger.ForContext<CanEditWorkFlowActionRequirement>();
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, CanEditWorkFlowActionRequirement requirement, Guid resource)
    {
        string userStaffId = context.User.Claims.FirstOrDefault(claim => claim.Type == AuthClaimType.StaffEmployeeId)?.Value;

        _logger.ForContext(nameof(userStaffId), userStaffId)
            .Information("Detected logged in user with ID");

        if (userStaffId is null) return;

        StaffId staffId = StaffId.FromValue(Guid.Parse(userStaffId));
        
        ActionId actionId = ActionId.FromValue(resource);

        _logger.ForContext(nameof(ActionId), actionId, true)
            .Information("Determined Action Id");

        bool assignedToUser = await _context
            .Set<Action>()
            .AnyAsync(record =>
                record.Id == actionId &&
                record.AssignedToId == staffId);

        if (assignedToUser) 
            context.Succeed(requirement);
    }
}

public sealed class IsInGroupAllowedToEditWorkFlows : AuthorizationHandler<CanEditWorkFlowActionRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, CanEditWorkFlowActionRequirement requirement)
    {
        bool execStaff = context.User.IsInRole(AuthRoles.ExecStaffMember);
        bool adminStaff = context.User.IsInRole(AuthRoles.Admin);

        if (execStaff || adminStaff)
            context.Succeed(requirement);

        return Task.CompletedTask;
    }
}
