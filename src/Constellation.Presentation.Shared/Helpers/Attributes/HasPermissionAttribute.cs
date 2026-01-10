namespace Constellation.Presentation.Shared.Helpers.Attributes;

using Microsoft.AspNetCore.Authorization;

public sealed class HasPermissionAttribute : AuthorizeAttribute
{
    public HasPermissionAttribute(params string[] Permissions)
        : base(policy: string.Join(",", Permissions))
    { }
}