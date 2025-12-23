namespace Constellation.Presentation.Shared.Helpers.Attributes;

using Application.Models.Auth;
using Microsoft.AspNetCore.Authorization;

public sealed class HasPermissionAttribute : AuthorizeAttribute
{
    public HasPermissionAttribute(params AuthPermission[] permissions)
        : base(policy: string.Join(",", permissions.Select(p => p.Value)))
    { }
}