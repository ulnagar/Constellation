namespace Constellation.Application.Domains.Auth.Queries.GetRoleDetails;

using Core.ValueObjects;
using Models.Auth;
using Models.Identity;
using Models.Identity.Enums;
using System;
using System.Collections.Generic;

public sealed record RoleDetailResponse(
    Guid Id,
    string Name,
    AppRoleType Type,
    List<AuthPermission> Permissions,
    List<RoleDetailResponse.UserResponse> Users)
{
    public sealed record UserResponse(
        Guid Id,
        Name UserName,
        string Email,
        List<AppUserLink> Links);
}
