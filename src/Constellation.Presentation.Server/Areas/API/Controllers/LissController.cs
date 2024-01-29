namespace Constellation.Presentation.Server.Areas.API.Controllers;

using Core.Primitives;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System.Text.Json;
using System.Text.Json.Serialization;

[Route("liss")]
[ApiController]
public sealed class LissController : ControllerBase
{
    private readonly ILogger _logger;

    public LissController(
        ILogger logger)
    {
        _logger = logger.ForContext<LissController>();
    }

    [HttpPost]
    [Route("json")]
    public async Task<ILissResponse> ReceiveJson([FromBody] object body)
    {
        string stringValue = body.ToString();
        if (stringValue is null)
            return new LissResponseError() { Error = "Invalid call" }; // error

        LissCall callDetails = JsonSerializer.Deserialize<LissCall>(stringValue);

        if (callDetails is null)
            return new LissResponseError() { Error = "Invalid call" }; // error

        if (LissCallMethod.FromValue(callDetails.Method) == LissCallMethod.Hello)
            return new LissResponse() { Id = callDetails.Id, Result = new LissResponseHello() { LissVersion = 10000 } };

        if (callDetails.Params.Length == 0 || callDetails.Params[0] is null)
            return new LissResponseError() { Error = "Invalid call" }; // error

        // Check authorisation
        string authorisationString = callDetails.Params[0].ToString();
        if (authorisationString is null)
            return new LissResponseError() { Error = "Invalid Authentication Object" }; // error
            
        LissCallAuthorisation authorisation = JsonSerializer.Deserialize<LissCallAuthorisation>(authorisationString);

        if (authorisation.UserName != "backup.admin" && authorisation.Password != "8912local")
            return new LissResponseError() { Error = "Invalid Authentication Object" };

        // Check Method
        LissCallMethod method = LissCallMethod.FromValue(callDetails.Method);

        if (method == LissCallMethod.PublishStudents)
            return await PublishStudents(callDetails.Params);

        if (method == LissCallMethod.PublishTimetable)
            return await PublishTimetable(callDetails.Params);

        if (method == LissCallMethod.PublishTeachers)
            return await PublishTeachers(callDetails.Params);

        return new LissResponse();
    }

    private async Task<ILissResponse> PublishStudents(object[] request)
    {
        if (request.Length != 3)
        {
            return new LissResponseError() { Error = "Invalid parameters provided!" };
        }

        List<LissPublishStudents> students = JsonSerializer.Deserialize<List<LissPublishStudents>>(request[2].ToString());

        return new LissResponseBlank();
    }

    private async Task<ILissResponse> PublishTimetable(object[] request)
    {
        if (request.Length != 8)
        {
            return new LissResponseError() { Error = "Invalid parameters provided!" };
        }

        List<LissPublishTimetable> timetables = JsonSerializer.Deserialize<List<LissPublishTimetable>>(request[1].ToString());

        var noTeacherListed = timetables.Where(entry => entry.TeacherId == null).ToList();

        return new LissResponseBlank();
    }

    private async Task<ILissResponse> PublishTeachers(object[] request)
    {
        if (request.Length != 3)
        {
            return new LissResponseError() { Error = "Invalid parameters provided!" };
        }

        List<LissPublishTeachers> teachers = JsonSerializer.Deserialize<List<LissPublishTeachers>>(request[2].ToString());

        return new LissResponseBlank();
    }




    // Incoming Models
    public class LissCall
    {
        [JsonPropertyName("jsonrpc")]
        public string JsonRpc { get; set; }

        [JsonPropertyName("method")]
        public string Method { get; set; }

        [JsonPropertyName("params")]
        public object[] Params { get; set; }

        [JsonPropertyName("id")]
        public int Id { get; set; }
    }

    public sealed class LissCallMethod : ValueObject
    {
        public static readonly LissCallMethod Hello = new("liss.hello");
        public static readonly LissCallMethod GetStudents = new("liss.getStudents");
        public static readonly LissCallMethod PublishStudents = new("liss.publishStudents");
        public static readonly LissCallMethod GetTeachers = new("liss.getTeachers");
        public static readonly LissCallMethod PublishTeachers = new("liss.publishTeachers");
        public static readonly LissCallMethod GetRooms = new("liss.getRooms");
        public static readonly LissCallMethod PublishRooms = new("liss.publishRooms");
        public static readonly LissCallMethod GetClassMemberships = new("liss.getClassMemberships");
        public static readonly LissCallMethod PublishClassMemberships = new("liss.publishClassMemberships");
        public static readonly LissCallMethod GetClasses = new("liss.getClasses");
        public static readonly LissCallMethod PublishClasses = new("liss.publishClasses");
        public static readonly LissCallMethod GetTimetableStructures = new("liss.getTimetableStructures");
        public static readonly LissCallMethod GetTimetable = new("liss.getTimetable");
        public static readonly LissCallMethod PublishTimetable = new("liss.publishTimetable");
        public static readonly LissCallMethod PublishDailyData = new("liss.publishDailyData");
        public static readonly LissCallMethod GetBellTimes = new("liss.getBellTimes");
        public static readonly LissCallMethod PublishBellTimes = new("liss.publishBellTimes");
        public static readonly LissCallMethod GetCalendar = new("liss.getCalendar");
        public static readonly LissCallMethod PublishCalendar = new("liss.publishCalendar");
        public static readonly LissCallMethod ChangeClassMembership = new("liss.changeClassMembership");

        public static LissCallMethod FromValue(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            return new(value);
        }

        private LissCallMethod(string value)
        {
            Value = value;
        }

        public string Value { get; }

        public override IEnumerable<object> GetAtomicValues()
        {
            yield return Value;
        }
    }

    public class LissCallMethodConverter : JsonConverter<LissCallMethod>
    {
        public override LissCallMethod Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            string s = reader.GetString();

            return LissCallMethod.FromValue(s);
        }

        public override void Write(Utf8JsonWriter writer, LissCallMethod value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.Value);
        }
    }

    public class LissCallAuthorisation
    {
        [JsonPropertyName("School")]
        public string School { get; set; }

        [JsonPropertyName("UserName")]
        public string UserName { get; set; }

        [JsonPropertyName("Password")]
        public string Password { get; set; }

        [JsonPropertyName("LissVersion")]
        public int LissVersion { get; set; }

        [JsonPropertyName("UserAgent")]
        public string UserAgent { get; set; }
    }

    public class LissPublishStudents
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

    public class LissPublishTimetable
    {
        public int DayNumber { get; set; }
        public string Period { get; set; }
        public string ClassCode { get; set; }
        public string TeacherId { get; set; }
        public string RoomId { get; set; }
        public string RoomCode { get; set; }

        [JsonPropertyName("TtStructure")]
        public string Timetable { get; set; }
    }

    public class LissPublishTeachers
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


    // Outgoing Models
    public interface ILissResponse { }

    public class LissResponse : ILissResponse
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("result")]
        public LissResponseBase Result { get; set; }
    }

    public abstract class LissResponseBase {}

    public class LissResponseHello : LissResponseBase
    {
        [JsonPropertyName("SIS")]
        public string System => "Constellation";

        [JsonPropertyName("LissVersion")]
        public int LissVersion { get; set; }
    }

    public class LissResponseError : ILissResponse
    {
        [JsonPropertyName("error")]
        public string Error { get; set; }
    }

    public class LissResponseBlank : ILissResponse
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("result")]
        public string Result => "";
    }
}
