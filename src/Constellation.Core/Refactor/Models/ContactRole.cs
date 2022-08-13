namespace Constellation.Core.Refactor.Models;

using Constellation.Core.Refactor.Common;
using Constellation.Core.Refactor.ValueObjects;
using System;

public class ContactRole : BaseAuditableEntity
{
    public Guid ContactId { get; set; }
    public virtual Contact Contact { get; set; }

    public Guid SchoolId { get; set; }
    public virtual School School { get; set; }

    public PositionType Position { get; set; }
}
