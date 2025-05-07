namespace Constellation.Core.Models.Edval;

using System;

public sealed class EdvalClass
{
    public string ClassCode { get; set; }
    public string EdvalClassCode { get; set; }
    public string Form { get; set; }

    public string OfferingName => ClassCode.Replace(" ", "", StringComparison.InvariantCultureIgnoreCase).PadLeft(7, '0');

}