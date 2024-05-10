namespace Constellation.Core.Models.Assets;

using Enums;
using Identifiers;
using Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ValueObjects;

public sealed class Asset : IAuditableEntity
{
    private readonly List<Allocation> _allocations = new();

    private Asset()
    {
        
    }

    public AssetId Id { get; private set; } = new();
    public AssetNumber AssetNumber { get; private set; }
    public string SerialNumber { get; private set; }
    public string SAPEquipmentNumber { get; private set; }
    public string ModelNumber { get; set; }
    public string ModelDescription { get; set; }
    public AssetStatus Status { get; set; }
    public AssetCategory Category { get; set; }
    public DateOnly PurchaseDate { get; set; }
    public string PurchaseDocument { get; set; }
    public decimal PurchaseCost { get; set; }
    public DateOnly WarrantyEndDate { get; set; }
    public DateOnly LastSightedAt { get; set; }
    public string LastSightedBy { get; set; }
    public IReadOnlyList<Allocation> Allocations => _allocations.AsReadOnly();

    public string CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public string ModifiedBy { get; set; }
    public DateTime ModifiedAt { get; set; }
    public bool IsDeleted { get; private set; }
    public string DeletedBy { get; set; }
    public DateTime DeletedAt { get; set; }
}
