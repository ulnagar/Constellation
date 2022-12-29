namespace Constellation.Application.AdminDashboards.AddUserToRole;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Models.Identity;
using Constellation.Core.Errors;
using Constellation.Core.Shared;
using Microsoft.AspNetCore.Identity;
using System.Threading;
using System.Threading.Tasks;

internal sealed class AddUserToRoleCommandHandler
    : ICommandHandler<AddUserToRoleCommand>
{
    private readonly UserManager<AppUser> _userManager;
    private readonly RoleManager<AppRole> _roleManager;

    public AddUserToRoleCommandHandler(
        UserManager<AppUser> userManager,
        RoleManager<AppRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task<Result> Handle(AddUserToRoleCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.UserId.ToString());

        if (user is null)
        {
            return Result.Failure(DomainErrors.Auth.UserNotFound);
        }

        var role = await _roleManager.FindByIdAsync(request.RoleId.ToString());

        if (role is null)
        {
            return Result.Failure(DomainErrors.Auth.RoleNotFound);
        }

        var result = await _userManager.AddToRoleAsync(user, role.Name);

        if (!result.Succeeded)
        {
            return Result.Failure(DomainErrors.Auth.CannotUpdateRole(role.Name));
        }
        else
        {
            return Result.Success();
        }
    }
}
