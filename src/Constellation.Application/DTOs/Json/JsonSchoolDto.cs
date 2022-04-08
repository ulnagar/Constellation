using Constellation.Application.Features.Partners.Schools.Commands;
using System.Text.Json.Serialization;

namespace Constellation.Application.DTOs.Json
{
    public class JsonSchoolDto
    {
        [JsonPropertyName("School_code")] public string Code { get; set; }
        [JsonPropertyName("School_name")] public string Name { get; set; }
        [JsonPropertyName("Street")] public string Address { get; set; }
        [JsonPropertyName("Town_suburb")] public string Town { get; set; }
        public string State => "NSW";
        [JsonPropertyName("Postcode")] public string PostCode { get; set; }
        [JsonPropertyName("Phone")] public string PhoneNumber { get; set; }
        [JsonPropertyName("School_Email")] public string EmailAddress { get; set; }
        [JsonPropertyName("Website")] public string Website { get; set; }
        [JsonPropertyName("Longitude")] public string Longitude { get; set; }
        [JsonPropertyName("Latitude")] public string Latitude { get; set; }
        [JsonPropertyName("Late_opening_school")] public string LateOpening { get; set; }

        public UpsertSchool ConvertToCommand()
        {
            var formattedPhoneNumber = PhoneNumber.Replace(" ", "").Replace("(", "").Replace(")", "").Trim();
            formattedPhoneNumber = (formattedPhoneNumber.Length == 8) ? $"02{formattedPhoneNumber}" : formattedPhoneNumber;

            return new UpsertSchool
            {
                Code = Code,
                Name = Name,
                Address = Address,
                Town = Town,
                State = State,
                PostCode = PostCode,
                PhoneNumber = formattedPhoneNumber,
                EmailAddress = EmailAddress,
                Website = Website,
                Longitude = double.Parse(Longitude),
                Latitude = double.Parse(Latitude),
                LateOpening = (LateOpening == "Y")
            };
        }
    }
}
