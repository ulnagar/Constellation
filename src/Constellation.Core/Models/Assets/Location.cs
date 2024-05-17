namespace Constellation.Core.Models.Assets;

using Constellation.Core.Models.Assets.Enums;
using Constellation.Core.Models.Assets.Identifiers;
using Primitives;
using System;

public sealed record Location : IAuditableEntity
{
    private Location() { }

    public LocationId Id { get; private set; } = new();
    public AssetId AssetId { get; private set; }
    public LocationCategory Category { get; private set; }
    public string Site { get; private set; }
    public string SchoolCode { get; private set; }
    public string Room { get; private set; }
    public bool CurrentLocation { get; private set; }
    
    public DateOnly ArrivalDate { get; private set; }
    public DateOnly DepartureDate { get; private set; }
    
    public string CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public string ModifiedBy { get; set; }
    public DateTime ModifiedAt { get; set; }
    public bool IsDeleted { get; private set; }
    public string DeletedBy { get; set; }
    public DateTime DeletedAt { get; set; }
}
