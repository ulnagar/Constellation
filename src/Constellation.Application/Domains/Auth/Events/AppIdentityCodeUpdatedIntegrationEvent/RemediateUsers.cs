namespace Constellation.Application.Domains.Auth.Events.AppIdentityCodeUpdatedIntegrationEvent;

using Abstractions.Messaging;
using Commands.AuditAllUsers;
using Constellation.Application.Models.Auth;
using Constellation.Application.Models.Identity.Enums;
using Constellation.Core.Models.SchoolContacts.Enums;
using Core.IntegrationEvents;
using Core.ValueObjects;
using Interfaces.Configuration;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Models.Identity;
using System.Security.Claims;
using System.Threading.Tasks;

internal sealed class RemediateUsers
: IIntegrationEventHandler<AppIdentityCodeUpdatedIntegrationEvent>
{
    private readonly ISender _mediator;
    private readonly UserManager<AppUser> _userManager;
    private readonly RoleManager<AppRole> _roleManager;
    private readonly AppConfiguration _configuration;

    public RemediateUsers(
        ISender mediator,
        UserManager<AppUser> userManager,
        RoleManager<AppRole> roleManager,
        IOptions<AppConfiguration> configuration)
    {
        _mediator = mediator;
        _userManager = userManager;
        _roleManager = roleManager;
        _configuration = configuration.Value;
    }

    public async Task Handle(AppIdentityCodeUpdatedIntegrationEvent notification, CancellationToken cancellationToken)
    {
        // Delete all users
        foreach (var user in _userManager.Users)
            await _userManager.DeleteAsync(user);

        // Delete all roles
        foreach (var role in _roleManager.Roles)
            await _roleManager.DeleteAsync(role);

        // Recreate roles
        List<AuthPermission> permissions = AuthPermission.GetOptions.ToList();

        await CreateRoleWithPermission(AppRole.SuperAdminRole, permissions, AppRoleType.Staff);

        IEnumerable<Position> positions = Position.GetOptions;

        foreach (var position in positions)
            await CreateRole(position.Value, AppRoleType.Contact);

        await CreateRole(AppRole.Parent, AppRoleType.Parent);
        await CreateRole(AppRole.Student, AppRoleType.Student);
        await CreateRole(AppRole.Staff, AppRoleType.Staff);

        // Recreate users
        await _mediator.Send(new AuditAllUsersCommand(), cancellationToken);

        // Ensure that the master admin user has permissions
        string adminEmail = _configuration.AdminUser;

        AppUser? adminUser = await _userManager.FindByEmailAsync(adminEmail);
        if (adminUser is not null)
        {
            bool isInRole = await _userManager.IsInRoleAsync(adminUser, AppRole.SuperAdminRole);
            if (!isInRole)
                await _userManager.AddToRoleAsync(adminUser, AppRole.SuperAdminRole);
        }
    }

    private async Task<AppRole?> CreateRole(string roleName, AppRoleType type)
    {
        AppRole? role = await _roleManager.FindByNameAsync(roleName);

        if (role is null)
        {
            IdentityResult create = await _roleManager.CreateAsync(new AppRole(roleName, type));

            if (create.Succeeded)
                role = await _roleManager.FindByNameAsync(roleName);
            else
                return null;
        }

        return role;
    }

    private async Task CreateRoleWithPermission(string roleName, List<AuthPermission> permissions, AppRoleType type)
    {
        AppRole? role = await CreateRole(roleName, type);

        if (role is null)
            return;

        IList<Claim> claims = await _roleManager.GetClaimsAsync(role!);

        List<Claim> permissionClaims = claims
            .Where(claim => claim.Type == AuthClaimType.Permission)
            .ToList();

        List<string> permissionValues = permissions.Select(entry => entry.Value).ToList();

        foreach (Claim claim in permissionClaims)
        {
            if (permissionValues.Contains(claim.Value))
                continue;

            await _roleManager.RemoveClaimAsync(role!, claim);
        }

        foreach (AuthPermission permission in permissions)
        {
            if (permissionClaims.All(claim => claim.Value != permission))
                await _roleManager.AddClaimAsync(role!, new Claim(AuthClaimType.Permission, permission));
        }
    }
}
