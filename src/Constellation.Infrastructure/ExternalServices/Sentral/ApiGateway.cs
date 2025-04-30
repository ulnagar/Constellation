namespace Constellation.Infrastructure.ExternalServices.Sentral;

using Application.Interfaces.Configuration;
using Microsoft.Extensions.Options;

public sealed class ApiGateway
{
    //private readonly SentralGatewayConfiguration _settings;
    private readonly HttpClient _client;
    private readonly ILogger _logger;

    public ApiGateway(
        IOptions<SentralGatewayConfiguration> settings,
        IHttpClientFactory factory,
        ILogger logger)
    {
        //_settings = settings.Value;
        _logger = logger.ForContext<ApiGateway>();

        _client = factory.CreateClient("sentral");
        _client.DefaultRequestHeaders.Add("X-API-KEY", settings.Value.ApiKey);
        _client.DefaultRequestHeaders.Add("X-API-TENANT", settings.Value.ApiTenant);
    }

    private enum JsonSection
    {
        Data,
        Includes,
        Meta,
        Error,
        Links
    }

    //private async Task<Dictionary<JsonSection, List<JsonElement>>> GetResponse(Uri path, CancellationToken cancellationToken = default)
    //{
    //    Dictionary<JsonSection, List<JsonElement>> completeResponse = new();
    //    completeResponse.Add(JsonSection.Data, new());
    //    completeResponse.Add(JsonSection.Includes, new());
        
    //    bool nextPageExists = true;

    //    while (nextPageExists)
    //    {
    //        HttpResponseMessage response = await _client.GetAsync(path, cancellationToken);

    //        if (!response.IsSuccessStatusCode)
    //        {
    //            return completeResponse;
    //        }

    //        string responseText = await response.Content.ReadAsStringAsync(cancellationToken);

    //        using JsonDocument document = JsonDocument.Parse(responseText);
    //        JsonElement root = document.RootElement;
    //        bool errorsExist = root.TryGetProperty("errors", out JsonElement errors);

    //        if (errorsExist)
    //        {
    //            // do something with the errors
    //            foreach (JsonElement item in errors.EnumerateArray())
    //                completeResponse[JsonSection.Error].Add(item.Clone());

    //            return completeResponse;
    //        }

    //        bool linksExist = root.TryGetProperty("links", out JsonElement links);

    //        if (!linksExist)
    //        {
    //            nextPageExists = false;
    //        }
    //        else
    //        {
    //            // do something with the links
    //            bool nextLinkExists = links.TryGetProperty("next", out JsonElement nextLink);

    //            if (nextLinkExists)
    //                path = new Uri(nextLink.GetString()!);
    //            else
    //                nextPageExists = false;
    //        }

    //        bool dataExists = root.TryGetProperty("data", out JsonElement data);

    //        switch (dataExists)
    //        {
    //            case true when data.ValueKind == JsonValueKind.Array:
    //            {
    //                foreach (JsonElement item in data.EnumerateArray())
    //                    completeResponse[JsonSection.Data].Add(item.Clone());
    //                break;
    //            }
    //            case true when data.ValueKind == JsonValueKind.Object:
    //                completeResponse[JsonSection.Data].Add(data.Clone());
    //                break;
    //        }

    //        bool includesExists = root.TryGetProperty("included", out JsonElement includes);

    //        if (includesExists)
    //        {
    //            foreach (JsonElement item in includes.EnumerateArray())
    //                completeResponse[JsonSection.Includes].Add(item.Clone());
    //        }
    //    }

    //    return completeResponse;
    //}

    //public Task<string> GetSentralStudentIdAsync(string studentName) => throw new NotImplementedException();

    //public Task<List<SentralPeriodAbsenceDto>> GetAbsenceDataAsync(string sentralStudentId) => throw new NotImplementedException();

    //public Task<List<SentralPeriodAbsenceDto>> GetPartialAbsenceDataAsync(string sentralStudentId) => throw new NotImplementedException();

    //public Task<string> GetSentralStudentIdFromSRN(string srn, string grade) => throw new NotImplementedException();

