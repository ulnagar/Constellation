namespace Constellation.Application.Domains.Auth.Queries.GetRoleDetails;

using Application.Models.Auth;
using Application.Models.Identity.Enums;
using Core.Models.Auth;
using Core.ValueObjects;
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
