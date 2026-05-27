namespace Constellation.Infrastructure.ExternalServices.Sentral;

using Application.Domains.AppSettings.Models;
using Application.Domains.Attendance.Reports.Commands.UpdateAttendanceDataForPeriodFromSentral;
using Application.Domains.LinkedSystems.Sentral.Queries.GetTermsAndWeeksForCurrentYear;
using Application.DTOs;
using Application.Extensions;
using Application.Interfaces.Configuration;
using Application.Interfaces.Gateways;
using Application.Interfaces.Services;
using Constellation.Application.Domains.MeritAwards.Awards.Enums;
using Constellation.Core.Enums;
using Constellation.Core.Models.Students.ValueObjects;
using Constellation.Infrastructure.Extensions;
using Constellation.Infrastructure.ExternalServices.Sentral.Models;
using Core.Abstractions.Clock;
using Core.Errors;
using Core.Helpers;
using Core.Models.AppSettings.Enums;
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
using System.Threading;

public class Gateway : ISentralGateway
{
    private readonly IDateTimeProvider _dateTime;
    private readonly IAppSettingsService _appSettings;
    private readonly SentralGatewayConfiguration _settings;
    private readonly ILogger _logger;
    private readonly bool _logOnly;
    private readonly HttpClient _client;
    private readonly HttpClient _apiClient;

    private HtmlDocument? _studentListPage;

    public Gateway(
        IDateTimeProvider dateTime,
        IOptions<SentralGatewayConfiguration> settings, 
        IAppSettingsService appSettings,
        IHttpClientFactory factory,
        ILogger logger)
    {
        _dateTime = dateTime;
        _appSettings = appSettings;
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
        completeResponse.Add(JsonSection.Data, []);
        completeResponse.Add(JsonSection.Includes, []);

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
        Uri path = new($"{_settings.ApiUrl}/restapi/v1/core/core-student?includeInactive=0");

        Dictionary<JsonSection, List<JsonElement>> studentResponse = await GetApiJsonResponse(path, cancellationToken);

        List<FamilyDetailsDto> familyDetails = [];

        foreach (JsonElement entry in studentResponse[JsonSection.Data])
        {
            string? sentralId = entry.ExtractString("id");

            if (sentralId is null)
                continue;

            FamilyDetailsDto response = await GetParentContactEntryFromApi(sentralId, cancellationToken);

            FamilyDetailsDto? existingEntry = familyDetails.FirstOrDefault(dto => dto.FamilyId == response.FamilyId);

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
        Uri path = new($"{_settings.ApiUrl}/restapi/v1/core/core-student/{sentralStudentId}?include=studentRelationships,contacts");

        Dictionary<JsonSection, List<JsonElement>> studentResponse = await GetApiJsonResponse(path, cancellationToken);

        List<CoreStudent> students = [];
        List<CoreStudentRelationship> people = [];
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
                            string type = entry.ExtractString("type") ?? string.Empty;

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
                            }
                        }