    ///// <summary>
    ///// Not used/required
    ///// </summary>
    ///// <returns></returns>
    ///// <exception cref="NotImplementedException"></exception>
    //public Task<Dictionary<string, List<string>>> GetFamilyGroupings() => throw new NotImplementedException();

    //public async Task<FamilyDetailsDto> GetParentContactEntry(string sentralStudentId)
    //{
    //    Uri path = new($"https://admin.aurora.dec.nsw.gov.au/restapi/v1/core/core-student/{sentralStudentId}?include=studentRelationships,contacts");

    //    Dictionary<JsonSection, List<JsonElement>> studentResponse = await GetResponse(path);
        
    //    List<CoreStudent> students = new();
    //    List<CoreStudentRelationship> people = new();
    //    List<CoreStudentPersonRelation> relationships = new();
    //    CoreFamily family = new();

    //    foreach (KeyValuePair<JsonSection, List<JsonElement>> section in studentResponse)
    //    {
    //        switch (section.Key)
    //        {
    //            case JsonSection.Data:
    //            {
    //                foreach (JsonElement entry in section.Value)
    //                {
    //                    Result<CoreStudent> student = CoreStudent.ConvertFromJson(entry);

    //                    if (student.IsFailure)
    //                        continue;

    //                    students.Add(student.Value);
    //                }

    //                break;
    //            }
    //            case JsonSection.Includes:
    //            {
    //                foreach (JsonElement entry in section.Value)
    //                {
    //                    string type = entry.ExtractString("type");

    //                    switch (type)
    //                    {
    //                        case "coreStudentRelationship":
    //                        {
    //                            Result<CoreStudentRelationship> relationship = CoreStudentRelationship.ConvertFromJson(entry);
    //                            if (relationship.IsFailure)
    //                                continue;

    //                            people.Add(relationship.Value);
    //                            break;
    //                        }
    //                        case "coreStudentPersonRelation":
    //                        {
    //                            Result<CoreStudentPersonRelation> relationship = CoreStudentPersonRelation.ConvertFromJson(entry);
    //                            if (relationship.IsFailure)
    //                                continue;

    //                            relationships.Add(relationship.Value);
    //                            break;
    //                        }
    //                    }
    //                }

    //                break;
    //            }
    //        }
    //    }

    //    string? familyId = students.FirstOrDefault()?.FamilyId;

    //    if (string.IsNullOrWhiteSpace(familyId))
    //        return new FamilyDetailsDto();

    //    path = new($"https://admin.aurora.dec.nsw.gov.au/restapi/v1/core/core-family/{familyId}");

    //    Dictionary<JsonSection, List<JsonElement>> familyResponse = await GetResponse(path);

    //    foreach (JsonElement entry in familyResponse.Where(entry => entry.Key == JsonSection.Data).SelectMany(entry => entry.Value))
    //    {
    //        Result<CoreFamily> familyResult = CoreFamily.ConvertFromJson(entry);

    //        if (familyResult.IsFailure)
    //            continue;

    //        family = familyResult.Value;
    //    }

    //    if (family is null)
    //        return new FamilyDetailsDto();

    //    FamilyDetailsDto familyDetails = new()
    //    {
    //        FamilyId = family.FamilyId,
    //        AddressName = family.AddressTitle,
    //        AddressLine1 = family.AddressStreetNo,
    //        AddressLine2 = family.AddressStreet,
    //        AddressTown = family.AddressSuburb,
    //        AddressState = family.AddressState,
    //        AddressPostCode = family.AddressPostCode,
    //        FamilyEmail = family.EmailAddress
    //    };

    //    foreach (CoreStudentRelationship person in people.Where(person => person.IsResidentialGuardian))
    //    {
    //        familyDetails.Contacts.Add(new()
    //        {
    //            Title = person.Title,
    //            FirstName = person.FirstName,
    //            LastName = person.LastName,
    //            SentralId = person.PersonId,
    //            Email = person.EmailAddress,
    //            Mobile = person.Mobile,
    //            Sequence = person.Sequence,
    //            SentralReference = person.Gender switch
    //            {
    //                "M" => Parent.SentralReference.Father,
    //                "F" => Parent.SentralReference.Mother,
    //                _ => Parent.SentralReference.Other
    //            }
    //        });
    //    }

