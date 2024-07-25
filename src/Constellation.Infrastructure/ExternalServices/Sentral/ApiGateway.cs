namespace Constellation.Infrastructure.ExternalServices.Sentral;

using Application.Attendance.GetAttendanceDataFromSentral;
using Application.Attendance.GetValidAttendanceReportDates;
using Application.DTOs;
using Application.Interfaces.Configuration;
using Application.Interfaces.Gateways;
using Core.Shared;
using HtmlAgilityPack;
using Microsoft.Extensions.Options;
using Models;
using Newtonsoft.Json;

public sealed class ApiGateway : ISentralGateway
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

    private async Task<Dictionary<JsonSection, List<dynamic>>> GetSingleResponse(Uri path, CancellationToken cancallationToken = default)
    {
        Dictionary<JsonSection, List<dynamic>> completeResponse = new();
        completeResponse.Add(JsonSection.Data, new());
        completeResponse.Add(JsonSection.Includes, new());
        
        bool nextPageExists = true;

        while (nextPageExists)
        {
            HttpResponseMessage response = await _client.GetAsync(path, cancallationToken);

            if (!response.IsSuccessStatusCode)
            {
                return completeResponse;
            }

            string responseText = await response.Content.ReadAsStringAsync(cancallationToken);

            dynamic item = JsonConvert.DeserializeObject(responseText);

            if (item is null || item["errors"] is not null)
            {
                return completeResponse;
            }

            completeResponse[JsonSection.Data].Add(item["data"]);

            if (item["included"] is not null)
                completeResponse[JsonSection.Includes].AddRange(item["included"]);
        
            dynamic links = item["links"];

            if (links is null)
            {
                nextPageExists = false;
                continue;
            }

            dynamic nextLink = links["next"];

            if (nextLink is null)
            {
                nextPageExists = false;
            }
            else
            {
                path = new Uri(nextLink.ToString());
            }
        }

        return completeResponse;
    }

    private async Task<Dictionary<JsonSection, List<dynamic>>> GetManyResponses(Uri path, CancellationToken cancallationToken = default)
    {
        Dictionary<JsonSection, List<dynamic>> completeResponse = new();
        completeResponse.Add(JsonSection.Data, new());
        completeResponse.Add(JsonSection.Includes, new());

        bool nextPageExists = true;

        while (nextPageExists)
        {
            HttpResponseMessage response = await _client.GetAsync(path, cancallationToken);

            if (!response.IsSuccessStatusCode)
            {
                return completeResponse;
            }

            string responseText = await response.Content.ReadAsStringAsync(cancallationToken);

            dynamic item = JsonConvert.DeserializeObject(responseText);

            if (item is null || item["errors"] is not null)
            {
                return completeResponse;
            }

            completeResponse[JsonSection.Data].AddRange(item["data"]);

            if (item["included"] is not null)
                completeResponse[JsonSection.Includes].AddRange(item["included"]);

            dynamic links = item["links"];

            if (links is null)
            {
                nextPageExists = false;
                continue;
            }

            dynamic nextLink = links["next"];

            if (nextLink is null)
            {
                nextPageExists = false;
            }
            else
            {
                path = new Uri(nextLink.ToString());
            }
        }

        return completeResponse;
    }


    public Task<string> GetSentralStudentIdAsync(string studentName) => throw new NotImplementedException();

    public Task<List<SentralPeriodAbsenceDto>> GetAbsenceDataAsync(string sentralStudentId) => throw new NotImplementedException();

    public Task<List<SentralPeriodAbsenceDto>> GetPartialAbsenceDataAsync(string sentralStudentId) => throw new NotImplementedException();

    public Task<string> GetSentralStudentIdFromSRN(string srn, string grade) => throw new NotImplementedException();

    public Task<Dictionary<string, List<string>>> GetFamilyGroupings() => throw new NotImplementedException();

    public async Task<FamilyDetailsDto> GetParentContactEntry(string sentralStudentId)
    {
        Uri path = new($"https://admin.aurora.dec.nsw.gov.au/restapi/v1/core/core-student/{sentralStudentId}?include=studentRelationships,contacts");

        Dictionary<JsonSection, List<dynamic>> studentResponse = await GetSingleResponse(path);
        
        List<CoreStudent> students = new();
        List<CoreStudentRelationship> people = new();
        List<CoreStudentPersonRelation> relationships = new();
        CoreFamily family = new();

        foreach (KeyValuePair<JsonSection, List<dynamic>> section in studentResponse)
        {
            switch (section.Key)
            {
                case JsonSection.Data:
                    {
                        foreach (dynamic entry in section.Value)
                        {
                            Result<CoreStudent> student = CoreStudent.ConvertFromJson(entry);

                            if (student.IsFailure)
                                continue;

                            students.Add(student.Value);
                        }

                        break;
                    }
                case JsonSection.Includes:
                    {
                        foreach (dynamic entry in section.Value)
                        {
                            if (entry["type"].ToString() == "coreStudentRelationship")
                            {
                                Result<CoreStudentRelationship> relationship = CoreStudentRelationship.ConvertFromJson(entry);
                                if (relationship.IsFailure)
                                    continue;

                                people.Add(relationship.Value);
                            }

                            if (entry["type"].ToString() == "coreStudentPersonRelation")
                            {
                                Result<CoreStudentPersonRelation> relationship = CoreStudentPersonRelation.ConvertFromJson(entry);
                                if (relationship.IsFailure)
                                    continue;

                                relationships.Add(relationship.Value);
                            }
                        }

                        break;
                    }
            }
        }

        string? familyId = students.FirstOrDefault()?.FamilyId;

        if (string.IsNullOrWhiteSpace(familyId))
            return new FamilyDetailsDto();

        path = new($"https://admin.aurora.dec.nsw.gov.au/restapi/v1/core/core-family/{familyId}");

        Dictionary<JsonSection, List<dynamic>> familyResponse = await GetSingleResponse(path);

        foreach (dynamic entry in familyResponse.Where(entry => entry.Key == JsonSection.Data).SelectMany(entry => entry.Value))
        {
            Result<CoreFamily> familyResult = CoreFamily.ConvertFromJson(entry);

            if (familyResult.IsFailure)
                continue;

            family = familyResult.Value;
        }

        if (family is null)
            return new FamilyDetailsDto();

        FamilyDetailsDto familyDetails = new()
        {
            FamilyId = family.FamilyId,
            AddressName = family.AddressTitle,
            AddressLine1 = family.AddressStreetNo,
            AddressLine2 = family.AddressStreet,
            AddressTown = family.AddressSuburb,
            AddressState = family.AddressState,
            AddressPostCode = family.AddressPostCode,
            FamilyEmail = family.EmailAddress
        };

        foreach (CoreStudentRelationship person in people.Where(person => person.IsResidentialGuardian))
        {
            familyDetails.Contacts.Add(new()
            {
                Title = person.Title,
                FirstName = person.FirstName,
                LastName = person.LastName,
                SentralId = person.PersonId,
                Email = person.EmailAddress,
                Mobile = person.Mobile,
                Sequence = person.Sequence
            });
        }

        foreach (CoreStudent student in students.Where(student => student.IsActive))
        {
            familyDetails.StudentIds.Add(student.StudentReferenceNumber);
        }

        return familyDetails;
    }

    public Task<List<DateOnly>> GetExcludedDatesFromCalendar(string year) => throw new NotImplementedException();

    public Task<List<ValidAttendenceReportDate>> GetValidAttendanceReportDatesFromCalendar(string year) => throw new NotImplementedException();

    public Task<ICollection<RollMarkReportDto>> GetRollMarkingReportAsync(DateOnly date) => throw new NotImplementedException();

    public async Task<ICollection<FamilyDetailsDto>> GetFamilyDetailsReport(ILogger logger)
    {
        Uri path = new($"https://admin.aurora.dec.nsw.gov.au/restapi/v1/core/core-student?includeInactive=0");

        Dictionary<JsonSection, List<dynamic>> studentResponse = await GetManyResponses(path);

        List<FamilyDetailsDto> familyDetails = new();

        foreach (dynamic entry in studentResponse[JsonSection.Data])
        {
            string sentralId = entry["id"].ToString();

            FamilyDetailsDto response = await GetParentContactEntry(sentralId);

            FamilyDetailsDto existingEntry = familyDetails.FirstOrDefault(dto => dto.FamilyId == response.FamilyId);

            if (existingEntry is not null)
            {
                existingEntry.StudentIds.AddRange(response.StudentIds);
            }
            else
            {
                familyDetails.Add(response);
            }
        }

        return familyDetails;
    }

    public Task<byte[]> GetSentralStudentPhoto(string studentId) => throw new NotImplementedException();

    public Task<ICollection<SentralReportDto>> GetStudentReportList(string sentralStudentId) => throw new NotImplementedException();

    public Task<byte[]> GetStudentReport(string sentralStudentId, string reportId) => throw new NotImplementedException();

    public Task<HtmlDocument> GetAwardsReport(CancellationToken cancellationToken = default) => throw new NotImplementedException();

    public Task<HtmlDocument> GetAwardsListing(string sentralStudentId, string calYear, CancellationToken cancellationToken = default) => throw new NotImplementedException();

    public Task<HtmlDocument> GetIncidentDetailsPage(string uri, CancellationToken cancellationToken = default) => throw new NotImplementedException();

    public Task<byte[]> GetAwardDocument(string sentralStudentId, string incidentId) => throw new NotImplementedException();

    public Task<SystemAttendanceData> GetAttendancePercentages(string term, string week, string year, DateOnly startDate, DateOnly endDate) => throw new NotImplementedException();

    public Task<Result<(DateOnly StartDate, DateOnly EndDate)>> GetDatesForWeek(string year, string term, string week) => throw new NotImplementedException();

    public Task<Result<(string Week, string Term)>> GetWeekForDate(DateOnly date) => throw new NotImplementedException();

    public Task<(Stream BasicFile, Stream DetailFile)> GetNAwardReport(CancellationToken cancellationToken = default) => throw new NotImplementedException();
}