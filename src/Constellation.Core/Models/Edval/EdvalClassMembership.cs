namespace Constellation.Core.Models.Edval;

using System.Text.Json.Serialization;

public sealed class EdvalClassMembership
{
    [JsonPropertyName("ClassCode")]
    public string OfferingName { get; set; }
    public string EdvalClassCode { get; set; }
    public string StudentId { get; set; }
}