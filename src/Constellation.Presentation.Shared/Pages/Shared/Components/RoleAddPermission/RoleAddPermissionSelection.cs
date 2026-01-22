namespace Constellation.Presentation.Shared.Pages.Shared.Components.RoleAddPermission;

using Application.Models.Auth;
using Helpers.ModelBinders;
using Microsoft.AspNetCore.Mvc;

public class RoleAddPermissionSelection
{
    [ModelBinder(typeof(BaseFromValueBinder))]
    public AuthPermission Permission { get; set; }
    public List<PermissionDto> Permissions { get; set; } = new();

    public sealed record PermissionDto(
        AuthPermission Permission,
        bool Available);
}