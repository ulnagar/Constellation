namespace Constellation.Application.Domains.Auth.Queries.GetUserDetails;

using Abstractions.Messaging;
using Core.Shared;
using Models.Auth;
using Models.Identity;
using Models.Identity.Errors;
using Models.Identity.Repositories;
using Serilog;
using System.Security.Claims;
using System.Threading.Tasks;

internal sealed class GetUserDetailsQueryHandler
: IQueryHandler<GetUserDetailsQuery, UserResponse>
{
    private readonly IIdentityRepository _identityRepository;
    private readonly ILogger _logger;

    public GetUserDetailsQueryHandler(
        IIdentityRepository identityRepository,
        ILogger logger)
    {
        _identityRepository = identityRepository;
        _logger = logger
            .ForContext<GetUserDetailsQuery>();
    }

    public async Task<Result<UserResponse>> Handle(GetUserDetailsQuery request, CancellationToken cancellationToken)
    {
        AppUser? user = await _identityRepository.GetUser(request.Id, cancellationToken);

        if (user is null)
        {
            _logger
                .ForContext(nameof(GetUserDetailsQuery), request, true)
                .ForContext(nameof(Error), AuthErrors.UserNotFound(request.Id), true)
                .Warning("Failed to retrieve User details");

            return Result.Failure<UserResponse>(AuthErrors.UserNotFound(request.Id));
        }

        List<UserResponse.UserClaim> userClaims = [];

        List<AppRole> roles = await _identityRepository.GetRolesForUser(user, cancellationToken);

        foreach (AppRole role in roles)
        {
            List<AuthPermission> permissions = await _identityRepository.GetRolePermissions(role.Id, cancellationToken);
            
            foreach (AuthPermission permission in permissions)
            {
                userClaims.Add(new(role.Name, AuthClaimType.Permission, permission));
            }
        }

        List<Claim> claims = await _identityRepository.GetClaims(user, cancellationToken);

        foreach (Claim claim in claims)
        {
            userClaims.Add(new (string.Empty, claim.Type, claim.Value));
        }

        UserResponse response = new(
            user.Id,
            user.Name,
            user.Email,
            user.Logins.ToList(),
            user.Links.Where(link => !link.IsDeleted).ToList(),
            roles,
            userClaims);

        return response;
    }
}
