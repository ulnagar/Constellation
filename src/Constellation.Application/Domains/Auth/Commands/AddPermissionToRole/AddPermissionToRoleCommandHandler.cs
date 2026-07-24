namespace Constellation.Application.Domains.Auth.Commands.AddPermissionToRole;

using Abstractions.Messaging;
using Core.Errors;
using Core.Shared;
using Interfaces.Services;
using Microsoft.AspNetCore.Identity;
using Models.Auth;
using Models.Identity;
using Models.Identity.Errors;
using Models.Identity.Repositories;
using Serilog;
using System.Threading.Tasks;

internal sealed class AddPermissionToRoleCommandHandler
: ICommandHandler<AddPermissionToRoleCommand>
{
    private readonly IIdentityRepository _identityRepository;
    private readonly IAuthService _authService;
    private readonly ILogger _logger;

    public AddPermissionToRoleCommandHandler(
        IIdentityRepository identityRepository,
        IAuthService authService,
        ILogger logger)
    {
        _identityRepository = identityRepository;
        _authService = authService;
        _logger = logger
            .ForContext<AddPermissionToRoleCommand>();
    }

    public async Task<Result> Handle(AddPermissionToRoleCommand request, CancellationToken cancellationToken)
    {
        AppRole? role = await _identityRepository.GetRole(request.RoleId, cancellationToken);

        if (role is null)
        {
            _logger
                .ForContext(nameof(AddPermissionToRoleCommand), request, true)
                .ForContext(nameof(Error), AuthErrors.RoleNotFound(request.RoleId), true)
                .Warning("Failed to add Permission to Role");

            return Result.Failure(AuthErrors.RoleNotFound(request.RoleId));
        }

        List<AuthPermission> existingPermissions = await _identityRepository.GetRolePermissions(request.RoleId, cancellationToken);

        if (existingPermissions.Contains(request.Permission))
        {
            _logger
                .ForContext(nameof(AddPermissionToRoleCommand), request, true)
                .ForContext(nameof(Error), AuthErrors.PermissionAlreadyAdded(role.Name, request.Permission), true)
                .Warning("Failed to add Permission to Role");

            return Result.Failure(AuthErrors.PermissionAlreadyAdded(role.Name, request.Permission));
        }

        IdentityResult result = await _identityRepository.AddPermissionToRole(role, request.Permission, cancellationToken);

        if (!result.Succeeded)
        {
            _logger
                .ForContext(nameof(AddPermissionToRoleCommand), request, true)
                .ForContext(nameof(IdentityError), result.Errors, true)
                .Warning("Failed to add Permission to Role");

            return Result.Failure(ApplicationErrors.UnknownError);
        }

        _authService.InvalidateRoleClaimsCache(role.Name);

        return Result.Success();
    }
}
