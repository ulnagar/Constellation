namespace Constellation.Core.Models.Stocktake.Enums;

using Common;
using System.Collections.Generic;

public sealed class LocationCategory : StringEnumeration<LocationCategory>
{
    public static readonly LocationCategory AuroraCollege = new("Aurora College");
    public static readonly LocationCategory PublicSchool = new("Public School");
    public static readonly LocationCategory StateOffice = new("State Office");
    public static readonly LocationCategory PrivateResidence = new("Private Residence");
    public static readonly LocationCategory Other = new("Other");

    private LocationCategory(string value)
        : base(value, value) { }

    public static IEnumerable<LocationCategory> GetOptions => GetEnumerable;
}