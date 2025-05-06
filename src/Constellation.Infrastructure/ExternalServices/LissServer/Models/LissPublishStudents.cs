namespace Constellation.Infrastructure.ExternalServices.LissServer.Models;

using System;
using System.Text.Json.Serialization;

public sealed class LissPublishStudents
{
    [JsonPropertyName("StudentId")]
    public string StudentId { get; set; }

    [JsonPropertyName("FirstName")]
    public string FirstName { get; set; }

    [JsonPropertyName("Surname")]
    public string LastName { get; set; }

    [JsonPropertyName("PreferredName")]
    public string PreferredName { get; set; }

    [JsonPropertyName("Form")]
    public string Grade { get; set; }

    [JsonPropertyName("RollGroup")]
    public string RollGroup { get; set; }

    [JsonPropertyName("House")]
    public string House { get; set; }

    [JsonPropertyName("Gender")]
    public string Gender { get; set; }

    [JsonPropertyName("StatewideId")]
    public string StudentReference { get; set; }

    [JsonPropertyName("Email")]
    public string EmailAddress { get; set; }

    [JsonPropertyName("Phone")]
    public string PhoneNumber { get; set; }

    [JsonPropertyName("Guid")]
    public string UniqueId { get; set; }

    [JsonPropertyName("StartDate")]
    public DateTime StartDate { get; set; }

    [JsonPropertyName("EndDate")]
    public DateTime EndDate { get; set; }
}