namespace Constellation.Infrastructure.ExternalServices.Sentral;

using Application.Attendance.GetAttendanceDataFromSentral;
using Application.Attendance.GetValidAttendanceReportDates;
using Application.Awards.Enums;
using Application.DTOs;
using Application.Extensions;
using Application.Interfaces.Configuration;
using Application.Interfaces.Gateways;
using Constellation.Core.Models;
using Constellation.Core.Models.Students.Identifiers;
using Constellation.Core.Models.Students.ValueObjects;
using Constellation.Infrastructure.Extensions;
using Constellation.Infrastructure.ExternalServices.Sentral.Models;
using Core.Abstractions.Clock;
using Core.Models.Families;
using Core.Models.Students.Enums;
using Core.Shared;
using Errors;
using ExcelDataReader;
using HtmlAgilityPack;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;

public class Gateway : ISentralGateway
{
    private readonly IDateTimeProvider _dateTime;
    private readonly SentralGatewayConfiguration _settings;
    private readonly ILogger _logger;
    private readonly bool _logOnly = true;
    private readonly HttpClient _client;
    private readonly HttpClient _apiClient;

    private HtmlDocument _studentListPage;

    public Gateway(
        IDateTimeProvider dateTime,
        IOptions<SentralGatewayConfiguration> settings, 
        IHttpClientFactory factory,
        ILogger logger)
    {
        _dateTime = dateTime;
        _logger = logger.ForContext<ISentralGateway>();

        _settings = settings.Value;

        _logOnly = !_settings.IsConfigured();

        if (_logOnly)
        {
            _logger.Information("Gateway initalised in log only mode");

            return;
        }

        _client = factory.CreateClient("sentral");


        _apiClient = factory.CreateClient("sentral");
        _apiClient.DefaultRequestHeaders.Add("X-API-KEY", settings.Value.ApiKey);
        _apiClient.DefaultRequestHeaders.Add("X-API-TENANT", settings.Value.ApiTenant);
    }

    #region API Operations

    private enum JsonSection
    {
        Data,
        Includes,
        Meta,
        Error,
        Links
    }

    private async Task<Dictionary<JsonSection, List<JsonElement>>> GetApiJsonResponse(Uri path, CancellationToken cancellationToken = default)
    {
        Dictionary<JsonSection, List<JsonElement>> completeResponse = new();
        completeResponse.Add(JsonSection.Data, new());
        completeResponse.Add(JsonSection.Includes, new());

        bool nextPageExists = true;

        while (nextPageExists)
        {
            HttpResponseMessage response = await _apiClient.GetAsync(path, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return completeResponse;
            }

            string responseText = await response.Content.ReadAsStringAsync(cancellationToken);

            using JsonDocument document = JsonDocument.Parse(responseText);
            JsonElement root = document.RootElement;
            bool errorsExist = root.TryGetProperty("errors", out JsonElement errors);

            if (errorsExist)
            {
                // do something with the errors
                foreach (JsonElement item in errors.EnumerateArray())
                    completeResponse[JsonSection.Error].Add(item.Clone());

                return completeResponse;
            }

            bool linksExist = root.TryGetProperty("links", out JsonElement links);

            if (!linksExist)
            {
                nextPageExists = false;
            }
            else
            {
                // do something with the links
                bool nextLinkExists = links.TryGetProperty("next", out JsonElement nextLink);

                if (nextLinkExists)
                    path = new Uri(nextLink.GetString()!);
                else
                    nextPageExists = false;
            }

            bool dataExists = root.TryGetProperty("data", out JsonElement data);

            switch (dataExists)
            {
                case true when data.ValueKind == JsonValueKind.Array:
                    {
                        foreach (JsonElement item in data.EnumerateArray())
                            completeResponse[JsonSection.Data].Add(item.Clone());
                        break;
                    }
                case true when data.ValueKind == JsonValueKind.Object:
                    completeResponse[JsonSection.Data].Add(data.Clone());
                    break;
            }

            bool includesExists = root.TryGetProperty("included", out JsonElement includes);

            if (includesExists)
            {
                foreach (JsonElement item in includes.EnumerateArray())
                    completeResponse[JsonSection.Includes].Add(item.Clone());
            }
        }

        return completeResponse;
    }

    private async Task<byte[]> GetApiImageResponse(Uri path, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await _apiClient.GetAsync(path, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            return [];
        }

        return await response.Content.ReadAsByteArrayAsync(cancellationToken);
    }

    public async Task<ICollection<FamilyDetailsDto>> GetFamilyDetailsReportFromApi(ILogger logger, CancellationToken cancellationToken = default)
    {
        Uri path = new($"https://admin.aurora.dec.nsw.gov.au/restapi/v1/core/core-student?includeInactive=0");

        Dictionary<JsonSection, List<JsonElement>> studentResponse = await GetApiJsonResponse(path, cancellationToken);

        List<FamilyDetailsDto> familyDetails = new();

        foreach (JsonElement entry in studentResponse[JsonSection.Data])
        {
            string sentralId = entry.ExtractString("id");

            FamilyDetailsDto response = await GetParentContactEntryFromApi(sentralId, cancellationToken);

            FamilyDetailsDto existingEntry = familyDetails.FirstOrDefault(dto => dto.FamilyId == response.FamilyId);

            if (existingEntry is not null)
            {
                existingEntry.StudentReferenceNumbers.AddRange(response.StudentReferenceNumbers);
            }
            else
            {
                familyDetails.Add(response);
            }
        }

        return familyDetails;
    }

