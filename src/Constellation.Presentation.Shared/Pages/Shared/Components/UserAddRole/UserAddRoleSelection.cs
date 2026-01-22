namespace Constellation.Presentation.Shared.Pages.Shared.Components.UserAddRole;

using Application.Models.Identity.Enums;
using Core.ValueObjects;

public class UserAddRoleSelection
{
    public Name Name { get; set; }
    public Guid RoleId { get; set; }
    public List<RoleDto> RoleList { get; set; } = [];

    public class RoleDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public AppRoleType Type { get; set; }
        public bool Available { get; set; }
    }
}
