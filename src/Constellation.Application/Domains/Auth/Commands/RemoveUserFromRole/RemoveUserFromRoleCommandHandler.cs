namespace Constellation.Application.Domains.Auth.Commands.RemoveUserFromRole;

using Abstractions.Messaging;
using Application.Models.Identity;
using Application.Models.Identity.Errors;
using Core.Models.Auth;
using Core.Shared;
using Microsoft.AspNetCore.Identity;
using System.Threading;
using System.Threading.Tasks;

internal sealed class RemoveUserFromRoleCommandHandler
    : ICommandHandler<RemoveUserFromRoleCommand>
{
    private readonly UserManager<AppUser> _userManager;
    private readonly RoleManager<AppRole> _roleManager;

    public RemoveUserFromRoleCommandHandler(
        UserManager<AppUser> userManager,
        RoleManager<AppRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task<Result> Handle(RemoveUserFromRoleCommand request, CancellationToken cancellationToken)
    {
        AppUser? user = await _userManager.FindByIdAsync(request.UserId.ToString());

        if (user is null)
        {
            return Result.Failure(AuthErrors.UserNotFound(request.UserId));
        }

        AppRole? role = await _roleManager.FindByIdAsync(request.RoleId.ToString());

        if (role is null)
        {
            return Result.Failure(AuthErrors.RoleNotFound(request.RoleId));
        }

        IdentityResult result = await _userManager.RemoveFromRoleAsync(user, role.Name);

        if (!result.Succeeded)
        {
            return Result.Failure(AuthErrors.CannotUpdateRole(role.Name));
        }
        else
        {
            return Result.Success();
        }
    }
}
