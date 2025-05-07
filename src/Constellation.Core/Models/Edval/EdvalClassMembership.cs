namespace Constellation.Core.Models.Edval;

using System;

public sealed class EdvalClassMembership
{
    public string ClassCode { get; set; }
    public string EdvalClassCode { get; set; }
    public string StudentId { get; set; }

    public string OfferingName => ClassCode.Replace(" ", "", StringComparison.InvariantCultureIgnoreCase).PadLeft(7, '0');
}