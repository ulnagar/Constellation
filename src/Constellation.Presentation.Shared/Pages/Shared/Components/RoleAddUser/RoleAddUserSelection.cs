namespace Constellation.Presentation.Shared.Pages.Shared.Components.RoleAddUser;

public class RoleAddUserSelection
{
    public string RoleName { get; set; }
    public Guid UserId { get; set; }
    public List<UserDto> UserList { get; set; } = [];

    public class UserDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public bool Available { get; set; }
    }
}
