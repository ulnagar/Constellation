namespace Constellation.Infrastructure.ExternalServices.LissServer.Models;

using System.Text.Json.Serialization;

public sealed class LissPublishTeachers
{
    public string TeacherId { get; set; }
    public string TeacherCode { get; set; }
    public string Title { get; set; }
    public string FirstName { get; set; }

    [JsonPropertyName("Surname")]
    public string LastName { get; set; }
    public string PreferredName { get; set; }
    public string DisplayName { get; set; }
    public string Faculty { get; set; }
    public string StaffType { get; set; }
    public string Gender { get; set; }
    public string DaysAvailable { get; set; }

    [JsonPropertyName("Email")]
    public string EmailAddress { get; set; }

    [JsonPropertyName("Phone")]
    public string PhoneNumber { get; set; }

    [JsonPropertyName("Guid")]
    public string UniqueId { get; set; }
}