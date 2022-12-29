namespace Constellation.Presentation.Server.Pages.Shared.Components.RoleAddUser;

public class RoleAddUserSelection
{
    public Guid RoleId { get; set; }
    public string RoleName { get; set; }

    public Guid UserId { get; set; }
    public List<UserDto> UserList { get; set; } = new();

    public class UserDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
    }
}