    public async Task<FamilyDetailsDto> GetParentContactEntryFromApi(string sentralStudentId, CancellationToken cancellationToken = default)
    {
        Uri path = new($"https://admin.aurora.dec.nsw.gov.au/restapi/v1/core/core-student/{sentralStudentId}?include=studentRelationships,contacts");

        Dictionary<JsonSection, List<JsonElement>> studentResponse = await GetApiJsonResponse(path, cancellationToken);

        List<CoreStudent> students = new();
        List<CoreStudentRelationship> people = new();
        List<CoreStudentPersonRelation> relationships = new();
        CoreFamily family = new();

        foreach (KeyValuePair<JsonSection, List<JsonElement>> section in studentResponse)
        {
            switch (section.Key)
            {
                case JsonSection.Data:
                    {
                        foreach (JsonElement entry in section.Value)
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
                        foreach (JsonElement entry in section.Value)
                        {
                            string type = entry.ExtractString("type");

                            switch (type)
                            {
                                case "coreStudentRelationship":
                                    {
                                        Result<CoreStudentRelationship> relationship = CoreStudentRelationship.ConvertFromJson(entry);
                                        if (relationship.IsFailure)
                                            continue;

                                        people.Add(relationship.Value);
                                        break;
                                    }
                                case "coreStudentPersonRelation":
                                    {
                                        Result<CoreStudentPersonRelation> relationship = CoreStudentPersonRelation.ConvertFromJson(entry);
                                        if (relationship.IsFailure)
                                            continue;

                                        relationships.Add(relationship.Value);
                                        break;
                                    }
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

        Dictionary<JsonSection, List<JsonElement>> familyResponse = await GetApiJsonResponse(path, cancellationToken);

        foreach (JsonElement entry in familyResponse.Where(entry => entry.Key == JsonSection.Data).SelectMany(entry => entry.Value))
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
                Sequence = person.Sequence,
                SentralReference = person.Gender switch
                {
                    "M" => Parent.SentralReference.Father,
                    "F" => Parent.SentralReference.Mother,
                    _ => Parent.SentralReference.Other
                }
            });
        }

        foreach (CoreStudent student in students.Where(student => student.IsActive))
        {
            familyDetails.StudentReferenceNumbers.Add(student.StudentReferenceNumber);
        }

        return familyDetails;
    }

    public async Task<byte[]> GetSentralStudentPhotoFromApi(string sentralStudentId, CancellationToken cancellationToken = default)
    {
        Uri path = new($"https://admin.aurora.dec.nsw.gov.au/restapi/v1/core/core-student/{sentralStudentId}/photo");

        byte[] imageResponse = await GetApiImageResponse(path, cancellationToken);

        return imageResponse;
    }

    #endregion

    private async Task Login(CancellationToken cancellationToken)
    {
        string uri = $"{_settings.ServerUrl}/auth/?manual=true";

        HttpRequestMessage request = new();
        request.Headers.CacheControl = new CacheControlHeaderValue
        {
            NoCache = true
        };

        List<KeyValuePair<string, string>> formData = new()
        {
            new KeyValuePair<string, string>("username", _settings.Username),
            new KeyValuePair<string, string>("password", _settings.Password),
            new KeyValuePair<string, string>("action", "login")
        };

        FormUrlEncodedContent formDataEncoded = new(formData);

        for (int i = 1; i < 6; i++)
        {
            try
            {
                HttpResponseMessage response = await _client.PostAsync(uri, formDataEncoded, cancellationToken);
                response.EnsureSuccessStatusCode();

                return;
            }
            catch (Exception ex)
            {
                _logger
                    .ForContext("Method", nameof(Login))
                    .Warning($"Failed to login to Sentral Server with error: {ex.Message}");

                if (ex.InnerException != null)
                    _logger.Warning($"Inner Exception: {ex.InnerException.Message}");

                // Wait and retry
                await Task.Delay(5000, cancellationToken);
            }
        }

        throw new Exception($"Could not connect to Sentral Server");
    }

    private async Task<string> GetJsonByGet(string uri, CancellationToken cancellationToken)
    {
        for (int i = 1; i < 6; i++)
        {
            try
            {
                await Login(cancellationToken);

                HttpResponseMessage response = await _client.GetAsync(uri, cancellationToken);
                string content = await response.Content.ReadAsStringAsync(cancellationToken);

                return content;
            }
            catch (Exception ex)
            {
                _logger.Warning($"Failed to retrieve information from Sentral Server with error: {ex.Message}");
                if (ex.InnerException != null)
                    _logger.Warning($"Inner Exception: {ex.InnerException.Message}");

                // Wait and retry
                await Task.Delay(5000, cancellationToken);
            }
        }

        return null;
    }

    private async Task<HtmlDocument> GetPageByGet(string uri, CancellationToken cancellationToken)
    {
        for (int i = 1; i < 6; i++)
        {
            try
            {
                await Login(cancellationToken);

                HttpResponseMessage response = await _client.GetAsync(uri, cancellationToken);
                string content = await response.Content.ReadAsStringAsync(cancellationToken);
                
                HtmlDocument page = new();
                page.LoadHtml(content);

                return page;
            }
            catch (Exception ex)
            {
                _logger.Warning($"Failed to retrieve information from Sentral Server with error: {ex.Message}");
                if (ex.InnerException != null)
                    _logger.Warning($"Inner Exception: {ex.InnerException.Message}");

                // Wait and retry
                await Task.Delay(5000, cancellationToken);
            }
        }

        return null;
    }

    private async Task<HtmlDocument> GetPageByPost(Uri uri, List<KeyValuePair<string, string>> payload, CancellationToken cancellationToken)
    {
        for (int i = 1; i < 6; i++)
        {
            try
            {
                await Login(cancellationToken);

                using FormUrlEncodedContent formContent = new(payload);
                HttpResponseMessage response = await _client.PostAsync(uri, formContent, cancellationToken);
                string content = await response.Content.ReadAsStringAsync(cancellationToken);
                
                HtmlDocument page = new();
                page.LoadHtml(content);

                return page;
            }
            catch (Exception ex)
            {
                _logger.Warning($"Failed to retrieve information from Sentral Server with error: {ex.Message}");
                if (ex.InnerException != null)
                    _logger.Warning($"Inner Exception: {ex.InnerException.Message}");

                // Wait and retry
                await Task.Delay(5000, cancellationToken);
            }
        }

        return null;
    }

    private async Task<Stream> GetStreamByPost(Uri uri, List<KeyValuePair<string, string>> payload, CancellationToken cancellationToken)
    {
        for (int i = 1; i < 6; i++)
        {
            try
            {
                await Login(cancellationToken);

                using FormUrlEncodedContent formContent = new(payload);
                HttpResponseMessage response = await _client.PostAsync(uri, formContent, cancellationToken);
                
                return await response.Content.ReadAsStreamAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.Warning($"Failed to retrieve information from Sentral Server with error: {ex.Message}");
                if (ex.InnerException != null)
                    _logger.Warning($"Inner Exception: {ex.InnerException.Message}");

                // Wait and retry
                await Task.Delay(5000, cancellationToken);
            }
        }

        return Stream.Null;
    }

    private async Task<Stream> GetStreamByGet(string uri, CancellationToken cancellationToken)
    {
        for (int i = 1; i < 6; i++)
        {
            try
            {
                await Login(cancellationToken);

                HttpResponseMessage response = await _client.GetAsync(uri, cancellationToken);

                return await response.Content.ReadAsStreamAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.Warning($"Failed to retrieve information from Sentral Server with error: {ex.Message}");
                if (ex.InnerException != null)
                    _logger.Warning($"Inner Exception: {ex.InnerException.Message}");

                // Wait and retry
                await Task.Delay(5000, cancellationToken);
            }
        }

        return Stream.Null;
    }

    private async Task<byte[]> GetByteArrayByGet(string uri, CancellationToken cancellationToken)
    {
        for (int i = 1; i < 6; i++)
        {
            try
            {
                await Login(cancellationToken);

                HttpResponseMessage response = await _client.GetAsync(uri, cancellationToken);
                
                return await response.Content.ReadAsByteArrayAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.Warning($"Failed to retrieve information from Sentral Server with error: {ex.Message}");
                if (ex.InnerException != null)
                    _logger.Warning($"Inner Exception: {ex.InnerException.Message}");

                // Wait and retry
                await Task.Delay(5000, cancellationToken);
            }
        }

        return null;
    }

    private async Task<byte[]> GetByteArrayByPost(string uri, List<KeyValuePair<string, string>> payload, CancellationToken cancellationToken)
    {
        for (int i = 1; i < 6; i++)
        {
            try
            {
                await Login(cancellationToken);

                HttpResponseMessage response = await _client.PostAsync(uri, new FormUrlEncodedContent(payload), cancellationToken);
                byte[] content = await response.Content.ReadAsByteArrayAsync(cancellationToken);

                return content;
            }
            catch (Exception ex)
            {
                _logger.Warning($"Failed to retrieve information from Sentral Server with error: {ex.Message}");
                if (ex.InnerException != null)
                    _logger.Warning($"Inner Exception: {ex.InnerException.Message}");

                // Wait and retry
                await Task.Delay(5000, cancellationToken);
            }
        }

        return null;
    }

    public async Task<Result<DateTime>> IssueAward(
        List<string> studentSentralIds,
        IssueAwardType awardType)
    {
        if (_logOnly)
        {
            _logger
                .ForContext(nameof(studentSentralIds), studentSentralIds, true)
                .ForContext(nameof(awardType), awardType, true)
                .Information("IssueAward");

            return _dateTime.Now;
        }

        if (studentSentralIds is null || studentSentralIds.Count == 0)
            return Result.Failure<DateTime>(SentralGatewayErrors.NoStudentIdsProvided);

        // Stellar = 3, Galaxy = 6, Universal = 7
        string award = awardType switch
        {
            IssueAwardType.Stellar => "3",
            IssueAwardType.Galaxy => "6",
            IssueAwardType.Universal => "7",
            _ => null
        };

        List<KeyValuePair<string, string>> payload =
            [
                new("action", "addAwards"),
                new("awards[]", award)
            ];

        foreach (string student in studentSentralIds)
            payload.Add(new("students[]", student));

        payload.Add(new("date", _dateTime.Today.ToString("yyyy-MM-dd")));

        HtmlDocument result = await GetPageByPost(new($"{_settings.ServerUrl}/wellbeing/awards/new"), payload, CancellationToken.None);
        DateTime current = _dateTime.Now;

        if (result is null)
            return Result.Failure<DateTime>(SentralGatewayErrors.IncorrectResponseFromServer);

        return current;
    }

    public async Task<string> GetSentralStudentIdAsync(string studentName)
    {
        if (_logOnly)
        {
            _logger.Information("GetSentralStudentIdAsync: studentName={studentName}", studentName);

            return null;
        }

        _studentListPage ??= await GetPageByGet($"{_settings.ServerUrl}/profiles/main/search?eduproq=&search=advanced&plan_type=plans", default);

        if (_studentListPage == null)
            return null;

        HtmlDocument page = _studentListPage;

        HtmlNode studentTable = page.DocumentNode.SelectSingleNode(_settings.XPaths.StudentTable);

        if (studentTable != null)
        {
            foreach (HtmlNode row in studentTable.Descendants("tr"))
            {
                HtmlNode cell = row.ChildNodes.FindFirst("td");
                string sentralName = cell.InnerText.Trim();
                string sentralFirst = sentralName.Split(',')[1].Trim().ToLower();
                string sentralLast = sentralName.Split(',')[0].Trim().ToLower();

                if (studentName.ToLower() == $"{sentralFirst} {sentralLast}")
                {
                    string href = cell.ChildNodes.FindFirst("a").GetAttributeValue("href", "");
                    if (string.IsNullOrWhiteSpace(href))
                    {
                        // Something went wrong? What now?
                        throw new NodeAttributeNotFoundException();
                    }
                    else
                    {
                        return href.Split('/').Last();
                    }
                }
            }
        }

        return null;
    }

    public async Task<ICollection<SentralReportDto>> GetStudentReportList(string sentralStudentId)
    {
        if (_logOnly)
        {
            _logger.Information("GetStudentReportList: sentralStudentId={sentralStudentId}", sentralStudentId);

            return new List<SentralReportDto>();
        }

        HtmlNode reportTable = null;
        HtmlDocument page = await GetPageByGet($"{_settings.ServerUrl}/profiles/students/{sentralStudentId}/academic-history", default);

        List<SentralReportDto> dataList = new();

        if (page == null)
            return dataList;

        HtmlNodeCollection menuItems = page.DocumentNode.SelectNodes("//*[@id='reporting_period']/option");

        if (menuItems == null || menuItems.Count == 0)
            return dataList;

        foreach (HtmlNode menuItem in menuItems)
        {
            string linkRef = menuItem.GetAttributeValue("value", "");
            if (string.IsNullOrWhiteSpace(linkRef))
                continue;

            HtmlDocument reportPage = await GetPageByGet($"{_settings.ServerUrl}/profiles/students/{sentralStudentId}/academic-history?type=sreport&page=printed_report&reporting_period={linkRef}", default);
            reportTable = reportPage.DocumentNode.SelectSingleNode("//*[@id='layout-2col-content']/div/div/div[2]/table/tbody");

            if (reportTable != null)
                break;
        }

        if (reportTable != null)
        {
            foreach (HtmlNode row in reportTable.Descendants("tr"))
            {
                int cellNumber = 0;

                SentralReportDto entry = new();

                // Process Row!
                foreach (HtmlNode cell in row.Descendants("td"))
                {
                    cellNumber++;

                    switch (cellNumber)
                    {
                        case 1:
                            // Report Period Name
                            entry.Name = cell.InnerText.Trim();
                            break;
                        case 2:
                            // Report Semester
                            break;
                        case 3:
                            // Report Year
                            entry.Year = cell.InnerText.Trim();
                            break;
                        case 4:
                            // Report Layout
                            break;
                        case 5:
                            // Report Download link
                            string link = cell.FirstChild.GetAttributeValue("onclick", "downloadFile(0)");
                            entry.PublishId = link.Split('(')[1].Split(')')[0];
                            break;
                        default:
                            break;
                    }
                }

                dataList.Add(entry);
            }
        }

        return dataList;
    }

    public async Task<byte[]> GetStudentReport(string sentralStudentId, string reportId)
    {
        if (_logOnly)
        {
            _logger.Information("GetStudentReport: sentralStudentId={sentralStudentId}, reportId={reportId}", sentralStudentId, reportId);

            return null;
        }

        List<KeyValuePair<string, string>> formData = new()
        {
            new KeyValuePair<string, string>("file_id", reportId),
            new KeyValuePair<string, string>("action", "download_file")
        };
        
        byte[] response = await GetByteArrayByPost($"{_settings.ServerUrl}/profiles/students/{sentralStudentId}/academic-history?type=sreport&page=printed_report", formData, default);

        return response;
    }

    public async Task<byte[]> GetSentralStudentPhoto(string studentId)
    {
        if (_logOnly)
        {
            _logger.Information("GetSentralStudentPhoto: studentId={studentId}", studentId);

            return null;
        }

        byte[] response = await GetByteArrayByGet($"{_settings.ServerUrl}/_common/lib/photo?type=student&id={studentId}&w=250&h=250", default);

        return response;
    }

    public async Task<IndigenousStatus> GetStudentIndigenousStatus(string sentralStudentId)
    {
        if (_logOnly)
        {
            _logger.Information("GetStudentIndigenousStatus: sentralStudentId={sentralStudentId}", sentralStudentId);

            return IndigenousStatus.Unknown;
        }

        HtmlDocument page = await GetPageByGet($"{_settings.ServerUrl}/profiles/students/{sentralStudentId}/overview", CancellationToken.None);

        if (page is null)
            return IndigenousStatus.Unknown;

        //HtmlNode atsiField = page.DocumentNode.SelectSingleNode("/html/body/div[8]/div/div[4]/div[2]/div/div/div[1]/div[2]/div/table/tr/td[1]/table/tr[7]/td");
        HtmlNode atsiField = page.DocumentNode.SelectSingleNode("//*[@id=\"expander-content-1\"]/table/tr/td[1]/table/tr[7]/td");

        if (atsiField is null)
            return IndigenousStatus.Unknown;

        string atsiValue = atsiField.InnerText.Trim();

        return atsiValue switch
        {
            "Aboriginal but not Torres Strait Islander Origin" => IndigenousStatus.AboriginalButNotTorresStraitIslander,
            "Torres Strait Islander but Not Aboriginal Origin" => IndigenousStatus.TorresStraitIslanderButNotAboriginal,
            "Both Torres Strait and Aboriginal Origin" => IndigenousStatus.BothAboriginalAndTorresStraitIslander,
            "" => IndigenousStatus.NeitherAboriginalNorTorresStraitIslander,
            _ => IndigenousStatus.Unknown
        };
    }

    public async Task<string> GetSentralStudentIdFromSRN(string srn, string grade)
    {
        if (_logOnly)
        {
            _logger.Information("GetSentralStudentIdFromSRN: srn={srn}, grade={grade}", srn, grade);

            return null;
        }

        HtmlDocument page = await GetPageByGet($"{_settings.ServerUrl}/admin/datasync/students?year={grade}&type=active", default);

        // OLD XPATH - CHANGED 2023-02-24
        //var studentTable = page.DocumentNode.SelectSingleNode("/html/body/div[6]/div/div/div[3]/div/div/div/div[2]/table");
        
        // OLD XPATH - CHANGED 2024-02-02
        //HtmlNode studentTable = page?.DocumentNode.SelectSingleNode("/html/body/div[7]/div/div/div[3]/div/div/div/div[2]/table");
        
        HtmlNode studentTable = page?.DocumentNode.SelectSingleNode("/html/body/div[8]/div/div[2]/div[3]/div/div/div/div[2]/table");
        
        if (studentTable == null) return null;
        
        foreach (HtmlNode row in studentTable.Descendants("tr"))
        {
            HtmlNode cell = row.ChildNodes.FindFirst("td");
            if (cell == null)
                continue;

            string sentralSRN = cell.InnerText.Trim();

            if (sentralSRN != srn) continue;
                
            string href = cell.ChildNodes.FindFirst("a").GetAttributeValue("href", "");
            if (!string.IsNullOrWhiteSpace(href))
            {
                return href.Split('=').Last();
            }
        }

        return null;
    }

    public async Task<List<SentralPeriodAbsenceDto>> GetAbsenceDataAsync(string sentralStudentId)
    {
        if (_logOnly)
        {
            _logger.Information("GetAbsenceDataAsync: sentralStudentId={sentralStudentId}", sentralStudentId);

            return new List<SentralPeriodAbsenceDto>();
        }

        HtmlDocument page = await GetPageByGet($"{_settings.ServerUrl}/attendancepxp/administration/student?id={sentralStudentId}", default);

        if (page == null)
            return new List<SentralPeriodAbsenceDto>();

        HtmlNode absenceTable = page.DocumentNode.SelectSingleNode(_settings.XPaths.AbsenceTable);

        List<SentralPeriodAbsenceDto> absences = new();

        if (absenceTable != null)
        {
            IEnumerable<HtmlNode> rows = absenceTable.Descendants("tr");
            DateOnly previousDate = new();

            foreach (HtmlNode row in rows)
            {
                HtmlNode dateCell = row.ChildNodes.FindFirst("td");
                string stringDate = dateCell.InnerText.Trim();
                DateOnly date;

                if (stringDate == "No period absences have been recorded for this student.")
                    return absences;

                if (string.IsNullOrWhiteSpace(stringDate) || stringDate == "&nbsp;")
                {
                    if (previousDate == DateOnly.MinValue)
                    {
                        continue;
                    }

                    //stringDate = previousDate.ToString("dd-MM-yyyy");
                    date = previousDate;
                }
                else
                {
                    date = DateOnly.Parse(stringDate);
                    previousDate = date;
                }

                SentralPeriodAbsenceDto periodAbsence = new()
                {
                    Date = date
                };

                int cellNumber = 0;
                // Process Row!
                foreach (HtmlNode cell in row.Descendants("td"))
                {
                    cellNumber++;

                    switch (cellNumber)
                    {
                        case 1:
                        case 6:
                        case 7:
                            break;
                        case 2:
                            string[] periodsText = cell.InnerText.Trim().Split(' ');
                            periodAbsence.Period = periodsText[0].Trim();
                            periodAbsence.ClassName = periodsText[2].Trim();
                            break;
                        case 3:
                            string absenceTypeText = cell.InnerText.Trim();
                            switch (absenceTypeText[..4])
                            {
                                case "Abse":
                                    periodAbsence.Type = SentralPeriodAbsenceDto.Whole;
                                    break;
                                default:
                                    if (!absenceTypeText.Contains('('))
                                    {
                                        // What the hell happened here? This shouldn't happen!
                                    }
                                    else
                                    {
                                        // Partial absence, but for how long?
                                        periodAbsence.Type = SentralPeriodAbsenceDto.Partial;
                                        string stringMinutes = absenceTypeText.Split('(')[1].Split(')')[0];
                                        periodAbsence.MinutesAbsent = int.Parse(stringMinutes);
                                        periodAbsence.PartialType = absenceTypeText.Split('(')[0].Trim();
                                    }
                                    break;
                            }
                            break;
                        case 4:
                            periodAbsence.Reason = cell.InnerText.Trim();
                            break;
                        case 5:
                            if (string.IsNullOrWhiteSpace(periodAbsence.Reason))
                                periodAbsence.Reason = cell.InnerText.Trim();
                            break;
                        case 8:
                            // Last cell, so do we have a valid PeriodAbsence object?
                            if (periodAbsence.IsValid())
                            {
                                absences.Add(periodAbsence);
                            }
                            break;
                    }
                }
            }
        }

        return absences;
    }

    public async Task<Result<List<SentralPeriodAbsenceDto>>> GetAbsenceDataAsync(
        string sentralStudentId, 
        string year, 
        CancellationToken cancellationToken = default)
    {
        if (_logOnly)
        {
            _logger
                .ForContext(nameof(sentralStudentId), sentralStudentId)
                .ForContext(nameof(year), year)
                .Information("GetAbsenceDataAsync");

            return new List<SentralPeriodAbsenceDto>();
        }
        
        List<SentralPeriodAbsenceDto> absences = new();
        
        for (int term = 1; term < 5; term++)
        {
            HtmlDocument page = await GetPageByGet($"{_settings.ServerUrl}/attendancepxp/administration/student?id={sentralStudentId}?term={term}&year={year}", cancellationToken);

            if (page == null)
                return Result.Failure<List<SentralPeriodAbsenceDto>>(SentralGatewayErrors.IncorrectResponseFromServer);

            HtmlNode absenceTable = page.DocumentNode.SelectSingleNode(_settings.XPaths.AbsenceTable);
            
            if (absenceTable != null)
            {
                IEnumerable<HtmlNode> rows = absenceTable.Descendants("tr");
                DateOnly previousDate = new();

                foreach (HtmlNode row in rows)
                {
                    HtmlNode dateCell = row.ChildNodes.FindFirst("td");
                    string stringDate = dateCell.InnerText.Trim();
                    DateOnly date;

                    if (stringDate == "No period absences have been recorded for this student.")
                        continue;

                    if (string.IsNullOrWhiteSpace(stringDate) || stringDate == "&nbsp;")
                    {
                        if (previousDate == DateOnly.MinValue)
                        {
                            continue;
                        }

                        //stringDate = previousDate.ToString("dd-MM-yyyy");
                        date = previousDate;
                    }
                    else
                    {
                        date = DateOnly.Parse(stringDate);
                        previousDate = date;
                    }

                    SentralPeriodAbsenceDto periodAbsence = new()
                    {
                        Date = date
                    };

                    int cellNumber = 0;
                    // Process Row!
                    foreach (HtmlNode cell in row.Descendants("td"))
                    {
                        cellNumber++;

                        switch (cellNumber)
                        {
                            case 1:
                            case 6:
                            case 7:
                                break;
                            case 2:
                                string[] periodsText = cell.InnerText.Trim().Split(' ');
                                periodAbsence.Period = periodsText[0].Trim();
                                periodAbsence.ClassName = periodsText[2].Trim();
                                break;
                            case 3:
                                string absenceTypeText = cell.InnerText.Trim();
                                switch (absenceTypeText[..4])
                                {
                                    case "Abse":
                                        periodAbsence.Type = SentralPeriodAbsenceDto.Whole;
                                        break;
                                    default:
                                        if (!absenceTypeText.Contains('('))
                                        {
                                            // What the hell happened here? This shouldn't happen!
                                        }
                                        else
                                        {
                                            // Partial absence, but for how long?
                                            periodAbsence.Type = SentralPeriodAbsenceDto.Partial;
                                            string stringMinutes = absenceTypeText.Split('(')[1].Split(')')[0];
                                            periodAbsence.MinutesAbsent = int.Parse(stringMinutes);
                                            periodAbsence.PartialType = absenceTypeText.Split('(')[0].Trim();
                                        }
                                        break;
                                }
                                break;
                            case 4:
                                periodAbsence.Reason = cell.InnerText.Trim();
                                break;
                            case 5:
                                if (string.IsNullOrWhiteSpace(periodAbsence.Reason))
                                    periodAbsence.Reason = cell.InnerText.Trim();
                                break;
                            case 8:
                                // Last cell, so do we have a valid PeriodAbsence object?
                                if (periodAbsence.IsValid())
                                {
                                    absences.Add(periodAbsence);
                                }
                                break;
                        }
                    }
                }
            }
        }

        return absences;
    }

    public async Task<Dictionary<StudentReferenceNumber, List<SentralPeriodAbsenceDto>>> GetAttendanceModuleAbsenceDataForSchool(
        CancellationToken cancellationToken = default)
    {
        Dictionary<StudentReferenceNumber, List<SentralPeriodAbsenceDto>> data = new();

        if (_logOnly)
        {
            _logger.Information("GetAttendanceModuleAbsenceDataForSchool");

            return data;
        }

        Uri filePath = new Uri($"{_settings.ServerUrl}/attendance/reports/absences");

        List<KeyValuePair<string, string>> formData = new()
        {
            new KeyValuePair<string, string>("length", "year"),
            new KeyValuePair<string, string>("year", _dateTime.CurrentYear.ToString()),
            new KeyValuePair<string, string>("absence_display", "code"),
            new KeyValuePair<string, string>("absence_types", "all"),
            new KeyValuePair<string, string>("reasons[]", "1"),
            new KeyValuePair<string, string>("reasons[]", "2"),
            new KeyValuePair<string, string>("reasons[]", "3"),
            new KeyValuePair<string, string>("reasons[]", "4"),
            new KeyValuePair<string, string>("reasons[]", "5"),
            new KeyValuePair<string, string>("reasons[]", "6"),
            new KeyValuePair<string, string>("reasons[]", "7"),
            new KeyValuePair<string, string>("reasons[]", "8"),
            new KeyValuePair<string, string>("reasons[]", "9"),
            new KeyValuePair<string, string>("reasons[]", "10"),
            new KeyValuePair<string, string>("group_absences", "date"),
            new KeyValuePair<string, string>("group", "years"),
            new KeyValuePair<string, string>("years[]", "5"),
            new KeyValuePair<string, string>("years[]", "6"),
            new KeyValuePair<string, string>("years[]", "7"),
            new KeyValuePair<string, string>("years[]", "8"),
            new KeyValuePair<string, string>("years[]", "9"),
            new KeyValuePair<string, string>("years[]", "10"),
            new KeyValuePair<string, string>("years[]", "11"),
            new KeyValuePair<string, string>("years[]", "12"),
            new KeyValuePair<string, string>("action", "export")
        };

        Stream completePage = await GetStreamByPost(filePath, formData, cancellationToken);

        if (completePage is null)
            return data;

        using IExcelDataReader completeReader = ExcelReaderFactory.CreateReader(completePage);
        DataSet completeWorksheet = completeReader.AsDataSet();

        foreach (DataRow row in completeWorksheet.Tables[0].Rows)
        {
            if (row[0].ToString() == "Student ID") // This is a header row
                continue;

            string srn = row[0].ToString().FormatField();
            Result<StudentReferenceNumber> studentReferenceNumber = StudentReferenceNumber.Create(srn);
            if (studentReferenceNumber.IsFailure)
            {
                _logger
                    .ForContext(nameof(StudentReferenceNumber), srn)
                    .ForContext(nameof(Error), studentReferenceNumber.Error, true)
                    .Information("Error parsing SRN to StudentReferenceNumber object");
            }

            SentralPeriodAbsenceDto absence = new();
            string stringDate = row[2].ToString().FormatField();
            DateOnly rowDate = DateOnly.ParseExact(stringDate, "yyyy-MM-dd");
            absence.Date = rowDate;
            absence.Reason = row[9].ToString().FormatField();

            absence.Timeframe = row[10].ToString().FormatField();
            if (string.IsNullOrWhiteSpace(absence.Timeframe))
            {
                absence.WholeDay = true;
            }
            else
            {
                bool startTimeSuccess = TimeOnly.TryParseExact(absence.Timeframe.Split(' ')[0], "h:mmtt", CultureInfo.InvariantCulture, DateTimeStyles.None, out TimeOnly startTime);
                if (startTimeSuccess)
                    absence.StartTime = startTime;
                else
                    _logger
                        .ForContext("DetectedTime", absence.Timeframe.Split(' ')[0])
                        .ForContext("AbsenceDate", absence.Date)
                        .ForContext(nameof(StudentReferenceNumber), studentReferenceNumber)
                        .Information("Error parsing absence start time to TimeOnly object");

                bool endTimeSuccess = TimeOnly.TryParseExact(absence.Timeframe.Split(' ')[2], "h:mmtt", CultureInfo.InvariantCulture, DateTimeStyles.None, out TimeOnly endTime);
                if (endTimeSuccess)
                    absence.EndTime = endTime;
                else
                    _logger
                        .ForContext("DetectedTime", absence.Timeframe.Split(' ')[0])
                        .ForContext("AbsenceDate", absence.Date)
                        .ForContext(nameof(StudentReferenceNumber), studentReferenceNumber)
                        .Information("Error parsing absence end time to TimeOnly object");
            }

            string comment = row[11].ToString().FormatField();
            if (!string.IsNullOrWhiteSpace(comment))
            {
                string explainer = row[12].ToString().FormatField();
                if (string.IsNullOrWhiteSpace(explainer))
                {
                    absence.ExternalExplanation = comment;
                }
                else
                {
                    string explainerSource = row[13].ToString().FormatField();
                    absence.ExternalExplanation = comment;
                    absence.ExternalExplanationSource = string.IsNullOrWhiteSpace(explainerSource) 
                        ? explainer 
                        : $"{explainer} via {explainerSource}";
                }
            }

            if (data.TryGetValue(studentReferenceNumber.Value, out List<SentralPeriodAbsenceDto> record))
            {
                record.Add(absence);
            }
            else
            {
                data.Add(studentReferenceNumber.Value, [ absence ]);
            }
        }

        return data;
    }

    public async Task<List<SentralPeriodAbsenceDto>> GetPartialAbsenceDataAsync(string sentralStudentId)
    {
        List<SentralPeriodAbsenceDto> term1 = await GetPartialAbsenceDataForTerm(sentralStudentId, 1);
        List<SentralPeriodAbsenceDto> term2 = await GetPartialAbsenceDataForTerm(sentralStudentId, 2);
        List<SentralPeriodAbsenceDto> term3 = await GetPartialAbsenceDataForTerm(sentralStudentId, 3);
        List<SentralPeriodAbsenceDto> term4 = await GetPartialAbsenceDataForTerm(sentralStudentId, 4);

        List<SentralPeriodAbsenceDto> returnData = new();
        returnData.AddRange(term1);
        returnData.AddRange(term2);
        returnData.AddRange(term3);
        returnData.AddRange(term4);

        return returnData;
    }

    private async Task<List<SentralPeriodAbsenceDto>> GetPartialAbsenceDataForTerm(string sentralStudentId, int term)
    {
        if (_logOnly)
        {
            _logger.Information("GetPartialAbsenceDataForTerm: sentralStudentId={sentralStudentId}, term={term}", sentralStudentId, term);

            return new List<SentralPeriodAbsenceDto>();
        }

        HtmlDocument page = await GetPageByGet($"{_settings.ServerUrl}/attendance/administration/student/{sentralStudentId}?term={term}", default);

        if (page == null)
            return new List<SentralPeriodAbsenceDto>();

        HtmlNode absenceTable = page.DocumentNode.SelectSingleNode(_settings.XPaths.PartialAbsenceTable);

        List<SentralPeriodAbsenceDto> detectedAbsences = new();

        if (absenceTable != null)
        {
            IEnumerable<HtmlNode> rows = absenceTable.Descendants("tr");

            foreach (HtmlNode row in rows)
            {
                SentralPeriodAbsenceDto absence = new();
                HtmlNode firstTD = row.ChildNodes.FindFirst("td");

                // The weekly heading rows are TH with no TD tags, so we should just skip these.
                if (firstTD == null)
                    continue;

                HtmlNode input = firstTD.ChildNodes.FindFirst("input");
                IEnumerable<HtmlAttribute> value = input.Attributes.AttributesWithName("value");
                string rowData = value.FirstOrDefault()?.Value;

                // If this row has no absence data, the value is a numerical id, not a date.
                if (rowData == null || !rowData.Contains('-'))
                    continue;

                string stringDate = rowData[..rowData.IndexOf('_')];
                DateOnly rowDate = DateOnly.Parse(stringDate);
                absence.Date = rowDate;

                int cellNumber = 0;
                foreach (HtmlNode cell in row.Descendants("td"))
                {
                    cellNumber++;

                    switch (cellNumber)
                    {
                        case 4:
                            // Attendance column, contains span with Absence Reason as innerText
                            absence.Reason = cell.ChildNodes.FindFirst("span").InnerText;
                            break;
                        case 5:
                            // Time column, contains label with start and end time of absence as innerText
                            absence.Timeframe = cell.ChildNodes.FindFirst("label").InnerText;

                            if (absence.Timeframe.Contains('-'))
                            {
                                bool startTimeSuccess = TimeOnly.TryParseExact(absence.Timeframe.Split(' ')[0], "h:mmtt", CultureInfo.InvariantCulture, DateTimeStyles.None, out TimeOnly startTime);
                                if (startTimeSuccess)
                                    absence.StartTime = startTime;
                                else
                                    _logger
                                        .ForContext("DetectedTime", absence.Timeframe.Split(' ')[0])
                                        .ForContext("AbsenceDate", absence.Date)
                                        .ForContext("SentralStudentId", sentralStudentId)
                                        .Information("Error parsing absence start time to TimeOnly object");

                                bool endTimeSuccess = TimeOnly.TryParseExact(absence.Timeframe.Split(' ')[2], "h:mmtt", CultureInfo.InvariantCulture, DateTimeStyles.None, out TimeOnly endTime);
                                if (endTimeSuccess)
                                    absence.EndTime = endTime;
                                else
                                    _logger
                                        .ForContext("DetectedTime", absence.Timeframe.Split(' ')[0])
                                        .ForContext("AbsenceDate", absence.Date)
                                        .ForContext("SentralStudentId", sentralStudentId)
                                        .Information("Error parsing absence end time to TimeOnly object");
                            }
                            else
                            {
                                absence.WholeDay = true;
                            }

                            break;
                        case 6:
                            // Explanation column, contains comment and source for Sentral absences as innerText inside SPANS
                            if (cell.ChildNodes.FindFirst("span") == null)
                            {
                                // This absence either has not been explained in Sentral, or does not have an explanation comment
                                continue;
                            }

                            HtmlNodeCollection spans = cell.SelectNodes("span");

                            if (spans.Count > 1)
                            {
                                absence.ExternalExplanation = spans[0].InnerText.Trim();
                                absence.ExternalExplanationSource = spans[1].InnerText.Trim();
                            }
                            else
                            {
                                absence.ExternalExplanation = cell.ChildNodes.FindFirst("span").InnerText.Trim();
                            }

                            break;
                    }
                }

                if (!string.IsNullOrWhiteSpace(absence.Reason) && !string.IsNullOrWhiteSpace(absence.Timeframe))
                    detectedAbsences.Add(absence);
            }
        }

        return detectedAbsences;
    }

    public async Task<Result<List<DateOnly>>> GetEnrolledDatesForStudent(string sentralId, string year, DateOnly startDate, DateOnly endDate)
    {
        if (_logOnly)
        {
            _logger
                .ForContext(nameof(sentralId), sentralId)
                .ForContext(nameof(year), year)
                .ForContext(nameof(startDate), startDate)
                .ForContext(nameof(endDate), endDate)
                .Information("GetEnrolledDatesForStudent");

            return new List<DateOnly>();
        }

        HtmlDocument page = await GetPageByGet($"{_settings.ServerUrl}/attendancepxp/administration/student?id={sentralId}&year={year}", default);

        if (page == null)
        {
            return Result.Failure<List<DateOnly>>(SentralGatewayErrors.IncorrectResponseFromServer);
        }

        HtmlNodeCollection pxpRolls = page.DocumentNode.SelectNodes(@"//*[contains(@class, 'pxp-roll')]");

        List<DateOnly> enrolledDates = new();

        foreach (HtmlNode term in pxpRolls)
        {
            IEnumerable<HtmlNode> cells = term.Descendants("td");

            foreach (HtmlNode cell in cells)
            {
                if (!cell.HasClass("tips"))
                    continue;

                List<string> classes = cell.GetClasses().ToList();

                if (classes.Contains("mixed") || classes.Contains("present") || classes.Contains("absent"))
                {
                    string cellTitle = cell.GetAttributeValue<string>("title", "");

                    if (string.IsNullOrWhiteSpace(cellTitle))
                        continue;

                    int pos = cellTitle.IndexOf("::");

                    if (pos == -1)
                        continue;

                    string stringDate = cellTitle[..pos];

                    bool success = DateOnly.TryParse(stringDate, out DateOnly date);

                    if (success)
                    {
                        if (date < startDate || date > endDate)
                            continue;

                        enrolledDates.Add(date);
                    }
                }
            }
        }

        return enrolledDates;
    }

    public async Task<List<DateOnly>> GetExcludedDatesFromCalendar(string year)
    {
        if (_logOnly)
        {
            _logger.Information("GetExcludedDatesFromCalendar: year={year}", year);

            return new List<DateOnly>();
        }

        HtmlDocument page = await GetPageByGet($"{_settings.ServerUrl}/admin/settings/school/calendar/{year}/month", default);

        if (page == null)
            return new List<DateOnly>();

        HtmlNode calendarTable = page.DocumentNode.SelectSingleNode(_settings.XPaths.CalendarTable);

        List<DateOnly> nonSchoolDays = new();

        if (calendarTable == null) return nonSchoolDays.OrderBy(a => a).ToList();
        
        IEnumerable<HtmlNode> rows = calendarTable.Descendants("tr");

        foreach (HtmlNode row in rows)
        {
            IEnumerable<HtmlNode> days = row.Descendants("td");

            foreach (HtmlNode day in days)
            {
                if (!day.HasClass("school-break") && 
                    !day.HasClass("holiday") &&
                    !day.HasClass("holiday-once")) 
                    continue;

                string action = day.GetAttributeValue("onclick", "");

                if (string.IsNullOrWhiteSpace(action)) continue;
                    
                string detectedDate = action.Split('\'')[1];
                DateOnly date = DateOnly.Parse(detectedDate);

                nonSchoolDays.Add(date);
            }
        }

        return nonSchoolDays.OrderBy(a => a).ToList();
    }

    public async Task<Result<(string Week, string Term)>> GetWeekForDate(DateOnly date)
    {
        if (_logOnly)
        {
            _logger
                .Information("GetWeekForDate: date={date}", date);

            return ("1", "1");
        }

        string year = date.Year.ToString();

        HtmlDocument page = await GetPageByGet($"{_settings.ServerUrl}/admin/settings/school/calendar/{year}/term", default);

        if (page == null)
            return Result.Failure<(string, string)>(new("SentralGateway.GetPage.Failure", "Could not retrieve page from Sentral Server"));

        HtmlNode calendarTable = page.DocumentNode.SelectSingleNode(_settings.XPaths.TermCalendarTable);

        if (calendarTable != null)
        {
            IEnumerable<HtmlNode> rows = calendarTable.Descendants("tr");

            string term = "1";
            string week = "1";

            foreach (HtmlNode row in rows)
            {
                if (row.Descendants("td").Count() == 1)
                {
                    // This is a header row
                    HtmlNode header = row.Descendants("td").First();
                    HtmlNode termName = header.Descendants("b").FirstOrDefault();

                    if (termName is null)
                    {
                        // This is a blank row, skip
                        continue;
                    }

                    term = termName.InnerText.Split(' ')[1];
                    continue;
                }

                HtmlNode weekName = row.Descendants("th").FirstOrDefault();

                if (string.IsNullOrWhiteSpace(weekName.InnerText))
                    continue;

                week = weekName.InnerText;

                foreach (HtmlNode cell in row.Descendants("td"))
                {
                    string action = cell.GetAttributeValue("onclick", "");
                    if (string.IsNullOrWhiteSpace(action))
                        continue;

                    string detectedDate = action.Split('\'')[1];
                    if (DateOnly.Parse(detectedDate) == date)
                        return (week, term);
                }
            }
        }

        return Result.Failure<(string, string)>(new("SentralGateway.GetWeekForDate.NotFound", "Could not identify Term and Week from date"));
    }

    public async Task<Result<(DateOnly StartDate, DateOnly EndDate)>> GetDatesForWeek(string year, string term, string week)
    {
        if (_logOnly)
        {
            _logger
                .Information("GetDatesForWeek: year={year}, term={term}, week={week}", year, term, week);

            return (_dateTime.Today, _dateTime.Today.AddDays(12));
        }

        DateOnly startDate = DateOnly.MinValue;
        DateOnly endDate = DateOnly.MinValue;

        HtmlDocument page = await GetPageByGet($"{_settings.ServerUrl}/admin/settings/school/calendar/{year}/term", default);

        if (page == null)
            return Result.Failure<(DateOnly, DateOnly)>(new("SentralGateway.GetPage.Failure", "Could not retrieve page from Sentral Server"));

        HtmlNode calendarTable = page.DocumentNode.SelectSingleNode(_settings.XPaths.TermCalendarTable);

        if (calendarTable != null)
        {
            IEnumerable<HtmlNode> rows = calendarTable.Descendants("tr");

            bool correctTerm = false;

            foreach (HtmlNode row in rows)
            {
                if (row.Descendants("td").Count() == 1)
                {
                    // This is a header row
                    HtmlNode header = row.Descendants("td").First();
                    HtmlNode termName = header.Descendants("b").FirstOrDefault();

                    if (termName is null)
                    {
                        // This is a blank row, skip
                        continue;
                    }

                    if (termName.InnerText == $"Term {term}")
                    {
                        correctTerm = true;
                        continue;
                    }
                    else
                    {
                        correctTerm = false;
                        continue;
                    }
                }

                if (correctTerm == true)
                {
                    HtmlNode weekName = row.Descendants("th").FirstOrDefault();

                    if (weekName?.InnerText == week)
                    {
                        HtmlNode monday = row.Descendants("td").First();

                        string mondayAction = monday.GetAttributeValue("onclick", "");
                        if (!string.IsNullOrWhiteSpace(mondayAction))
                        {
                            string detectedDate = mondayAction.Split('\'')[1];
                            DateOnly date = DateOnly.Parse(detectedDate);

                            startDate = date;
                        }

                        HtmlNode friday = row.Descendants("td").Last();

                        string fridayAction = friday.GetAttributeValue("onclick", "");
                        if (!string.IsNullOrWhiteSpace(fridayAction))
                        {
                            string detectedDate = fridayAction.Split('\'')[1];
                            DateOnly date = DateOnly.Parse(detectedDate);

                            endDate = date;
                        }
                    }

                    if (startDate != DateOnly.MinValue && endDate != DateOnly.MinValue)
                        return (startDate, endDate);
                }
            }
        }

        return Result.Failure<(DateOnly, DateOnly)>(new("SentralGateway.GetPage.Incorrect", "Sentral Server page did not include calendar table"));
    }

    public async Task<List<ValidAttendenceReportDate>> GetValidAttendanceReportDatesFromCalendar(string year)
    {
        if (_logOnly)
        {
            _logger.Information("GetValidAttendanceReportDatesFromCalendar: year={year}", year);

            return new List<ValidAttendenceReportDate>();
        }

        List<ValidAttendenceReportDate> validDates = new();

        HtmlDocument page = await GetPageByGet($"{_settings.ServerUrl}/admin/settings/school/calendar/{year}/term", default);

        if (page == null)
            return validDates;

        HtmlNode calendarTable = page.DocumentNode.SelectSingleNode(_settings.XPaths.TermCalendarTable);

        if (calendarTable != null)
        {
            string TermName = "";
            string WeekName = "";

            List<HtmlNode> rows = calendarTable.Descendants("tr").ToList();

            for (int row = 0; row < rows.Count - 1; row++)
            {
                HtmlNode firstChildNode = rows[row].ChildNodes.Where(node => node.Name != "#text").First();

                if (firstChildNode.Name == "td" && firstChildNode.GetAttributeValue("colspan", 0) > 1)
                {
                    // This is a header row with the term name
                    IEnumerable<HtmlNode> nodes = firstChildNode.Descendants("b");
                    foreach (HtmlNode node in nodes)
                    {
                        if (node.InnerText.Contains("Term"))
                        {
                            TermName = node.InnerText.Trim();
                        }
                    }
                }

                if (firstChildNode.Name == "th")
                {
                    if (firstChildNode.InnerText.Trim() == "11")
                    {
                        // This is a calendar row starting with the week number
                        WeekName = $"Week {firstChildNode.InnerText.Trim()}";

                        DateOnly startDate = new();
                        DateOnly endDate = new();

                        string startDateAction = rows[row].ChildNodes.Where(node => node.Name != "#text").ToArray()[1].GetAttributeValue("onclick", "");
                        if (!string.IsNullOrWhiteSpace(startDateAction))
                        {
                            string detectedDate = startDateAction.Split('\'')[1];
                            DateOnly date = DateOnly.Parse(detectedDate);

                            startDate = date;
                        }

                        string endDateAction = rows[row].ChildNodes.Where(node => node.Name != "#text").ToArray()[5].GetAttributeValue("onclick", "");
                        if (!string.IsNullOrWhiteSpace(endDateAction))
                        {
                            string detectedDate = endDateAction.Split('\'')[1];
                            DateOnly date = DateOnly.Parse(detectedDate);

                            endDate = date;
                        }

                        validDates.Add(new ValidAttendenceReportDate(
                            TermName,
                            startDate.ToDateTime(TimeOnly.MinValue),
                            endDate.ToDateTime(TimeOnly.MinValue),
                            $"{TermName} {WeekName}"));
                    }
                    else
                    {
                        // This is a calendar row starting with the week number
                        WeekName = $"Week {firstChildNode.InnerText.Trim()} - Week {rows[row + 1].ChildNodes.Where(node => node.Name != "#text").First().InnerText.Trim()}";
                        
                        DateOnly startDate = new();
                        DateOnly endDate = new();

                        string startDateAction = rows[row].ChildNodes.Where(node => node.Name != "#text").ToArray()[1].GetAttributeValue("onclick", "");
                        if (!string.IsNullOrWhiteSpace(startDateAction))
                        {
                            string detectedDate = startDateAction.Split('\'')[1];
                            DateOnly date = DateOnly.Parse(detectedDate);

                            startDate = date;
                        }

                        string endDateAction = rows[row + 1].ChildNodes.Where(node => node.Name != "#text").ToArray()[5].GetAttributeValue("onclick", "");
                        if (!string.IsNullOrWhiteSpace(endDateAction))
                        {
                            string detectedDate = endDateAction.Split('\'')[1];
                            DateOnly date = DateOnly.Parse(detectedDate);

                            endDate = date;
                        }

                        validDates.Add(new ValidAttendenceReportDate(
                            TermName,
                            startDate.ToDateTime(TimeOnly.MinValue),
                            endDate.ToDateTime(TimeOnly.MinValue),
                            $"{TermName} {WeekName}"));

                        row++;
                    }
                }
            }
        }

        return validDates.OrderBy(date => date.StartDate).ToList();
    }

    public async Task<Dictionary<string, List<string>>> GetFamilyGroupings()
    {
        if (_logOnly)
        {
            _logger.Information("GetFamilyGroupings");

            return null;
        }

        Dictionary<string, List<string>> data = new();

        Stream completePage =
            await GetStreamByGet(
                $"{_settings.ServerUrl}/enquiry/export/view_export?name=complete&inputs[class]=&inputs[roll_class]=&inputs[schyear]=&format=xls&headings=1&action=Download",
                default);

        if (completePage is null)
            return data;

        using IExcelDataReader completeReader = ExcelReaderFactory.CreateReader(completePage);
        DataSet completeWorksheet = completeReader.AsDataSet();

        foreach (DataRow row in completeWorksheet.Tables[0].Rows)
        {
            if (row.ItemArray.First()?.ToString() == "STUDENT_ID") // This is a header row
                continue;

            string familyId = row[4].ToString().FormatField();
            string studentReferenceNumber = row[0].ToString().FormatField();

            if (data.ContainsKey(familyId))
            {
                data[familyId].Add(studentReferenceNumber);
            }
            else
            {
                data.Add(familyId, new List<string>() { studentReferenceNumber });
            }
        }

        return data;
    }

    public async Task<FamilyDetailsDto> GetParentContactEntry(string sentralStudentId)
    {
        if (_logOnly)
        {
            _logger.Information("GetParentContactEntry: sentralStudentId={sentralStudentId}", sentralStudentId);

            return null;
        }

        FamilyDetailsDto result = new();

        if (string.IsNullOrWhiteSpace(sentralStudentId))
            return null;

        HtmlDocument page = await GetPageByGet($"{_settings.ServerUrl}/profiles/students/{sentralStudentId}/family", default);

        if (page == null)
            return null;

        string familyEmail = page.DocumentNode.SelectSingleNode(_settings.XPaths.FamilyEmail).InnerText.Trim().ToLowerInvariant();
        
        string familyAddressBlock = HttpUtility.HtmlDecode(page.DocumentNode.SelectSingleNode(_settings.XPaths.FamilyName).InnerHtml.Trim());
        string familyName = familyAddressBlock.Split('<')[0].Trim();
        string addressLine1 = familyAddressBlock.Split('>')[1].Split('<')[0].Trim();
        string addressLine2 = familyAddressBlock.Split('>').Last().Trim();

        string postCode = addressLine2.Split(' ').Last().Trim();
        int postCodePosition = addressLine2.IndexOf(postCode);
        addressLine2 = postCodePosition < 0
            ? addressLine2
            : addressLine2.Remove(postCodePosition).Trim();

        string state = addressLine2.Split(' ').Last().Trim();
        int statePosition = addressLine2.IndexOf(state);
        addressLine2 = statePosition < 0
            ? addressLine2
            : addressLine2.Remove(statePosition).Trim();

        string town = addressLine2.Trim();

        result.FamilyEmail = familyEmail;
        result.AddressName = familyName;
        result.AddressLine1 = addressLine1;

        if (addressLine1.Contains('"'))
        {
            string line1 = addressLine1.Split('"')[1].Trim();
            string line2 = addressLine1.Split('"').Last().Trim();

            result.AddressLine1 = line1;
            result.AddressLine2 = string.IsNullOrWhiteSpace(line2) ? string.Empty : line2;
        }
        else
        {
            result.AddressLine2 = string.Empty;
        }
        result.AddressTown = town;
        result.AddressState = state;
        result.AddressPostCode = postCode;

        string parent1NameBlock = HttpUtility.HtmlDecode(page.DocumentNode.SelectSingleNode(_settings.XPaths.Parent1Name).InnerHtml.Trim());
        string parent1Type = parent1NameBlock.Split('>')[1].Trim().Split('<')[0].Trim().Split(' ').Last().Trim(':', ' ');
        string parent1Name = parent1NameBlock.Split('>').Last().Trim();
        string parent1Email = page.DocumentNode.SelectSingleNode(_settings.XPaths.Parent1Email).InnerText.Trim().ToLowerInvariant();
        string parent1Mobile = page.DocumentNode.SelectSingleNode(_settings.XPaths.Parent1Mobile).InnerText.Trim();

        string parent2NameBlock = HttpUtility.HtmlDecode(page.DocumentNode.SelectSingleNode(_settings.XPaths.Parent2Name).InnerHtml.Trim());
        string parent2Type = parent2NameBlock.Split('>')[1].Trim().Split('<')[0].Trim().Split(' ').Last().Trim(':', ' ');
        string parent2Name = parent2NameBlock.Split('>').Last().Trim();
        string parent2Email = page.DocumentNode.SelectSingleNode(_settings.XPaths.Parent2Email).InnerText.Trim().ToLowerInvariant();
        string parent2Mobile = page.DocumentNode.SelectSingleNode(_settings.XPaths.Parent2Mobile).InnerText.Trim();

        if (!string.IsNullOrWhiteSpace(parent1Name))
        {
            bool referenceSuccess = Enum.TryParse(parent1Type, out Parent.SentralReference sentralReference);

            string title = parent1Name.Split(' ')[0].Trim();
            int titlePosition = parent1Name.IndexOf(title);
            parent1Name = titlePosition < 0
                ? parent1Name
                : parent1Name.Remove(titlePosition, title.Length).Trim();

            string lastName = parent1Name.Split(' ').Last().Trim();
            int lastNamePosition = parent1Name.IndexOf(lastName);
            parent1Name = lastNamePosition < 0
                ? parent1Name
                : parent1Name.Remove(lastNamePosition).Trim();

            if (referenceSuccess)
            {
                result.Contacts.Add(new()
                {
                    Title = title,
                    LastName = lastName,
                    FirstName = parent1Name,
                    SentralReference = sentralReference,
                    Mobile = parent1Mobile,
                    Email = parent1Email
                });
            }
            else
            {
                result.Contacts.Add(new()
                {
                    Title = title,
                    LastName = lastName,
                    FirstName = parent1Name,
                    SentralReference = Parent.SentralReference.Other,
                    Mobile = parent1Mobile,
                    Email = parent1Email
                });
            }
        }

        if (!string.IsNullOrWhiteSpace(parent2Name))
        {
            bool referenceSuccess = Enum.TryParse(parent2Type, out Parent.SentralReference sentralReference);

            string title = parent2Name.Split(' ')[0].Trim();
            int titlePosition = parent2Name.IndexOf(title);
            parent2Name = titlePosition < 0
                ? parent2Name
                : parent2Name.Remove(titlePosition, title.Length).Trim();

            string lastName = parent2Name.Split(' ').Last().Trim();
            int lastNamePosition = parent2Name.IndexOf(lastName);
            parent2Name = lastNamePosition < 0
                ? parent2Name
                : parent2Name.Remove(lastNamePosition).Trim();

            if (referenceSuccess)
            {
                result.Contacts.Add(new()
                {
                    Title = title,
                    LastName = lastName,
                    FirstName = parent2Name,
                    SentralReference = sentralReference,
                    Mobile = parent2Mobile,
                    Email = parent2Email
                });
            }
            else
            {
                result.Contacts.Add(new()
                {
                    Title = title,
                    LastName = lastName,
                    FirstName = parent2Name,
                    SentralReference = Parent.SentralReference.Other,
                    Mobile = parent2Mobile,
                    Email = parent2Email
                });
            }
        }

        return result;
    }

    public async Task<ICollection<RollMarkReportDto>> GetRollMarkingReportAsync(DateOnly date)
    {
        if (_logOnly)
        {
            _logger.Information("GetRollMarkingReportAsync: date={date}", date);

            return new List<RollMarkReportDto>();
        }

        string sentralDate = date.ToString("yyyy-MM-dd");

        HtmlDocument primaryPage = await GetPageByGet($"{_settings.ServerUrl}/attendancepxp/period/administration/roll_report?campus_id=1&range=single_day&date={sentralDate}&export=1", default);
        HtmlDocument secondaryPage = await GetPageByGet($"{_settings.ServerUrl}/attendancepxp/period/administration/roll_report?campus_id=2&range=single_day&date={sentralDate}&export=1", default);

        if (primaryPage == null || secondaryPage == null)
            return new List<RollMarkReportDto>();

        List<string> primaryList = new();
        if (!primaryPage.DocumentNode.InnerHtml.StartsWith("<"))
            primaryList = primaryPage.DocumentNode.InnerHtml.Split('\u000A').ToList();

        List<string> secondaryList = new();
        if (!secondaryPage.DocumentNode.InnerHtml.StartsWith("<"))
            secondaryList = secondaryPage.DocumentNode.InnerHtml.Split('\u000A').ToList();

        List<RollMarkReportDto> list = new();

        foreach (string entry in primaryList)
        {
            string[] splitString = Regex.Split(entry, "[,]{1}(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))");
            
            if (splitString[0] == "\"Date\"" || splitString.Length != 7)
                continue;

            list.Add(new RollMarkReportDto
            {
                Date = DateTime.Parse(splitString[0].TrimStart('"').TrimEnd('"')),
                Period = splitString[1].TrimStart('"').TrimEnd('"'),
                ClassName = splitString[2].TrimStart('"').TrimEnd('"'),
                Teacher = splitString[3].TrimStart('"').TrimEnd('"'),
                Year = splitString[4].TrimStart('"').TrimEnd('"'),
                Room = splitString[5].TrimStart('"').TrimEnd('"'),
                Submitted = splitString[6].TrimStart('"').TrimEnd('"') == "Submitted"
            });
        }

        foreach (string entry in secondaryList)
        {
            string[] splitString = entry.Split(',');

            if (splitString[0] == "\"Date\"" || splitString.Length != 7)
                continue;

            list.Add(new RollMarkReportDto
            {
                Date = DateTime.Parse(splitString[0].TrimStart('"').TrimEnd('"')),
                Period = splitString[1].TrimStart('"').TrimEnd('"'),
                ClassName = splitString[2].TrimStart('"').TrimEnd('"'),
                Teacher = splitString[3].TrimStart('"').TrimEnd('"'),
                Year = splitString[4].TrimStart('"').TrimEnd('"'),
                Room = splitString[5].TrimStart('"').TrimEnd('"'),
                Submitted = splitString[6].TrimStart('"').TrimEnd('"') == "Submitted"
            });
        }

        return list;
    }

    public async Task<ICollection<FamilyDetailsDto>> GetFamilyDetailsReport(ILogger logger)
    {
        if (_logOnly)
        {
            _logger.Information("GetFamilyDetailsReport");

            return new List<FamilyDetailsDto>();
        }

        List<FamilyDetailsDto> data = new();
        
        Stream completePage = await GetStreamByGet($"{_settings.ServerUrl}/enquiry/export/view_export?name=complete&inputs[class]=&inputs[roll_class]=&inputs[schyear]=&format=xls&headings=1&action=Download", default);
        Stream emailPage = await GetStreamByGet($"{_settings.ServerUrl}/enquiry/export/view_export?name=email&inputs[only_eldest]=no&inputs[addresses]=all&format=xls&headings=1&action=Download", default);
        
        if (completePage == null || emailPage == null)
            return data;

        using IExcelDataReader completeReader = ExcelReaderFactory.CreateReader(completePage);
        DataSet completeWorksheet = completeReader.AsDataSet();
        
        foreach (DataRow row in completeWorksheet.Tables[0].Rows)
        {
            if (row.ItemArray.First()?.ToString() == "STUDENT_ID") // This is a header row
                continue;

            string familyId = row[4].ToString().FormatField();

            FamilyDetailsDto existingEntry = data.FirstOrDefault(detail => detail.FamilyId == familyId);

            if (existingEntry is not null)
            {
                string studentId = row[0].ToString().FormatField();

                if (existingEntry.StudentReferenceNumbers.All(entry => entry != studentId))
                    existingEntry.StudentReferenceNumbers.Add(studentId);
            }
            else
            {
                FamilyDetailsDto detail = new()
                {
                    FamilyId = familyId,
                    AddressName = row[83].ToString().FormatField(),
                    AddressLine1 = row[85].ToString().FormatField(),
                    AddressLine2 = row[86].ToString().FormatField(),
                    AddressTown = row[88].ToString().FormatField(),
                    AddressPostCode = row[89].ToString().FormatField()
                };

                if (!string.IsNullOrWhiteSpace(row[50].ToString().FormatField()))
                {
                    detail.Contacts.Add(new()
                    {
                        SentralId = $"{familyId}.1",
                        Sequence = 1,
                        Title = row[49].ToString().FormatField(),
                        FirstName = row[50].ToString().FormatField(),
                        LastName = row[51].ToString().FormatField(),
                        Mobile = row[53].ToString().FormatField()
                    });
                }

                if (!string.IsNullOrWhiteSpace(row[62].ToString().FormatField()))
                {
                    detail.Contacts.Add(new()
                    {
                        SentralId = $"{familyId}.2",
                        Sequence = 2,
                        Title = row[61].ToString().FormatField(),
                        FirstName = row[62].ToString().FormatField(),
                        LastName = row[63].ToString().FormatField(),
                        Mobile = row[65].ToString().FormatField()
                    });
                }

                detail.StudentReferenceNumbers.Add(row[0].ToString().FormatField());

                data.Add(detail);
            }
        }

        using IExcelDataReader emailReader = ExcelReaderFactory.CreateReader(emailPage);
        DataSet emailWorksheet = emailReader.AsDataSet();

        foreach (DataRow row in emailWorksheet.Tables[0].Rows)
        {
            if (row.ItemArray.First()?.ToString() == "STUDENT_ID") // This is a header row
                continue;

            string studentId = row[0].ToString().FormatField();

            FamilyDetailsDto detail = data.FirstOrDefault(item => item.StudentReferenceNumbers.Contains(studentId));

            if (detail is null)
            {
                // This student is not part of a family?
                logger.Warning("Student {studentId} is not found in any active family", studentId);

                continue;
            }

            detail.FamilyEmail = row[8].ToString().FormatEmail();

            string email2 = row[9].ToString().FormatEmail();
            if (!string.IsNullOrWhiteSpace(email2))
            {
                FamilyDetailsDto.Contact parent = detail.Contacts.FirstOrDefault(contact => contact.Sequence == 1);

                if (parent is not null)
                    parent.Email = email2;
            }

            string email3 = row[10].ToString().FormatEmail();
            if (!string.IsNullOrWhiteSpace(email3))
            {
                FamilyDetailsDto.Contact parent = detail.Contacts.FirstOrDefault(contact => contact.Sequence == 2);

                if (parent is not null)
                    parent.Email = email3;
            }
        }

        return data.Where(entry => entry.StudentReferenceNumbers.Any()).ToList();
    }

    public async Task<HtmlDocument> GetAwardsReport(CancellationToken cancellationToken = default)
    {
        if (_logOnly)
        {
            _logger.Information("GetAwardsReport");

            return new HtmlDocument();
        }
        
        List<KeyValuePair<string, string>> payload = new()
        {
            new KeyValuePair<string, string>("action", "exportStudentAwards")
        };

        HtmlDocument report = await GetPageByPost(new($"{_settings.ServerUrl}/wellbeing/awards/export"), payload, cancellationToken);

        return report;
    }

    public async Task<HtmlDocument> GetAwardsListing(string sentralStudentId, string calYear, CancellationToken cancellationToken = default)
    {
        if (_logOnly)
        {
            _logger.Information("GetAwardsListing: sentralStudentId={sentralStudentId}, calYear={calYear}", sentralStudentId, calYear);

            return new HtmlDocument();
        }

        HtmlDocument page = await GetPageByGet($"{_settings.ServerUrl}/wellbeing/students/incidents?id={sentralStudentId}&category=1&year={calYear}", cancellationToken);

        return page;
    }

    public async Task<HtmlDocument> GetIncidentDetailsPage(string uri, CancellationToken cancellationToken = default)
    {
        if (_logOnly)
        {
            _logger.Information("GetIncidentDetailsPage: uri={uri}", uri);

            return new HtmlDocument();
        }

        HtmlDocument page = await GetPageByGet($"{_settings.ServerUrl}{uri}", cancellationToken);

        return page;
    }

    public async Task<byte[]> GetAwardDocument(string sentralStudentId, string incidentId)
    {
        if (_logOnly)
        {
            _logger.Information("GetAwardDocument: sentralStudentId={sentralStudentId}, incidentId={incidentId}", sentralStudentId, incidentId);

            return null;
        }

        // Get the Issue Id first
        // https://admin.aurora.dec.nsw.gov.au/wellbeing/letters/print?letter_type=incident&id=30133&student_id=1868

        HtmlDocument previewPage = await GetPageByGet($"{_settings.ServerUrl}/wellbeing/letters/print?letter_type=incident&id={incidentId}&student_id={sentralStudentId}", default);

        HtmlNodeCollection inputs = previewPage.DocumentNode.SelectNodes("//input[@name='selected_issues[]']");
        //HtmlNodeCollection inputs = previewPage.DocumentNode.SelectNodes("//*[@id='print_frm']/div[1]/div[2]/div[2]/div[2]/span/input");
        
        string issue = string.Empty;

        foreach (HtmlNode input in inputs)
            issue = input.Attributes["value"].Value;

        // Use the Issue Id to generate the certificate

        List<KeyValuePair<string, string>> formData = new()
        {
            new KeyValuePair<string, string>("selected_issues[]", issue),
            new KeyValuePair<string, string>("letter_template_id", "31"),
            new KeyValuePair<string, string>("letter_type", "incident"),
            new KeyValuePair<string, string>("id[]", incidentId),
            new KeyValuePair<string, string>("do_action", "print"),
            new KeyValuePair<string, string>("issue_id", "")
        };

        byte[] response = await GetByteArrayByPost($"{_settings.ServerUrl}/wellbeing/letters/print?format=pdf", formData, default);

        if (response is null)
        {
            _logger.Warning("Did not successfully generate the certificate: {@formData}", formData);
            return Array.Empty<byte>();
        }

        string code = System.Text.Encoding.Default.GetString(response);

        bool readyToDownload = false;
        int retryCount = 20;

        while (!readyToDownload)
        {
            if (retryCount == 0)
                return null;

            string progress = await GetJsonByGet($"{_settings.ServerUrl}/_common/lib/jasper_reports?action=pollQueue&user_key={code}", default);

            string status = JsonSerializerExtensions.DeserializeAnonymousType(progress, new { status = "" }).status;
            
            if (status != "COMPLETE")
            {
                await Task.Delay(1000, default);
                retryCount--;
            }
            else
            {
                readyToDownload = true;
            }
        }

        byte[] document = await GetByteArrayByGet($"{_settings.ServerUrl}/_common/lib/jasper_reports?format=pdf&key={code}&action=save", default);

        if (document is null)
        {
            _logger.Warning("Did not successfully download certificate: {@formData} ({code})", formData, code);
            return Array.Empty<byte>();
        }

        return document;
    }

    public async Task<(Stream BasicFile, Stream DetailFile)> GetNAwardReport(CancellationToken cancellationToken = default)
    {
        Stream baseFile = await GetStreamByGet($"{_settings.ServerUrl}/wellbeing/reports/incidents?report_id=4154&export-xls&victims-witnesses=All", cancellationToken);
        Stream detailFile = await GetStreamByGet($"{_settings.ServerUrl}/wellbeing/reports/incidents?report_id=4154&export-xls&victims-witnesses=All&extra-n-award-details=true", cancellationToken);

        return (baseFile, detailFile);
    }

    public async Task<SystemAttendanceData> GetAttendancePercentages(string term, string week, string year, DateOnly startDate, DateOnly endDate)
    {
        SystemAttendanceData response = new();

        if (_logOnly)
        {
            _logger.Information("GetAttendancePercentages: ");

            return response;
        }

        List<KeyValuePair<string, string>> payload = new()
        {
            // length=year
            new("length", "period"),
            // year=2023
            new("year", year),
            // start_date=2023-01-01
            new ("start_date", _dateTime.FirstDayOfYear.ToString("yyyy-MM-dd")),
            // end_date=2023-11-03
            new ("end_date", endDate.ToString("yyyy-MM-dd")),
            // limit_sign=equal
            new("limit_sign", "equal"),
            // limit_percent=100
            new("limit_percent", "100"),
            // reasons%5B%5D=8
            new("reasons[]", "8"),
            // reasons%5B%5D=1
            new("reasons[]", "1"),
            // reasons%5B%5D=7
            new("reasons[]", "7"),
            // reasons%5B%5D=5
            new("reasons[]", "5"),
            // reasons%5B%5D=3
            new("reasons[]", "3"),
            // reasons%5B%5D=9
            new("reasons[]", "9"),
            // show_current=true
            new("show_current", "true"),
            // group=years
            new("group", "years"),
            // years%5B%5D=5
            new("years[]", "5"),
            // years%5B%5D=6
            new("years[]", "6"),
            // years%5B%5D=7
            new("years[]", "7"),
            // years%5B%5D=8
            new("years[]", "8"),
            // years%5B%5D=9
            new("years[]", "9"),
            // years%5B%5D=10
            new("years[]", "10"),
            // years%5B%5D=11
            new("years[]", "11"),
            // years%5B%5D=12
            new("years[]", "12"),
            // action=export
            new("action", "export")
        };

        response.YearToDateDayCalculationDocument = await GetPageByPost(new($"{_settings.ServerUrl}/attendance/reports/percentage"), payload, default);

        Stream perMinuteYearToDateCalculationFile = await GetStreamByGet($"{_settings.ServerUrl}/attendancepxp/period/administration/percentage_attendance_report?length=period&year={year}&start_date={_dateTime.FirstDayOfYear.ToString("yyyy-MM-dd")}&end_date={endDate.ToString("yyyy-MM-dd")}&attendance_source=attendance&enrolled_students=true&group=years&years%5B%5D=5&years%5B%5D=6&years%5B%5D=7&years%5B%5D=8&years%5B%5D=9&years%5B%5D=10&years%5B%5D=11&years%5B%5D=12&action=export", default);
        response.YearToDateMinuteCalculationDocument = perMinuteYearToDateCalculationFile.IsExcelFile() ? perMinuteYearToDateCalculationFile : null;

        payload = new()
        {
            // length=week
            new("length", "week"),
            // term=3
            new ("term", term),
            // week=1
            new ("week", week),
            // year=2023
            new("year", year),
            // limit_sign=equal
            new("limit_sign", "equal"),
            // limit_percent=100
            new("limit_percent", "100"),
            // reasons%5B%5D=8
            new("reasons[]", "8"),
            // reasons%5B%5D=1
            new("reasons[]", "1"),
            // reasons%5B%5D=7
            new("reasons[]", "7"),
            // reasons%5B%5D=5
            new("reasons[]", "5"),
            // reasons%5B%5D=3
            new("reasons[]", "3"),
            // reasons%5B%5D=9
            new("reasons[]", "9"),
            // show_current=true
            new("show_current", "true"),
            // group=years
            new("group", "years"),
            // years%5B%5D=5
            new("years[]", "5"),
            // years%5B%5D=6
            new("years[]", "6"),
            // years%5B%5D=7
            new("years[]", "7"),
            // years%5B%5D=8
            new("years[]", "8"),
            // years%5B%5D=9
            new("years[]", "9"),
            // years%5B%5D=10
            new("years[]", "10"),
            // years%5B%5D=11
            new("years[]", "11"),
            // years%5B%5D=12
            new("years[]", "12"),
            // action=export
            new("action", "export")
        };

        response.WeekDayCalculationDocument = await GetPageByPost(new($"{_settings.ServerUrl}/attendance/reports/percentage"), payload, default);

        Stream perMinuteWeekCalculationFile = await GetStreamByGet($"{_settings.ServerUrl}/attendancepxp/period/administration/percentage_attendance_report?length=week&term={term}&week={week}&year={year}&attendance_source=attendance&enrolled_students=true&group=years&years%5B%5D=5&years%5B%5D=6&years%5B%5D=7&years%5B%5D=8&years%5B%5D=9&years%5B%5D=10&years%5B%5D=11&years%5B%5D=12&action=export", default);
        response.WeekMinuteCalculationDocument = perMinuteWeekCalculationFile.IsExcelFile() ? perMinuteWeekCalculationFile : null;

        return response;
    }
}
