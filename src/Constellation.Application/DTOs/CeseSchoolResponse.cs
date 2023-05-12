namespace Constellation.Application.DTOs;

using System.Text.Json.Serialization;

public sealed class CeseSchoolResponse
{
    [JsonPropertyName("School_code")] public string Code { get; set; }
    [JsonPropertyName("School_name")] public string Name { get; set; }
    [JsonPropertyName("Street")] public string Address { get; set; }
    [JsonPropertyName("Town_suburb")] public string Town { get; set; }
    public static string State => "NSW";
    [JsonPropertyName("Postcode")] public string PostCode { get; set; }
    [JsonPropertyName("Phone")] public string PhoneNumber { get; set; }
    [JsonPropertyName("School_Email")] public string EmailAddress { get; set; }
    [JsonPropertyName("Website")] public string Website { get; set; }
    [JsonPropertyName("Longitude")] public string Longitude { get; set; }
    [JsonPropertyName("Latitude")] public string Latitude { get; set; }
    [JsonPropertyName("Late_opening_school")] public string LateOpening { get; set; }
}