    //    foreach (CoreStudent student in students.Where(student => student.IsActive))
    //    {
    //        familyDetails.StudentIds.Add(student.StudentReferenceNumber);
    //    }

    //    return familyDetails;
    //}

    //public async Task<List<DateOnly>> GetExcludedDatesFromCalendar(string year)
    //{
    //    Uri path = new($"https://admin.aurora.dec.nsw.gov.au/restapi/v1/core/date");

    //    Dictionary<JsonSection, List<JsonElement>> studentResponse = await GetResponse(path);

    //    List<CoreDate> dates = new();

    //    foreach (KeyValuePair<JsonSection, List<JsonElement>> section in studentResponse)
    //    {
    //        switch (section.Key)
    //        {
    //            case JsonSection.Data:
    //            {
    //                foreach (JsonElement entry in section.Value)
    //                {
    //                    Result<CoreDate> date = CoreDate.ConvertFromJson(entry);

    //                    if (date.IsFailure)
    //                        continue;

    //                    dates.Add(date.Value);
    //                }

    //                break;
    //            }
    //        }
    //    }

    //    IEnumerable<CoreDate> excludedDates = dates.Where(entry => entry.Code != "W" && entry.Code != "S");

    //    return excludedDates.Select(entry => entry.Date).ToList();
    //}

    //public async Task<List<ValidAttendenceReportDate>> GetValidAttendanceReportDatesFromCalendar(string year)
    //{
    //    Uri path = new($"https://admin.aurora.dec.nsw.gov.au/restapi/v1/core/date");

    //    Dictionary<JsonSection, List<JsonElement>> studentResponse = await GetResponse(path);

    //    List<CoreDate> dates = new();

    //    foreach (KeyValuePair<JsonSection, List<JsonElement>> section in studentResponse)
    //    {
    //        switch (section.Key)
    //        {
    //            case JsonSection.Data:
    //            {
    //                foreach (JsonElement entry in section.Value)
    //                {
    //                    Result<CoreDate> date = CoreDate.ConvertFromJson(entry);

    //                    if (date.IsFailure)
    //                        continue;

    //                    dates.Add(date.Value);
    //                }

    //                break;
    //            }
    //        }
    //    }

    //    List<ValidAttendenceReportDate> response = new();

    //    for (int i = 1; i < 5; i++)
    //    {
    //        IEnumerable<CoreDate> datesFromTerm = dates.Where(entry => entry.Code != "W").Where(entry => entry.Term == i.ToString());

    //        IEnumerable<IGrouping<int, CoreDate>> groupedDates = datesFromTerm.GroupBy(entry =>
    //            int.Parse(entry.Week) % 2 == 0 
    //                ? int.Parse(entry.Week) - 1 
    //                : int.Parse(entry.Week));

    //        foreach (IGrouping<int, CoreDate> group in groupedDates)
    //        {
    //            if (group.Key == 11)
    //            {
    //                response.Add(new(
    //                    $"Term {i}",
    //                    group.MinBy(entry => entry.Date).Date.ToDateTime(TimeOnly.MinValue),
    //                    group.MaxBy(entry => entry.Date).Date.ToDateTime(TimeOnly.MinValue),
    //                    $"Term {i} Week 11"));
    //            }
    //            else
    //            {
    //                response.Add(new(
    //                    $"Term {i}",
    //                    group.MinBy(entry => entry.Date).Date.ToDateTime(TimeOnly.MinValue),
    //                    group.MaxBy(entry => entry.Date).Date.ToDateTime(TimeOnly.MinValue),
    //                    $"Term {i} Week {group.Key} - Week {group.Key + 1}"));
    //            }
    //        }
    //    }

    //    return response;
    //}

    ///// <summary>
    ///// Not suitable for use yet, as there is no way to identify unmarked rolls for cancelled lessons
    ///// </summary>
    ///// <param name="date"></param>
    ///// <returns></returns>
    //public async Task<ICollection<RollMarkReportDto>> GetRollMarkingReportAsync(DateOnly date)
    //{
    //    Uri path = new($"https://admin.aurora.dec.nsw.gov.au/restapi/v1/attendance/period-roll?date={date.ToString("yyyy-MM-dd")}");

