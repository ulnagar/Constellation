namespace Constellation.Core.Models;

public class Team
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Link { get; set; } = string.Empty;
    // Link format is https://teams.microsoft.com/l/team/{General Channel Id}/conversations?groupId={Group Id}&tenantId={Tenant Id}
}
