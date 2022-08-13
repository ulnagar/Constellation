namespace Constellation.Core.Refactor.Models;

using Constellation.Core.Refactor.Common;
using Constellation.Core.Refactor.ValueObjects;
using System;

public class MSTeam : BaseAuditableEntity
{
    public Guid GroupId { get; set; }

    public string Name { get; set; }
    public MSTeamType TeamType { get; set; }

    // Link format is https://teams.microsoft.com/l/team/{General Channel Id}/conversations?groupId={Group Id}&tenantId={Tenant Id}
    public string Link { get; set; }

}
