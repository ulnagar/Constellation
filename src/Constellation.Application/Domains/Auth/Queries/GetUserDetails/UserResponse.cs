namespace Constellation.Application.Domains.Auth.Queries.GetUserDetails;

using Core.Models.Auth;
using Core.ValueObjects;
using Models.Identity;
using System;
using System.Collections.Generic;

public sealed record UserResponse(
    Guid Id,
    Name Name,
    string Email,
    List<AppUserLoginAttempt> Logins,
    List<AppUserLink> Links,
    List<AppRole> Roles,
    List<UserResponse.UserClaim> Claims,
    List<UserResponse.Passkey> Passkeys)
{
    public sealed record UserClaim(
        string RoleName,
        string Type,
        string Value);

    public sealed record Passkey(
        string Name,
        DateTimeOffset CreatedAt,
        byte[] CredentialId);
}