    //    Dictionary<JsonSection, List<JsonElement>> response = await GetResponse(path);

    //    List<PeriodRoll> rolls = new();

    //    foreach (KeyValuePair<JsonSection, List<JsonElement>> section in response)
    //    {
    //        switch (section.Key)
    //        {
    //            case JsonSection.Data:
    //            {
    //                foreach (JsonElement entry in section.Value)
    //                {
    //                    Result<PeriodRoll> roll = PeriodRoll.ConvertFromJson(entry);

    //                    if (roll.IsFailure)
    //                        continue;

    //                    rolls.Add(roll.Value);
    //                }

    //                break;
    //            }
    //        }
    //    }

    //    path = new($"https://admin.aurora.dec.nsw.gov.au/restapi/v1/core/core-classes?include=assignedStaff");

    //    Dictionary<JsonSection, List<JsonElement>> classResponse = await GetResponse(path);

    //    List<CoreClass> classes = new();
    //    List<CoreStaff> staffMembers = new();

    //    foreach (KeyValuePair<JsonSection, List<JsonElement>> section in classResponse)
    //    {
    //        switch (section.Key)
    //        {
    //            case JsonSection.Data:
    //            {
    //                foreach (JsonElement entry in section.Value)
    //                {
    //                    Result<CoreClass> item = CoreClass.ConvertFromJson(entry);

    //                    if (item.IsFailure)
    //                        continue;

    //                    classes.Add(item.Value);
    //                }

    //                break;
    //            }
    //            case JsonSection.Includes:
    //            {
    //                foreach (JsonElement entry in section.Value)
    //                {
    //                    string type = entry.ExtractString("type");

    //                    switch (type)
    //                    {
    //                        case "coreStaff":
    //                            {
    //                                Result<CoreStaff> staff = CoreStaff.ConvertFromJson(entry);
    //                                if (staff.IsFailure)
    //                                    continue;

    //                                if (staffMembers.All(item => item.Id != staff.Value.Id))
    //                                    staffMembers.Add(staff.Value);

    //                                break;
    //                            }
    //                    }
    //                }

    //                break;
    //            }
    //        }
    //    }

    //    //IEnumerable<PeriodRoll> unsubmittedRolls = rolls.Where(entry => entry.IsSubmitted == false);

    //    List<RollMarkReportDto> result = new();

    //    foreach (PeriodRoll entry in rolls)
    //    {
    //        RollMarkReportDto reportEntry = new()
    //        {
    //            Date = date,
    //            Period = entry.PeriodName,
    //            Room = string.Empty,
    //            Link = entry.RollMarkingUrl,
    //            Submitted = entry.IsSubmitted
    //        };
            
    //        string classId = new Uri(entry.RollMarkingUrl).Segments[^3].Replace("/", string.Empty);

    //        CoreClass matchingClass = classes.FirstOrDefault(item => item.Id == classId);

    //        if (matchingClass is not null)
    //        {
    //            CoreStaff matchingTeacher = staffMembers.FirstOrDefault(item => item.Id == matchingClass.TeacherId);

    //            if (matchingTeacher is not null)
    //            {
    //                reportEntry.ClassName = matchingClass.Name;
    //                reportEntry.ClassDescription = matchingClass.Description;
    //                reportEntry.Teacher = matchingTeacher.Name;
    //                reportEntry.TeacherEmail = matchingTeacher.EmailAddress;
    //            }
    //        }

    //        result.Add(reportEntry);
    //    }

    //    return result;
    //}

    //public async Task<ICollection<FamilyDetailsDto>> GetFamilyDetailsReport(ILogger logger)
    //{
    //    Uri path = new($"https://admin.aurora.dec.nsw.gov.au/restapi/v1/core/core-student?includeInactive=0");

    //    Dictionary<JsonSection, List<JsonElement>> studentResponse = await GetResponse(path);

    //    List<FamilyDetailsDto> familyDetails = new();

    //    foreach (JsonElement entry in studentResponse[JsonSection.Data])
    //    {
    //        string sentralId = entry.ExtractString("id");

