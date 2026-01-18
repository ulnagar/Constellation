namespace Constellation.Application.Domains.Auth.Queries.GetRoleDetails;

using Abstractions.Messaging;
using Constellation.Application.Models.Identity.Repositories;
using Core.Abstractions.Services;
using Core.Shared;
using Models.Auth;
using Models.Identity;
using Models.Identity.Errors;
using Serilog;

internal sealed class GetRoleDetailsQueryHandler
    : IQueryHandler<GetRoleDetailsQuery, RoleDetailResponse>
{
    private readonly IIdentityRepository _identityRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public GetRoleDetailsQueryHandler(
        IIdentityRepository identityRepository,
        ICurrentUserService currentUserService,
        ILogger logger)
    {
        _identityRepository = identityRepository;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<Result<RoleDetailResponse>> Handle(GetRoleDetailsQuery request, CancellationToken cancellationToken)
    {
        AppRole? role = await _identityRepository.GetRole(request.RoleId, cancellationToken);

        if (role is null)
        {
            _logger
                .ForContext(nameof(GetRoleDetailsQuery), request, true)
                .ForContext(nameof(Error), AuthErrors.RoleNotFound(request.RoleId), true)
                .Warning("Failed to retrieve Role details by user {User}", _currentUserService.UserName);

            return Result.Failure<RoleDetailResponse>(AuthErrors.RoleNotFound(request.RoleId));
        }

        List<AppUser> users = await _identityRepository.UsersInRole(role.Name, cancellationToken);

        List<RoleDetailResponse.UserResponse> userResponses = [];

        foreach (AppUser user in users)
        {
            userResponses.Add(new(
                user.Id, 
                user.Name, 
                user.Email, 
                user.Links.Where(link => !link.IsDeleted).ToList()));
        }

        List<AuthPermission> permissions = await _identityRepository.GetRolePermissions(role.Id, cancellationToken);

        return new RoleDetailResponse(
            role.Id,
            role.Name,
            role.Type,
            permissions,
            userResponses);
    }
}
