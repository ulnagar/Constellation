namespace Constellation.Application.Domains.Auth.Commands.RemovePermissionFromRole;

using Abstractions.Messaging;
using Constellation.Application.Models.Auth;
using Constellation.Application.Models.Identity;
using Constellation.Application.Models.Identity.Errors;
using Constellation.Application.Models.Identity.Repositories;
using Constellation.Core.Errors;
using Core.Shared;
using Microsoft.AspNetCore.Identity;
using Serilog;
using System.Threading.Tasks;

internal sealed class RemovePermissionFromRoleCommandHandler
: ICommandHandler<RemovePermissionFromRoleCommand>
{
    private readonly IIdentityRepository _identityRepository;
    private readonly ILogger _logger;

    public RemovePermissionFromRoleCommandHandler(
        IIdentityRepository identityRepository,
        ILogger logger)
    {
        _identityRepository = identityRepository;
        _logger = logger
            .ForContext<RemovePermissionFromRoleCommand>();
    }

    public async Task<Result> Handle(RemovePermissionFromRoleCommand request, CancellationToken cancellationToken)
    {
        AppRole? role = await _identityRepository.GetRole(request.RoleId, cancellationToken);

        if (role is null)
        {
            _logger
                .ForContext(nameof(RemovePermissionFromRoleCommand), request, true)
                .ForContext(nameof(Error), AuthErrors.RoleNotFound(request.RoleId), true)
                .Warning("Failed to remove Permission from Role");

            return Result.Failure(AuthErrors.RoleNotFound(request.RoleId));
        }

        List<AuthPermission> existingPermissions = await _identityRepository.GetRolePermissions(request.RoleId, cancellationToken);

        if (!existingPermissions.Contains(request.Permission))
        {
            _logger
                .ForContext(nameof(RemovePermissionFromRoleCommand), request, true)
                .ForContext(nameof(Error), AuthErrors.PermissionAlreadyAdded(role.Name, request.Permission), true)
                .Warning("Failed to remove Permission from Role");

            return Result.Failure(AuthErrors.PermissionNotFoundInRole(role.Name, request.Permission));
        }

        IdentityResult result = await _identityRepository.RemovePermissionFromRole(role, request.Permission, cancellationToken);

        if (!result.Succeeded)
        {
            _logger
                .ForContext(nameof(RemovePermissionFromRoleCommand), request, true)
                .ForContext(nameof(IdentityError), result.Errors, true)
                .Warning("Failed to remove Permission from Role");

            return Result.Failure(ApplicationErrors.UnknownError);
        }

        return Result.Success();
    }
}
