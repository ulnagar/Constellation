namespace Constellation.Application.Models.Identity.Errors;

using Auth;
using Core.Shared;
using System;

public static class AuthErrors
{
    public static Func<Guid, Error> RoleNotFound = roleId => new(
        "Auth.Role.NotFound",
        $"The role with ID '{roleId}' was not found.");

    public static Func<string, AuthPermission, Error> PermissionAlreadyAdded = (roleName, permission) => new(
        "Auth.Role.PermissionAlreadyAdded",
        $"The Role {roleName} has already been granted the permission {permission.Name}");

    public static Func<string, AuthPermission, Error> PermissionNotFoundInRole = (roleName, permission) => new(
        "Auth.Role.PermissionNotFoundInRole",
        $"The Role {roleName} does not include the permission {permission.Name}");
}
