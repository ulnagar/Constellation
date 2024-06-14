namespace Constellation.Core.Models.Assets.Enums;

using Common;
using System.Collections.Generic;

public sealed class LocationCategory : StringEnumeration<LocationCategory>
{
    public static readonly LocationCategory CoordinatingOffice = new("CoordinatingOffice", "Coordinating Office");
    public static readonly LocationCategory PublicSchool = new("PublicSchool", "Public School");
    public static readonly LocationCategory PrivateResidence = new("PrivateResidence", "Private Residence");
    public static readonly LocationCategory CorporateOffice = new("CorporateOffice", "Corporate Office");

    private LocationCategory(string value, string name)
        : base(value, name) { }

    public static IEnumerable<LocationCategory> GetOptions => GetEnumerable;
}