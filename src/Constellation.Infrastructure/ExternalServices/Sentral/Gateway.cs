﻿namespace Constellation.Infrastructure.ExternalServices.Sentral;

using Application.Attendance.GetAttendanceDataFromSentral;
using Constellation.Application.Attendance.GetValidAttendanceReportDates;
using Constellation.Application.DTOs;
using Constellation.Application.Extensions;
using Constellation.Application.Interfaces.Configuration;
using Constellation.Application.Interfaces.Gateways;
using Core.Abstractions.Clock;
using Core.Shared;
using ExcelDataReader;
using HtmlAgilityPack;
using Microsoft.Extensions.Options;
using OfficeOpenXml;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Net.Http.Headers;
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
    }

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

    private async Task<HtmlDocument> GetPageByPost(string uri, List<KeyValuePair<string, string>> payload, CancellationToken cancellationToken)
    {
        for (int i = 1; i < 6; i++)
        {
            try
            {
                await Login(cancellationToken);

                HttpResponseMessage response = await _client.PostAsync(uri, new FormUrlEncodedContent(payload), cancellationToken);
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

        var page = _studentListPage;

        var studentTable = page.DocumentNode.SelectSingleNode(_settings.XPaths.First(a => a.Key == "StudentTable").Value);

        if (studentTable != null)
        {
            foreach (var row in studentTable.Descendants("tr"))
            {
                var cell = row.ChildNodes.FindFirst("td");
                var sentralName = cell.InnerText.Trim();
                var sentralFirst = sentralName.Split(',')[1].Trim().ToLower();
                var sentralLast = sentralName.Split(',')[0].Trim().ToLower();

                if (studentName.ToLower() == $"{sentralFirst} {sentralLast}")
                {
                    var href = cell.ChildNodes.FindFirst("a").GetAttributeValue("href", "");
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
        var page = await GetPageByGet($"{_settings.ServerUrl}/profiles/students/{sentralStudentId}/academic-history", default);

        var dataList = new List<SentralReportDto>();

        if (page == null)
            return dataList;

        var menuItems = page.DocumentNode.SelectNodes("//*[@id='reporting_period']/option");

        if (menuItems == null || menuItems.Count == 0)
            return dataList;

        foreach (var menuItem in menuItems)
        {
            var linkRef = menuItem.GetAttributeValue("value", "");
            if (string.IsNullOrWhiteSpace(linkRef))
                continue;

            var reportPage = await GetPageByGet($"{_settings.ServerUrl}/profiles/students/{sentralStudentId}/academic-history?type=sreport&page=printed_report&reporting_period={linkRef}", default);
            reportTable = reportPage.DocumentNode.SelectSingleNode("//*[@id='layout-2col-content']/div/div/div[2]/table/tbody");

            if (reportTable != null)
                break;
        }

        if (reportTable != null)
        {
            foreach (var row in reportTable.Descendants("tr"))
            {
                var cellNumber = 0;

                var entry = new SentralReportDto();

                // Process Row!
                foreach (var cell in row.Descendants("td"))
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
                            var link = cell.FirstChild.GetAttributeValue("onclick", "downloadFile(0)");
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

        var page = await GetPageByGet($"{_settings.ServerUrl}/attendancepxp/administration/student?id={sentralStudentId}", default);

        if (page == null)
            return new List<SentralPeriodAbsenceDto>();

        var absenceTable = page.DocumentNode.SelectSingleNode(_settings.XPaths.First(a => a.Key == "AbsenceTable").Value);

        var absences = new List<SentralPeriodAbsenceDto>();

        if (absenceTable != null)
        {
            var rows = absenceTable.Descendants("tr");
            var previousDate = new DateOnly();

            foreach (var row in rows)
            {
                var dateCell = row.ChildNodes.FindFirst("td");
                var stringDate = dateCell.InnerText.Trim();
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

                var periodAbsence = new SentralPeriodAbsenceDto
                {
                    Date = date
                };

                var cellNumber = 0;
                // Process Row!
                foreach (var cell in row.Descendants("td"))
                {
                    cellNumber++;

                    switch (cellNumber)
                    {
                        case 1:
                        case 6:
                        case 7:
                            break;
                        case 2:
                            var periodsText = cell.InnerText.Trim().Split(' ');
                            periodAbsence.Period = periodsText[0].Trim();
                            periodAbsence.ClassName = periodsText[2].Trim();
                            break;
                        case 3:
                            var absenceTypeText = cell.InnerText.Trim();
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
                                        var stringMinutes = absenceTypeText.Split('(')[1].Split(')')[0];
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

    public async Task<List<SentralPeriodAbsenceDto>> GetPartialAbsenceDataAsync(string sentralStudentId)
    {
        var term1 = await GetPartialAbsenceDataForTerm(sentralStudentId, 1);
        var term2 = await GetPartialAbsenceDataForTerm(sentralStudentId, 2);
        var term3 = await GetPartialAbsenceDataForTerm(sentralStudentId, 3);
        var term4 = await GetPartialAbsenceDataForTerm(sentralStudentId, 4);

        var returnData = new List<SentralPeriodAbsenceDto>();
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

        var page = await GetPageByGet($"{_settings.ServerUrl}/attendance/administration/student/{sentralStudentId}?term={term}", default);

        if (page == null)
            return new List<SentralPeriodAbsenceDto>();

        var absenceTable = page.DocumentNode.SelectSingleNode(_settings.XPaths.First(a => a.Key == "PartialAbsenceTable").Value);

        var detectedAbsences = new List<SentralPeriodAbsenceDto>();

        if (absenceTable != null)
        {
            var rows = absenceTable.Descendants("tr");

            foreach (var row in rows)
            {
                var absence = new SentralPeriodAbsenceDto();
                var firstTD = row.ChildNodes.FindFirst("td");

                // The weekly heading rows are TH with no TD tags, so we should just skip these.
                if (firstTD == null)
                    continue;

                var input = firstTD.ChildNodes.FindFirst("input");
                var value = input.Attributes.AttributesWithName("value");
                var rowData = value.FirstOrDefault()?.Value;

                // If this row has no absence data, the value is a numerical id, not a date.
                if (rowData == null || !rowData.Contains('-'))
                    continue;

                var stringDate = rowData[..rowData.IndexOf('_')];
                var rowDate = DateOnly.Parse(stringDate);
                absence.Date = rowDate;

                var cellNumber = 0;
                foreach (var cell in row.Descendants("td"))
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
                                var startTimeSuccess = TimeOnly.TryParseExact(absence.Timeframe.Split(' ')[0], "h:mmtt", CultureInfo.InvariantCulture, DateTimeStyles.None, out TimeOnly startTime);
                                if (startTimeSuccess)
                                    absence.StartTime = startTime;
                                else
                                    _logger
                                        .ForContext("DetectedTime", absence.Timeframe.Split(' ')[0])
                                        .ForContext("AbsenceDate", absence.Date)
                                        .ForContext("SentralStudentId", sentralStudentId)
                                        .Information("Error parsing absence start time to TimeOnly object");

                                var endTimeSuccess = TimeOnly.TryParseExact(absence.Timeframe.Split(' ')[2], "h:mmtt", CultureInfo.InvariantCulture, DateTimeStyles.None, out TimeOnly endTime);
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

                            var spans = cell.SelectNodes("span");

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

    public async Task<List<DateOnly>> GetExcludedDatesFromCalendar(string year)
    {
        if (_logOnly)
        {
            _logger.Information("GetExcludedDatesFromCalendar: year={year}", year);

            return new List<DateOnly>();
        }

        var page = await GetPageByGet($"{_settings.ServerUrl}/admin/settings/school/calendar/{year}/month", default);

        if (page == null)
            return new List<DateOnly>();

        var calendarTable = page.DocumentNode.SelectSingleNode(_settings.XPaths.First(a => a.Key == "CalendarTable").Value);

        var nonSchoolDays = new List<DateOnly>();

        if (calendarTable != null)
        {
            var rows = calendarTable.Descendants("tr");

            foreach (var row in rows)
            {
                var days = row.Descendants("td");

                foreach (var day in days)
                {
                    if (day.HasClass("school-break") || day.HasClass("holiday") || day.HasClass("holiday-once"))
                    {
                        var action = day.GetAttributeValue("onclick", "");
                        if (!string.IsNullOrWhiteSpace(action))
                        {
                            var detectedDate = action.Split('\'')[1];
                            var date = DateOnly.Parse(detectedDate);

                            nonSchoolDays.Add(date);
                        }
                    }
                }
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

        HtmlNode calendarTable = page.DocumentNode.SelectSingleNode(_settings.XPaths.First(a => a.Key == "TermCalendarTable").Value);

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

        DateOnly startDate;
        DateOnly endDate;

        HtmlDocument page = await GetPageByGet($"{_settings.ServerUrl}/admin/settings/school/calendar/{year}/term", default);

        if (page == null)
            return Result.Failure<(DateOnly, DateOnly)>(new("SentralGateway.GetPage.Failure", "Could not retrieve page from Sentral Server"));

        HtmlNode calendarTable = page.DocumentNode.SelectSingleNode(_settings.XPaths.First(a => a.Key == "TermCalendarTable").Value);

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
                    var weekName = row.Descendants("th").FirstOrDefault();

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

        var validDates = new List<ValidAttendenceReportDate>();

        var page = await GetPageByGet($"{_settings.ServerUrl}/admin/settings/school/calendar/{year}/term", default);

        if (page == null)
            return validDates;

        var calendarTable = page.DocumentNode.SelectSingleNode(_settings.XPaths.First(a => a.Key == "TermCalendarTable").Value);

        if (calendarTable != null)
        {
            var TermName = "";
            var WeekName = "";

            var rows = calendarTable.Descendants("tr").ToList();

            for (int row = 0; row < rows.Count - 1; row++)
            {
                var firstChildNode = rows[row].ChildNodes.Where(node => node.Name != "#text").First();

                if (firstChildNode.Name == "td" && firstChildNode.GetAttributeValue("colspan", 0) > 1)
                {
                    // This is a header row with the term name
                    var nodes = firstChildNode.Descendants("b");
                    foreach (var node in nodes)
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

                        var startDateAction = rows[row].ChildNodes.Where(node => node.Name != "#text").ToArray()[1].GetAttributeValue("onclick", "");
                        if (!string.IsNullOrWhiteSpace(startDateAction))
                        {
                            var detectedDate = startDateAction.Split('\'')[1];
                            var date = DateOnly.Parse(detectedDate);

                            startDate = date;
                        }

                        var endDateAction = rows[row].ChildNodes.Where(node => node.Name != "#text").ToArray()[5].GetAttributeValue("onclick", "");
                        if (!string.IsNullOrWhiteSpace(endDateAction))
                        {
                            var detectedDate = endDateAction.Split('\'')[1];
                            var date = DateOnly.Parse(detectedDate);

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

                        var startDateAction = rows[row].ChildNodes.Where(node => node.Name != "#text").ToArray()[1].GetAttributeValue("onclick", "");
                        if (!string.IsNullOrWhiteSpace(startDateAction))
                        {
                            var detectedDate = startDateAction.Split('\'')[1];
                            var date = DateOnly.Parse(detectedDate);

                            startDate = date;
                        }

                        var endDateAction = rows[row + 1].ChildNodes.Where(node => node.Name != "#text").ToArray()[5].GetAttributeValue("onclick", "");
                        if (!string.IsNullOrWhiteSpace(endDateAction))
                        {
                            var detectedDate = endDateAction.Split('\'')[1];
                            var date = DateOnly.Parse(detectedDate);

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

    public async Task<IDictionary<string, IDictionary<string, string>>> GetParentContactEntry(string sentralStudentId)
    {
        if (_logOnly)
        {
            _logger.Information("GetParentContactEntry: sentralStudentId={sentralStudentId}", sentralStudentId);

            return new Dictionary<string, IDictionary<string, string>>();
        }

        var result = new Dictionary<string, IDictionary<string, string>>();

        if (string.IsNullOrWhiteSpace(sentralStudentId))
            return result;

        var page = await GetPageByGet($"{_settings.ServerUrl}/profiles/students/{sentralStudentId}/family", default);

        if (page == null)
            return result;

        var familyName = HttpUtility.HtmlDecode(page.DocumentNode.SelectSingleNode(_settings.XPaths.First(a => a.Key == "FamilyName").Value).InnerHtml.Trim().Split('<')[0]);
        var lastName = familyName.Split(' ').Last();
        var firstName = familyName[..^lastName.Length].Trim();

        var emailAddresses = new List<string>();

        var mothersEmail = page.DocumentNode.SelectSingleNode(_settings.XPaths.First(a => a.Key == "MothersEmail").Value).InnerText.Trim();
        var fathersEmail = page.DocumentNode.SelectSingleNode(_settings.XPaths.First(a => a.Key == "FathersEmail").Value).InnerText.Trim();


        switch (_settings.ContactPreference)
        {
            case SentralGatewayConfiguration.ContactPreferenceOptions.MotherThenFather:
                if (!string.IsNullOrWhiteSpace(mothersEmail))
                {
                    emailAddresses.Add(mothersEmail);
                }
                else if (!string.IsNullOrWhiteSpace(fathersEmail))
                {
                    emailAddresses.Add(fathersEmail);
                }

                break;
            case SentralGatewayConfiguration.ContactPreferenceOptions.FatherThenMother:
                if (!string.IsNullOrWhiteSpace(fathersEmail))
                {
                    emailAddresses.Add(fathersEmail);
                }
                else if (!string.IsNullOrWhiteSpace(mothersEmail))
                {
                    emailAddresses.Add(mothersEmail);
                }

                break;
            case SentralGatewayConfiguration.ContactPreferenceOptions.Both:
                if (!string.IsNullOrWhiteSpace(mothersEmail))
                {
                    emailAddresses.Add(mothersEmail);
                }

                if (!string.IsNullOrWhiteSpace(fathersEmail))
                {
                    emailAddresses.Add(fathersEmail);
                }

                break;
        }

        emailAddresses = emailAddresses.Distinct().ToList();

        foreach (var email in emailAddresses)
        {
            var entry = new Dictionary<string, string>
            {
                { "FirstName", firstName },
                { "LastName", lastName },
                { "EmailAddress", email }
            };

            result.Add(email, entry);
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

        List<FamilyDetailsDto> data = new List<FamilyDetailsDto>();
        
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

                if (existingEntry.StudentIds.All(entry => entry != studentId))
                    existingEntry.StudentIds.Add(studentId);
            }
            else
            {
                FamilyDetailsDto detail = new FamilyDetailsDto
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

                detail.StudentIds.Add(row[0].ToString().FormatField());

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

            FamilyDetailsDto detail = data.FirstOrDefault(item => item.StudentIds.Contains(studentId));

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

        return data.Where(entry => entry.StudentIds.Any()).ToList();
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

        HtmlDocument report = await GetPageByPost($"{_settings.ServerUrl}/wellbeing/awards/export", payload, cancellationToken);

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

    public async Task<Stream> GetNAwardReport()
    {
        Stream file = await GetStreamByGet($"{_settings.ServerUrl}/wellbeing/reports/incidents?report_id=4154&export-xls&victims-witnesses=All", default);

        return file;
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

        response.YearToDateDayCalculationDocument = await GetPageByPost($"{_settings.ServerUrl}/attendance/reports/percentage", payload, default);

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

        response.WeekDayCalculationDocument = await GetPageByPost($"{_settings.ServerUrl}/attendance/reports/percentage", payload, default);

        Stream perMinuteWeekCalculationFile = await GetStreamByGet($"{_settings.ServerUrl}/attendancepxp/period/administration/percentage_attendance_report?length=week&term={term}&week={week}&year={year}&attendance_source=attendance&enrolled_students=true&group=years&years%5B%5D=5&years%5B%5D=6&years%5B%5D=7&years%5B%5D=8&years%5B%5D=9&years%5B%5D=10&years%5B%5D=11&years%5B%5D=12&action=export", default);
        response.WeekMinuteCalculationDocument = perMinuteWeekCalculationFile.IsExcelFile() ? perMinuteWeekCalculationFile : null;

        return response;
    }
}