                        break;
                    }
            }
        }

        string? familyId = students.FirstOrDefault()?.FamilyId;

        if (string.IsNullOrWhiteSpace(familyId))
            return new FamilyDetailsDto();

        path = new($"{_settings.ApiUrl}/restapi/v1/core/core-family/{familyId}");

        Dictionary<JsonSection, List<JsonElement>> familyResponse = await GetApiJsonResponse(path, cancellationToken);
        bool foundFamily = false;

        foreach (JsonElement entry in familyResponse.Where(entry => entry.Key == JsonSection.Data).SelectMany(entry => entry.Value))
        {
            Result<CoreFamily> familyResult = CoreFamily.ConvertFromJson(entry);

            if (familyResult.IsFailure)
                continue;

            family = familyResult.Value;
            foundFamily = true;
        }

        if (!foundFamily)
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
        Uri path = new($"{_settings.ApiUrl}/restapi/v1/core/core-student/{sentralStudentId}/photo");

        byte[] imageResponse = await GetApiImageResponse(path, cancellationToken);

        return imageResponse;
    }

    public async Task<List<SchoolCalendarWeek>> GetTermsAndWeeksFromApi(string year, CancellationToken cancellationToken = default)
    {
        Uri path = new($"{_settings.ApiUrl}/restapi/v1/core/date");

        Dictionary<JsonSection, List<JsonElement>> studentResponse = await GetApiJsonResponse(path, cancellationToken);

        List<CoreDate> dates = [];

        foreach (KeyValuePair<JsonSection, List<JsonElement>> section in studentResponse)
        {
            switch (section.Key)
            {
                case JsonSection.Data:
                    {
                        foreach (JsonElement entry in section.Value)
                        {
                            Result<CoreDate> date = CoreDate.ConvertFromJson(entry);

                            if (date.IsFailure)
                                continue;

                            dates.Add(date.Value);
                        }

                        break;
                    }
            }
        }

        List<SchoolCalendarWeek> response = [];

        for (int i = 1; i < 5; i++)
        {
            List<IGrouping<int, CoreDate>> groupedDates = dates
                .Where(entry =>
                    entry.Code != "W" &&
                    entry.Term == i.ToString(CultureInfo.InvariantCulture))
                .GroupBy(entry => int.Parse(entry.Week, CultureInfo.InvariantCulture))
                .ToList();

            foreach (IGrouping<int, CoreDate> group in groupedDates)
            {
                response.Add(new(
                    $"Term {i}",
                    group.MinBy(entry => entry.Date)!.Date.ToDateTime(TimeOnly.MinValue),
                    group.MaxBy(entry => entry.Date)!.Date.ToDateTime(TimeOnly.MinValue),
                    $"Term {i} Week {group.Key}"));
            }
        }

        return response;
    }

    #endregion

    private async Task Login(CancellationToken cancellationToken = default)
    {
        Uri uri = new($"{_settings.ServerUrl}/auth/?manual=true");

        HttpRequestMessage request = new();
        request.Headers.CacheControl = new CacheControlHeaderValue
        {
            NoCache = true
        };

        List<KeyValuePair<string, string>> formData =
        [
            new KeyValuePair<string, string>("username", _settings.Username),
            new KeyValuePair<string, string>("password", _settings.Password),
            new KeyValuePair<string, string>("action", "login")
        ];

        FormUrlEncodedContent formDataEncoded = new(formData);

        for (int i = 1; i < 6; i++)
        {
            try
            {
                HttpResponseMessage response = await _client.PostAsync(uri, formDataEncoded, cancellationToken);
                response.EnsureSuccessStatusCode();

                formDataEncoded.Dispose();
                request.Dispose();
                
                return;
            }
            catch (Exception ex)
            {
                _logger
                    .ForContext("Method", nameof(Login))
                    .Warning("Failed to login to Sentral Server with error: {message}", ex.Message);

                if (ex.InnerException != null)
                    _logger.Warning("Inner Exception: {message}",ex.InnerException.Message);

                // Wait and retry
                await Task.Delay(5000, cancellationToken);
            }
        }

        formDataEncoded.Dispose();
        request.Dispose();

        throw new HttpRequestException("Could not connect to Sentral Server");
    }

    private async Task<string?> GetJsonByGet(string path, CancellationToken cancellationToken = default)
    {
        Uri uri = new(path);

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
                _logger.Warning("Failed to retrieve information from Sentral Server with error: {message}",ex.Message);
                if (ex.InnerException != null)
                    _logger.Warning("Inner Exception: {message}", ex.InnerException.Message);

                // Wait and retry
                await Task.Delay(5000, cancellationToken);
            }
        }

        return string.Empty;
    }

    private async Task<HtmlDocument?> GetPageByGet(string path, CancellationToken cancellationToken = default)
    {
        Uri uri = new(path);

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
                _logger.Warning("Failed to retrieve information from Sentral Server with error: {message}", ex.Message);
                if (ex.InnerException != null)
                    _logger.Warning("Inner Exception: {message}", ex.InnerException.Message);

                // Wait and retry
                await Task.Delay(5000, cancellationToken);
            }
        }

        return null;
    }

    private async Task<HtmlDocument?> GetPageByPost(
        Uri uri, 
        List<KeyValuePair<string, string>> payload, 
        CancellationToken cancellationToken = default)
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
                _logger.Warning("Failed to retrieve information from Sentral Server with error: {message}", ex.Message);
                if (ex.InnerException != null)
                    _logger.Warning("Inner Exception: {message}", ex.InnerException.Message);

                // Wait and retry
                await Task.Delay(5000, cancellationToken);
            }
        }

        return null;
    }

    private async Task<Stream> GetStreamByPost(
        Uri uri, 
        List<KeyValuePair<string, string>> payload, 
        CancellationToken cancellationToken = default)
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
                _logger.Warning("Failed to retrieve information from Sentral Server with error: {message}", ex.Message);
                if (ex.InnerException != null)
                    _logger.Warning("Inner Exception: {message}", ex.InnerException.Message);

                // Wait and retry
                await Task.Delay(5000, cancellationToken);
            }
        }

        return Stream.Null;
    }

    private async Task<Stream> GetStreamByGet(
        string path, 
        CancellationToken cancellationToken = default)
    {
        Uri uri = new(path);

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
                _logger.Warning("Failed to retrieve information from Sentral Server with error: {message}", ex.Message);
                if (ex.InnerException != null)
                    _logger.Warning("Inner Exception: {message}", ex.InnerException.Message);

                // Wait and retry
                await Task.Delay(5000, cancellationToken);
            }
        }

        return Stream.Null;
    }

    private async Task<byte[]?> GetByteArrayByGet(
        string path, 
        CancellationToken cancellationToken = default)
    {
        Uri uri = new(path);

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
                _logger.Warning("Failed to retrieve information from Sentral Server with error: {message}", ex.Message);
                if (ex.InnerException != null)
                    _logger.Warning("Inner Exception: {message}", ex.InnerException.Message);

                // Wait and retry
                await Task.Delay(5000, cancellationToken);
            }
        }

        return null;
    }

    private async Task<byte[]?> GetByteArrayByPost(
        string path, 
        List<KeyValuePair<string, string>> payload, 
        CancellationToken cancellationToken = default)
    {
        Uri uri = new(path);

        for (int i = 1; i < 6; i++)
        {
            try
            {
                await Login(cancellationToken);

                FormUrlEncodedContent encodedPayload = new FormUrlEncodedContent(payload);

                HttpResponseMessage response = await _client.PostAsync(uri, encodedPayload , cancellationToken);
                byte[] content = await response.Content.ReadAsByteArrayAsync(cancellationToken);

                encodedPayload.Dispose();
                return content;
            }
            catch (Exception ex)
            {
                _logger.Warning("Failed to retrieve information from Sentral Server with error: {message}", ex.Message);
                if (ex.InnerException != null)
                    _logger.Warning("Inner Exception: {message}", ex.InnerException.Message);

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

        if (studentSentralIds.Count == 0)
            return Result.Failure<DateTime>(SentralGatewayErrors.NoStudentIdsProvided);

        // Stellar = 3, Galaxy = 6, Universal = 7
        string? award = awardType switch
        {
            IssueAwardType.Stellar => "3",
            IssueAwardType.Galaxy => "6",
            IssueAwardType.Universal => "7",
            _ => null
        };

        if (string.IsNullOrWhiteSpace(award))
        {
            _logger
                .ForContext(nameof(IssueAwardType), awardType, true)
                .ForContext(nameof(Error), SentralGatewayErrors.DataConversionError(nameof(IssueAwardType)), true)
                .Warning($"Failed to convert {nameof(IssueAwardType)} to Sentral Award Id");
            
            return Result.Failure<DateTime>(SentralGatewayErrors.DataConversionError(nameof(IssueAwardType)));
        }

        List<KeyValuePair<string, string>> payload =
            [
                new("action", "addAwards"),
                new("awards[]", award)
            ];

        foreach (string student in studentSentralIds)
            payload.Add(new("students[]", student));

        payload.Add(new("date", _dateTime.Today.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)));

        HtmlDocument? result = await GetPageByPost(new($"{_settings.ServerUrl}/wellbeing/awards/new"), payload, CancellationToken.None);
        DateTime current = _dateTime.Now;

        if (result is null)
            return Result.Failure<DateTime>(SentralGatewayErrors.IncorrectResponseFromServer);

        return current;
    }
    
    public async Task<List<(string SentralId, List<string> Flags)>> GetStudentFlags(
        CancellationToken cancellationToken = default)
    {
        List<(string SentralId, List<string> Flags)> studentFlags = [];

        if (_logOnly)
        {
            _logger.Information("GetStudentFlags");

            return studentFlags;
        }
        
        _studentListPage ??= await GetPageByGet($"{_settings.ServerUrl}/profiles/main/search?eduproq=&search=advanced&plan_type=plans", cancellationToken);

        if (_studentListPage == null)
            return studentFlags;

        HtmlDocument page = _studentListPage;

        SentralConfiguration? studentTablePath = await _appSettings.Sentral(SentralPath.StudentTable, cancellationToken);

        if (studentTablePath is null)
            return studentFlags;

        HtmlNode? studentTable = page.DocumentNode.SelectSingleNode(studentTablePath.Path);

        if (studentTable is null)
            return studentFlags;

        foreach (HtmlNode row in studentTable.Descendants("tr"))
        {
            HtmlNode firstCell = row.ChildNodes.FindFirst("td");
            string sentralId;
            List<string> flags = [];

            string href = firstCell.ChildNodes.FindFirst("a").GetAttributeValue("href", "");
            if (string.IsNullOrWhiteSpace(href))
            {
                // Something went wrong? What now?
                throw new NodeAttributeNotFoundException();
            }
            else
            {
                sentralId = href.Split('/').ElementAt(^2);
            }

            IEnumerable<HtmlNode> flagSpans = row.Descendants("td").Last().Descendants("span");
            
            foreach (HtmlNode flagSpan in flagSpans)
                flags.Add(flagSpan.InnerText.Trim());

            studentFlags.Add(new (sentralId, flags));
        }

        return studentFlags;
    }

    public async Task<ICollection<SentralReportDto>> GetStudentReportList(string sentralStudentId)
    {
        if (_logOnly)
        {
            _logger.Information("GetStudentReportList: sentralStudentId={sentralStudentId}", sentralStudentId);

            return new List<SentralReportDto>();
        }

        HtmlNode? reportTable = null;
        HtmlDocument? page = await GetPageByGet($"{_settings.ServerUrl}/profiles/students/{sentralStudentId}/academic-history");

        List<SentralReportDto> dataList = [];

        if (page == null)
            return dataList;

        HtmlNodeCollection? menuItems = page.DocumentNode.SelectNodes("//*[@id='reporting_period']/option");

        if (menuItems is null || menuItems.Count == 0)
            return dataList;

        foreach (HtmlNode menuItem in menuItems)
        {
            string linkRef = menuItem.GetAttributeValue("value", "");
            if (string.IsNullOrWhiteSpace(linkRef))
                continue;

            HtmlDocument? reportPage = await GetPageByGet($"{_settings.ServerUrl}/profiles/students/{sentralStudentId}/academic-history?type=sreport&page=printed_report&reporting_period={linkRef}");
            reportTable = reportPage?.DocumentNode.SelectSingleNode("//*[@id='layout-2col-content']/div/div/div[2]/table/tbody");

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

            return [];
        }

        List<KeyValuePair<string, string>> formData =
        [
            new KeyValuePair<string, string>("file_id", reportId),
            new KeyValuePair<string, string>("action", "download_file")
        ];
        
        byte[]? response = await GetByteArrayByPost($"{_settings.ServerUrl}/profiles/students/{sentralStudentId}/academic-history?type=sreport&page=printed_report", formData);

        return response ?? [];
    }

    public async Task<IndigenousStatus> GetStudentIndigenousStatus(string sentralStudentId)
    {
        if (_logOnly)
        {
            _logger.Information("GetStudentIndigenousStatus: sentralStudentId={sentralStudentId}", sentralStudentId);

            return IndigenousStatus.Unknown;
        }

        HtmlDocument? page = await GetPageByGet($"{_settings.ServerUrl}/profiles/students/{sentralStudentId}/overview", CancellationToken.None);

        if (page is null)
            return IndigenousStatus.Unknown;

        SentralConfiguration? indigenousStatusPath = await _appSettings.Sentral(SentralPath.IndigenousStatus);

        if (indigenousStatusPath is null)
            return IndigenousStatus.Unknown;

        HtmlNode? atsiField = page.DocumentNode.SelectSingleNode(indigenousStatusPath.Path);

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

            return string.Empty;
        }

        HtmlDocument? page = await GetPageByGet($"{_settings.ServerUrl}/admin/datasync/students?year={grade}&type=active");

        SentralConfiguration? studentTablePath = await _appSettings.Sentral(SentralPath.StudentSRNTable);

        if (studentTablePath is null)
            return string.Empty;
        
        HtmlNode? studentTable = page?.DocumentNode.SelectSingleNode(studentTablePath.Path);
        
        if (studentTable is null)
            return string.Empty;
        
        foreach (HtmlNode row in studentTable.Descendants("tr"))
        {
            HtmlNode? cell = row.Descendants("td").FirstOrDefault();
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

        return string.Empty;
    }

    public async Task<List<SentralPeriodAbsenceDto>> GetAbsenceDataAsync(string sentralStudentId)
    {
        if (_logOnly)
        {
            _logger.Information("GetAbsenceDataAsync: sentralStudentId={sentralStudentId}", sentralStudentId);

            return [];
        }

        HtmlDocument? page = await GetPageByGet($"{_settings.ServerUrl}/attendancepxp/administration/student?id={sentralStudentId}");

        if (page == null)
            return [];

        SentralConfiguration? absenceTablePath = await _appSettings.Sentral(SentralPath.AbsenceTable);

        if (absenceTablePath is null)
            return [];

        HtmlNode? absenceTable = page.DocumentNode.SelectSingleNode(absenceTablePath.Path);

        List<SentralPeriodAbsenceDto> absences = [];

        if (absenceTable is null)
            return [];

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
                date = DateOnly.Parse(stringDate, CultureInfo.InvariantCulture);
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
                                if (!absenceTypeText.Contains('(', StringComparison.InvariantCultureIgnoreCase))
                                {
                                    // What the hell happened here? This shouldn't happen!
                                }
                                else
                                {
                                    // Partial absence, but for how long?
                                    periodAbsence.Type = SentralPeriodAbsenceDto.Partial;
                                    string stringMinutes = absenceTypeText.Split('(')[1].Split(')')[0];
                                    periodAbsence.MinutesAbsent = int.Parse(stringMinutes, CultureInfo.InvariantCulture);
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
        
        List<SentralPeriodAbsenceDto> absences = [];

        for (int term = 1; term < 5; term++)
        {
            HtmlDocument? page =
                await GetPageByGet(
                    $"{_settings.ServerUrl}/attendancepxp/administration/student?id={sentralStudentId}?term={term}&year={year}",
                    cancellationToken);

            if (page == null)
                return Result.Failure<List<SentralPeriodAbsenceDto>>(SentralGatewayErrors.IncorrectResponseFromServer);

            SentralConfiguration? absencesTablePath =
                await _appSettings.Sentral(SentralPath.AbsenceTable, cancellationToken);

            if (absencesTablePath is null)
                return Result.Failure<List<SentralPeriodAbsenceDto>>(SentralGatewayErrors.IncorrectResponseFromServer);

            HtmlNode? absenceTable = page.DocumentNode.SelectSingleNode(absencesTablePath.Path);

            if (absenceTable is null)
                return Result.Failure<List<SentralPeriodAbsenceDto>>(SentralGatewayErrors.IncorrectResponseFromServer);

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
                    date = DateOnly.Parse(stringDate, CultureInfo.InvariantCulture);
                    previousDate = date;
                }

                SentralPeriodAbsenceDto periodAbsence = new() { Date = date };

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
                                    if (!absenceTypeText.Contains('(', StringComparison.InvariantCultureIgnoreCase))
                                    {
                                        // What the hell happened here? This shouldn't happen!
                                    }
                                    else
                                    {
                                        // Partial absence, but for how long?
                                        periodAbsence.Type = SentralPeriodAbsenceDto.Partial;
                                        string stringMinutes = absenceTypeText.Split('(')[1].Split(')')[0];
                                        periodAbsence.MinutesAbsent = int.Parse(stringMinutes, CultureInfo.InvariantCulture);
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

    public async Task<Dictionary<StudentReferenceNumber, List<SentralPeriodAbsenceDto>>> GetAttendanceModuleAbsenceDataForSchool(
        CancellationToken cancellationToken = default)
    {
        Dictionary<StudentReferenceNumber, List<SentralPeriodAbsenceDto>> result = new();

        if (_logOnly)
        {
            _logger.Information("GetAttendanceModuleAbsenceDataForSchool");

            return result;
        }

        for (int i = 1; i < 5; i++)
        {
            List<SentralPeriodAbsenceDto> data = await GetAttendanceModuleAbsenceDataForTerm(i.ToString(CultureInfo.InvariantCulture), cancellationToken);

            foreach (var datum in data)
            {
                if (result.TryGetValue(datum.StudentReferenceNumber, out List<SentralPeriodAbsenceDto>? record))
                {
                    record.Add(datum);
                }
                else
                {
                    result.Add(datum.StudentReferenceNumber, [datum]);
                }
            }
        }

        return result;
    }

    private async Task<List<SentralPeriodAbsenceDto>> GetAttendanceModuleAbsenceDataForTerm(
        string term,
        CancellationToken cancellationToken = default)
    {
        List<SentralPeriodAbsenceDto> data = [];

        Uri filePath = new Uri($"{_settings.ServerUrl}/attendance/reports/absences");

        List<KeyValuePair<string, string>> formData =
        [
            new KeyValuePair<string, string>("length", "term"),
            new KeyValuePair<string, string>("term", term),
            new KeyValuePair<string, string>("year", _dateTime.CurrentYearAsString),
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
        ];

        Stream completePage = await GetStreamByPost(filePath, formData, cancellationToken);

        if (completePage.Length == 0) // Stream is null
            return [];

        using IExcelDataReader completeReader = ExcelReaderFactory.CreateReader(completePage);
        DataSet completeWorksheet = completeReader.AsDataSet();

        foreach (DataRow row in completeWorksheet.Tables[0].Rows)
        {
            string srn = row[0].ToString()?.FormatField ?? string.Empty;

            if (srn == "Student ID") // This is a header row
                continue;

            Result<StudentReferenceNumber> studentReferenceNumber = StudentReferenceNumber.Create(srn);
            if (studentReferenceNumber.IsFailure)
            {
                _logger
                    .ForContext(nameof(StudentReferenceNumber), srn)
                    .ForContext(nameof(Error), studentReferenceNumber.Error, true)
                    .Information("Error parsing SRN to StudentReferenceNumber object");

                continue;
            }

            SentralPeriodAbsenceDto absence = new();
            absence.StudentReferenceNumber = studentReferenceNumber.Value;
            string stringDate = row[2].ToString()?.FormatField ?? string.Empty;
            bool exactConversion = DateOnly.TryParseExact(stringDate, "yyyy-MM-dd", out DateOnly rowDate);
            if (!exactConversion)
                continue;

            absence.Date = rowDate;
            absence.Reason = row[9].ToString()?.FormatField ?? string.Empty;

            absence.Timeframe = row[10].ToString()?.FormatField ?? string.Empty;
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
                {
                    _logger
                        .ForContext("DetectedTime", absence.Timeframe.Split(' ')[0])
                        .ForContext("AbsenceDate", absence.Date)
                        .ForContext(nameof(StudentReferenceNumber), studentReferenceNumber.Value)
                        .Information("Error parsing absence start time to TimeOnly object");

                    continue;
                }

                bool endTimeSuccess = TimeOnly.TryParseExact(absence.Timeframe.Split(' ')[2], "h:mmtt", CultureInfo.InvariantCulture, DateTimeStyles.None, out TimeOnly endTime);
                if (endTimeSuccess)
                    absence.EndTime = endTime;
                else
                {
                    _logger
                        .ForContext("DetectedTime", absence.Timeframe.Split(' ')[0])
                        .ForContext("AbsenceDate", absence.Date)
                        .ForContext(nameof(StudentReferenceNumber), studentReferenceNumber.Value)
                        .Information("Error parsing absence end time to TimeOnly object");
                    
                    continue;
                }
            }

            string comment = row[11].ToString()?.FormatField ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(comment))
            {
                string explainer = row[12].ToString()?.FormatField ?? string.Empty;
                if (string.IsNullOrWhiteSpace(explainer))
                {
                    absence.ExternalExplanation = comment;
                }
                else
                {
                    string explainerSource = row[13].ToString()?.FormatField ?? string.Empty;
                    absence.ExternalExplanation = comment;
                    absence.ExternalExplanationSource = string.IsNullOrWhiteSpace(explainerSource)
                        ? explainer
                        : $"{explainer} via {explainerSource}";
                }
            }

            data.Add(absence);
        }

        return data;
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

        HtmlDocument? page = await GetPageByGet($"{_settings.ServerUrl}/attendancepxp/administration/student?id={sentralId}&year={year}");

        if (page is null)
            return Result.Failure<List<DateOnly>>(SentralGatewayErrors.IncorrectResponseFromServer);

        SentralConfiguration? enrolmentDatePath = await _appSettings.Sentral(SentralPath.StudentEnrolmentDates);

        if (enrolmentDatePath is null)
            return Result.Failure<List<DateOnly>>(SentralGatewayErrors.IncorrectResponseFromServer);
        
        HtmlNodeCollection pxpRolls = page.DocumentNode.SelectNodes(enrolmentDatePath.Path);

        List<DateOnly> enrolledDates = [];

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

                    int pos = cellTitle.IndexOf("::", StringComparison.InvariantCultureIgnoreCase);

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

            return [];
        }

        HtmlDocument? page = await GetPageByGet($"{_settings.ServerUrl}/admin/settings/school/calendar/{year}/month");

        if (page == null)
            return [];

        SentralConfiguration? calendarTablePath = await _appSettings.Sentral(SentralPath.CalendarTable);

        if (calendarTablePath is null)
            return [];

        HtmlNode? calendarTable = page.DocumentNode.SelectSingleNode(calendarTablePath.Path);

        List<DateOnly> nonSchoolDays = [];

        if (calendarTable is null) 
            return nonSchoolDays.OrderBy(a => a).ToList();
        
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
                DateOnly date = DateOnly.Parse(detectedDate, DateTimeFormatInfo.CurrentInfo);

                nonSchoolDays.Add(date);
            }
        }

        return nonSchoolDays.OrderBy(a => a).ToList();
    }

    public async Task<Result<(SchoolWeek Week, SchoolTerm Term)>> GetWeekForDate(DateOnly date)
    {
        if (_logOnly)
        {
            _logger
                .Information("GetWeekForDate: date={date}", date);

            return (SchoolWeek.Week1, SchoolTerm.Term1);
        }

        string year = date.Year.ToString(CultureInfo.InvariantCulture);

        HtmlDocument? page = await GetPageByGet($"{_settings.ServerUrl}/admin/settings/school/calendar/{year}/term");

        if (page is null)
            return Result.Failure<(SchoolWeek, SchoolTerm)>(SentralGatewayErrors.IncorrectResponseFromServer);

        SentralConfiguration? termCalendarTable = await _appSettings.Sentral(SentralPath.TermCalendarTable);

        if (termCalendarTable is null)
            return Result.Failure<(SchoolWeek, SchoolTerm)>(ApplicationErrors.InvalidConfiguration(nameof(SentralConfiguration)));

        HtmlNode calendarTable = page.DocumentNode.SelectSingleNode(termCalendarTable.Path);

        if (calendarTable is null)
        {
            return Result.Failure<(SchoolWeek, SchoolTerm)>(SentralGatewayErrors.TermDatesNotFound);
        }

        IEnumerable<HtmlNode> rows = calendarTable.Descendants("tr");

        SchoolTerm? term = SchoolTerm.Term1;

        foreach (HtmlNode row in rows)
        {
            if (row.Descendants("td").Count() == 1)
            {
                // This is a header row
                HtmlNode header = row.Descendants("td").First();
                HtmlNode? termName = header.Descendants("b").FirstOrDefault();

                if (termName is null)
                {
                    // This is a blank row, skip
                    continue;
                }

                term = SchoolTerm.FromName(termName.InnerText);
                continue;
            }

            HtmlNode? weekName = row.Descendants("th").FirstOrDefault();

            if (weekName is null || string.IsNullOrWhiteSpace(weekName.InnerText))
                continue;
            
            SchoolWeek? week = SchoolWeek.FromValue(weekName.InnerText);

            foreach (HtmlNode cell in row.Descendants("td"))
            {
                string action = cell.GetAttributeValue("onclick", "");
                if (string.IsNullOrWhiteSpace(action))
                    continue;

                string detectedDate = action.Split('\'')[1];
                if (DateOnly.Parse(detectedDate, DateTimeFormatInfo.CurrentInfo) != date)
                    continue;

                if (week is not null && term is not null)
                    return (week, term);
            }
        }

        return Result.Failure<(SchoolWeek, SchoolTerm)>(SentralGatewayErrors.TermDatesNotFound);
    }

    public async Task<Result<(DateOnly StartDate, DateOnly EndDate)>> GetDatesForWeek(string year, SchoolTerm term, SchoolWeek week)
    {
        if (_logOnly)
        {
            _logger
                .Information("GetDatesForWeek: year={year}, term={term}, week={week}", year, term, week);

            return (_dateTime.Today, _dateTime.Today.AddDays(12));
        }

        DateOnly startDate = DateOnly.MinValue;
        DateOnly endDate = DateOnly.MinValue;

        HtmlDocument? page = await GetPageByGet($"{_settings.ServerUrl}/admin/settings/school/calendar/{year}/term");

        if (page == null)
            return Result.Failure<(DateOnly, DateOnly)>(SentralGatewayErrors.IncorrectResponseFromServer);

        SentralConfiguration? termCalendarTable = await _appSettings.Sentral(SentralPath.TermCalendarTable);

        if (termCalendarTable is null)
            return Result.Failure<(DateOnly, DateOnly)>(ApplicationErrors.InvalidConfiguration(nameof(SentralConfiguration)));

        HtmlNode? calendarTable = page.DocumentNode.SelectSingleNode(termCalendarTable.Path);

        if (calendarTable is null)
            return Result.Failure<(DateOnly, DateOnly)>(SentralGatewayErrors.IncorrectResponseFromServer);

        IEnumerable<HtmlNode> rows = calendarTable.Descendants("tr");

        bool correctTerm = false;
        bool correctWeek = false;

        foreach (HtmlNode row in rows)
        {
            if (row.Descendants("td").Count() == 1)
            {
                // This is a header row
                HtmlNode header = row.Descendants("td").First();
                HtmlNode? termName = header.Descendants("b").FirstOrDefault();

                if (termName is not null)
                    correctTerm = termName.InnerText == term.Name;

                continue;
            }

            if (correctTerm)
            {
                HtmlNode? weekName = row.Descendants("th").FirstOrDefault();

                if (weekName is not null)
                    correctWeek = weekName.InnerText == week.Value;

                HtmlNode monday = row.Descendants("td").First();

                string mondayAction = monday.GetAttributeValue("onclick", "");
                if (!string.IsNullOrWhiteSpace(mondayAction))
                {
                    string detectedDate = mondayAction.Split('\'')[1];
                    DateOnly date = DateOnly.Parse(detectedDate, DateTimeFormatInfo.CurrentInfo);

                    startDate = date;
                }

                HtmlNode friday = row.Descendants("td").Last();

                string fridayAction = friday.GetAttributeValue("onclick", "");
                if (!string.IsNullOrWhiteSpace(fridayAction))
                {
                    string detectedDate = fridayAction.Split('\'')[1];
                    DateOnly date = DateOnly.Parse(detectedDate, DateTimeFormatInfo.CurrentInfo);

                    endDate = date;
                }

                if (correctWeek)
                    return (startDate, endDate);
            }
        }

        if (startDate != DateOnly.MinValue && endDate != DateOnly.MinValue)
            return (startDate, endDate);

        return Result.Failure<(DateOnly, DateOnly)>(SentralGatewayErrors.IncorrectResponseFromServer);
    }

    public async Task<ICollection<RollMarkReportDto>> GetRollMarkingReportAsync(DateOnly date)
    {
        if (_logOnly)
        {
            _logger.Information("GetRollMarkingReportAsync: date={date}", date);

            return new List<RollMarkReportDto>();
        }

        string sentralDate = date.ToString("yyyy-MM-dd", new DateTimeFormatInfo());

        List<RollMarkReportDto> response = [];

        for (int campus = 1; campus < 4; campus++)
        {
            HtmlDocument? page = await GetPageByGet($"{_settings.ServerUrl}/attendancepxp/period/administration/roll_report?campus_id={campus}&range=single_day&date={sentralDate}&export=1", CancellationToken.None);

            if (page is null)
                continue;

            List<string> list = [];
            if (!page.DocumentNode.InnerHtml.StartsWith('<'))
                list = page.DocumentNode.InnerHtml.Split('\u000A').ToList();

            foreach (string entry in list)
            {
                string[] splitString = RegularExpressions.CommaSeparatedValueRowWithQuotedContent().Split(entry);

                if (splitString[0] == "\"Date\"" || splitString.Length != 7)
                    continue;

                response.Add(new RollMarkReportDto
                {
                    Date = DateTime.Parse(splitString[0].TrimStart('"').TrimEnd('"'), DateTimeFormatInfo.CurrentInfo),
                    Period = splitString[1].TrimStart('"').TrimEnd('"'),
                    ClassName = splitString[2].TrimStart('"').TrimEnd('"'),
                    Teacher = splitString[3].TrimStart('"').TrimEnd('"'),
                    Year = splitString[4].TrimStart('"').TrimEnd('"'),
                    Room = splitString[5].TrimStart('"').TrimEnd('"'),
                    Submitted = splitString[6].TrimStart('"').TrimEnd('"') == "Submitted"
                });
            }
        }

        return response;
    }

    public async Task<HtmlDocument?> GetAwardsReport(CancellationToken cancellationToken = default)
    {
        if (_logOnly)
        {
            _logger.Information("GetAwardsReport");

            return new HtmlDocument();
        }
        
        List<KeyValuePair<string, string>> payload =
            [new KeyValuePair<string, string>("action", "exportStudentAwards")];

        HtmlDocument? report = await GetPageByPost(new($"{_settings.ServerUrl}/wellbeing/awards/export"), payload, cancellationToken);

        return report;
    }

    public async Task<HtmlDocument?> GetAwardsListing(string sentralStudentId, string calYear, CancellationToken cancellationToken = default)
    {
        if (_logOnly)
        {
            _logger.Information("GetAwardsListing: sentralStudentId={sentralStudentId}, calYear={calYear}", sentralStudentId, calYear);

            return new HtmlDocument();
        }

        HtmlDocument? page = await GetPageByGet($"{_settings.ServerUrl}/wellbeing/students/incidents?id={sentralStudentId}&category=1&year={calYear}", cancellationToken);

        return page;
    }

    public async Task<HtmlDocument?> GetIncidentDetailsPage(string uri, CancellationToken cancellationToken = default)
    {
        if (_logOnly)
        {
            _logger.Information("GetIncidentDetailsPage: uri={uri}", uri);

            return new HtmlDocument();
        }

        HtmlDocument? page = await GetPageByGet($"{_settings.ServerUrl}{uri}", cancellationToken);

        return page;
    }

    public async Task<byte[]> GetAwardDocument(string sentralStudentId, string incidentId)
    {
        if (_logOnly)
        {
            _logger.Information("GetAwardDocument: sentralStudentId={sentralStudentId}, incidentId={incidentId}", sentralStudentId, incidentId);

            return [];
        }

        // Get the Issue Id first
        // {_settings.ServerUrl}/wellbeing/letters/print?letter_type=incident&id=30133&student_id=1868

        HtmlDocument? previewPage = await GetPageByGet($"{_settings.ServerUrl}/wellbeing/letters/print?letter_type=incident&id={incidentId}&student_id={sentralStudentId}");

        if (previewPage is null)
            return [];

        HtmlNodeCollection inputs = previewPage.DocumentNode.SelectNodes("//input[@name='selected_issues[]']");
        
        string issue = string.Empty;

        foreach (HtmlNode input in inputs)
            issue = input.Attributes["value"].Value;

        // Use the Issue Id to generate the certificate

        List<KeyValuePair<string, string>> formData =
        [
            new KeyValuePair<string, string>("selected_issues[]", issue),
            new KeyValuePair<string, string>("letter_template_id", "31"),
            new KeyValuePair<string, string>("letter_type", "incident"),
            new KeyValuePair<string, string>("id[]", incidentId),
            new KeyValuePair<string, string>("do_action", "print"),
            new KeyValuePair<string, string>("issue_id", "")
        ];

        byte[]? response = await GetByteArrayByPost($"{_settings.ServerUrl}/wellbeing/letters/print?format=pdf", formData);

        if (response is null)
        {
            _logger.Warning("Did not successfully generate the certificate: {@formData}", formData);
            return [];
        }

        string code = System.Text.Encoding.Default.GetString(response);

        bool readyToDownload = false;
        int retryCount = 20;

        while (!readyToDownload)
        {
            if (retryCount == 0)
                return [];

            string? progress = await GetJsonByGet($"{_settings.ServerUrl}/_common/lib/jasper_reports?action=pollQueue&user_key={code}");

            if (progress is null)
                continue;

            string status = JsonSerializerExtensions.DeserializeAnonymousType(progress, new { status = "" })?.status ?? string.Empty;
            
            if (status != "COMPLETE")
            {
                await Task.Delay(1000);
                retryCount--;
            }
            else
            {
                readyToDownload = true;
            }
        }

        byte[]? document = await GetByteArrayByGet($"{_settings.ServerUrl}/_common/lib/jasper_reports?format=pdf&key={code}&action=save");

        if (document is null)
        {
            _logger.Warning("Did not successfully download certificate: {@formData} ({code})", formData, code);
            return [];
        }

        return document;
    }

    public async Task<(Stream BasicFile, Stream DetailFile)> GetNAwardReport(CancellationToken cancellationToken = default)
    {
        Stream baseFile = await GetStreamByGet($"{_settings.ServerUrl}/wellbeing/reports/incidents?report_id=4154&export-xls&victims-witnesses=All", cancellationToken);
        Stream detailFile = await GetStreamByGet($"{_settings.ServerUrl}/wellbeing/reports/incidents?report_id=4154&export-xls&victims-witnesses=All&extra-n-award-details=true", cancellationToken);

        return (baseFile, detailFile);
    }

    public async Task<SystemAttendanceData?> GetAttendancePercentages(SchoolTerm term, SchoolWeek week, string year, DateOnly startDate, DateOnly endDate)
    {
        if (_logOnly)
        {
            _logger.Information("GetAttendancePercentages: ");

            return null;
        }

        List<KeyValuePair<string, string>> payload =
        [
            new("length", "period"),
            // year=2023
            new("year", year),
            // start_date=2023-01-01
            new("start_date", _dateTime.FirstDayOfYear.ToString("yyyy-MM-dd", DateTimeFormatInfo.CurrentInfo)),
            // end_date=2023-11-03
            new("end_date", endDate.ToString("yyyy-MM-dd", DateTimeFormatInfo.CurrentInfo)),
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
        ];

        HtmlDocument? perMinuteYearToDateDocument = await GetPageByPost(new($"{_settings.ServerUrl}/attendance/reports/percentage"), payload);

        Stream perMinuteYearToDateCalculationFile = await GetStreamByGet($"{_settings.ServerUrl}/attendancepxp/period/administration/percentage_attendance_report?length=period&year={year}&start_date={_dateTime.FirstDayOfYear.ToString("yyyy-MM-dd", DateTimeFormatInfo.CurrentInfo)}&end_date={endDate.ToString("yyyy-MM-dd", DateTimeFormatInfo.CurrentInfo)}&attendance_source=attendance&enrolled_students=true&group=years&years%5B%5D=5&years%5B%5D=6&years%5B%5D=7&years%5B%5D=8&years%5B%5D=9&years%5B%5D=10&years%5B%5D=11&years%5B%5D=12&action=export");

        payload =
        [
            new("length", "week"),
            // term=3
            new("term", term.Value),
            // week=1
            new("week", week.Value),
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
        ];

        HtmlDocument? perWeekCalculationFile = await GetPageByPost(new($"{_settings.ServerUrl}/attendance/reports/percentage"), payload);

        Stream perMinuteWeekCalculationFile = await GetStreamByGet($"{_settings.ServerUrl}/attendancepxp/period/administration/percentage_attendance_report?length=week&term={term.Value}&week={week.Value}&year={year}&attendance_source=attendance&enrolled_students=true&group=years&years%5B%5D=5&years%5B%5D=6&years%5B%5D=7&years%5B%5D=8&years%5B%5D=9&years%5B%5D=10&years%5B%5D=11&years%5B%5D=12&action=export");

        SystemAttendanceData response = new()
        {
            YearToDateDayCalculationDocument = perMinuteYearToDateDocument ?? new HtmlDocument(),
            YearToDateMinuteCalculationDocument = perMinuteYearToDateCalculationFile.IsExcelFile() ? perMinuteYearToDateCalculationFile : Stream.Null,
            WeekDayCalculationDocument = perWeekCalculationFile ?? new HtmlDocument(),
            WeekMinuteCalculationDocument = perMinuteWeekCalculationFile.IsExcelFile() ? perMinuteWeekCalculationFile : Stream.Null
        };

        return response;
    }
}