    //        FamilyDetailsDto response = await GetParentContactEntry(sentralId);

    //        FamilyDetailsDto existingEntry = familyDetails.FirstOrDefault(dto => dto.FamilyId == response.FamilyId);

    //        if (existingEntry is not null)
    //        {
    //            existingEntry.StudentIds.AddRange(response.StudentIds);
    //        }
    //        else
    //        {
    //            familyDetails.Add(response);
    //        }
    //    }

    //    return familyDetails;
    //}

    //public Task<byte[]> GetSentralStudentPhoto(string studentId) => throw new NotImplementedException();

    //public Task<ICollection<SentralReportDto>> GetStudentReportList(string sentralStudentId) => throw new NotImplementedException();

    //public Task<byte[]> GetStudentReport(string sentralStudentId, string reportId) => throw new NotImplementedException();

    //public Task<HtmlDocument> GetAwardsReport(CancellationToken cancellationToken = default) => throw new NotImplementedException();

    //public Task<HtmlDocument> GetAwardsListing(string sentralStudentId, string calYear, CancellationToken cancellationToken = default) => throw new NotImplementedException();

    //public Task<HtmlDocument> GetIncidentDetailsPage(string uri, CancellationToken cancellationToken = default) => throw new NotImplementedException();

    //public Task<byte[]> GetAwardDocument(string sentralStudentId, string incidentId) => throw new NotImplementedException();

    //public Task<SystemAttendanceData> GetAttendancePercentages(string term, string week, string year, DateOnly startDate, DateOnly endDate) => throw new NotImplementedException();

    //public async Task<Result<(DateOnly StartDate, DateOnly EndDate)>> GetDatesForWeek(string year, string term, string week)
    //{
    //    Uri path = new($"https://admin.aurora.dec.nsw.gov.au/restapi/v1/core/date");

    //    Dictionary<JsonSection, List<JsonElement>> studentResponse = await GetResponse(path);

    //    List<CoreDate> dates = new();

    //    foreach (KeyValuePair<JsonSection, List<JsonElement>> section in studentResponse)
    //    {
    //        switch (section.Key)
    //        {
    //            case JsonSection.Data:
    //            {
    //                foreach (JsonElement entry in section.Value)
    //                {
    //                    Result<CoreDate> date = CoreDate.ConvertFromJson(entry);

    //                    if (date.IsFailure)
    //                        continue;

    //                    dates.Add(date.Value);
    //                }

    //                break;
    //            }
    //        }
    //    }

    //    IEnumerable<CoreDate> correctDates = dates.Where(entry => entry.Term == term && entry.Week == week);

    //    return (correctDates.MinBy(entry => entry.Date).Date, correctDates.MaxBy(entry => entry.Date).Date);
    //}

    //public async Task<Result<(string Week, string Term)>> GetWeekForDate(DateOnly date)
    //{
    //    Uri path = new($"https://admin.aurora.dec.nsw.gov.au/restapi/v1/core/date/{date.ToString("yyyy-MM-dd")}");

    //    Dictionary<JsonSection, List<JsonElement>> studentResponse = await GetResponse(path);

    //    List<CoreDate> dates = new();

    //    foreach (KeyValuePair<JsonSection, List<JsonElement>> section in studentResponse)
    //    {
    //        switch (section.Key)
    //        {
    //            case JsonSection.Data:
    //            {
    //                foreach (JsonElement entry in section.Value)
    //                {
    //                    Result<CoreDate> dateEntry = CoreDate.ConvertFromJson(entry);

    //                    if (dateEntry.IsFailure)
    //                        continue;

    //                    dates.Add(dateEntry.Value);
    //                }

    //                break;
    //            }
    //        }
    //    }

    //    if (!dates.Any() || dates.Count > 1)
    //        return Result.Failure<(string Week, string Term)>(SentralJsonErrors.TooManyResponses);

    //    return (dates.First().Week, dates.First().Term);
    //}

    //public Task<(Stream BasicFile, Stream DetailFile)> GetNAwardReport(CancellationToken cancellationToken = default) => throw new NotImplementedException();
}