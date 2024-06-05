﻿namespace Constellation.Core.Models.Assets.Enums;

using Common;

public sealed class AssetStatus : StringEnumeration<AssetStatus>
{
    public static readonly AssetStatus Active = new("Active");
    public static readonly AssetStatus PendingDisposal = new("Pending Disposal");
    public static readonly AssetStatus Disposed = new("Disposed");
    public static readonly AssetStatus Lost = new("Lost");
    public static readonly AssetStatus Stolen = new("Stolen");

    private AssetStatus(string value)
        : base(value, value) { }
}