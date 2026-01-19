namespace Constellation.Presentation.Shared.Pages.Shared.Components.RoleAddUPermission;

using Application.Models.Auth;

public class RoleAddPermissionSelection
{
    public AuthPermission Permission { get; set; }
    public List<PermissionDto> Permissions { get; set; } = new();

    public sealed record PermissionDto(
        AuthPermission Permission,
        bool Available);
}