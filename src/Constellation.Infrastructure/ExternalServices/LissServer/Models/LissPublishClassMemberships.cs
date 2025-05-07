namespace Constellation.Infrastructure.ExternalServices.LissServer.Models;

using Core.Models.Edval;
using System.Text.Json.Serialization;

public sealed class LissPublishClassMemberships
{
    [JsonPropertyName("ClassCode")]
    public string OfferingName { get; set; }
    public string EdvalClassCode { get; set; }
    public string StudentId { get; set; }

    public EdvalClassMembership ToClassMembership()
    {
        return new()
        {
            EdvalClassCode = EdvalClassCode,
            StudentId = StudentId,
            OfferingName = OfferingName
        };
    }
}