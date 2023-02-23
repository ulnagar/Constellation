using Constellation.Application.DTOs;
using Constellation.Application.Extensions;
using Constellation.Application.Interfaces.GatewayConfigurations;
using Constellation.Application.Interfaces.Gateways;
using Constellation.Infrastructure.DependencyInjection;
using HtmlAgilityPack;
using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Web;

namespace Constellation.Infrastructure.ExternalServices.Sentral
{
    public partial class Gateway : ISentralGateway, IScopedService
    {
        private readonly HttpClient _client;
        private readonly ISentralGatewayConfiguration _settings;
        private readonly ILogger _logger;
        private HtmlDocument StudentListPage;

        public Gateway(ISentralGatewayConfiguration settings, ILogger logger)
        {
            _settings = settings;
            _logger = logger.ForContext<ISentralGateway>();

            var config = new HttpClientHandler
            {
                CookieContainer = new CookieContainer()
            };

            var proxy = WebRequest.DefaultWebProxy;
            config.UseProxy = true;
            config.Proxy = proxy;

            _client = new HttpClient(config);
        }

        private async Task Login()
        {
            var uri = $"{_settings.Server}/check_login";

            var request = new HttpRequestMessage();
            request.Headers.CacheControl = new CacheControlHeaderValue
            {
                NoCache = true
            };

            var formData = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("sentral-username", _settings.Username),
                new KeyValuePair<string, string>("sentral-password", _settings.Password)
            };
            var formDataEncoded = new FormUrlEncodedContent(formData);

            for (int i = 1; i < 6; i++)
            {
                try
                {
                    var response = await _client.PostAsync(uri, formDataEncoded);
                    response.EnsureSuccessStatusCode();

                    return;
                }
                catch (Exception ex)
                {
                    _logger.Warning($"Failed to login to Sentral Server with error: {ex.Message}");
                    if (ex.InnerException != null)
                        _logger.Warning($"Inner Exception: {ex.InnerException.Message}");

                    // Wait and retry
                    await Task.Delay(5000);
                }
            }

            throw new Exception($"Could not connect to Sentral Server");
        }

        private async Task<HtmlDocument> GetPageAsync(string uri)
        {
            for (int i = 1; i < 6; i++)
            {
                try
                {
                    await Login();

                    var response = await _client.GetAsync(uri);

                    var page = new HtmlDocument();
                    page.LoadHtml(await response.Content.ReadAsStringAsync());

                    return page;
                }
                catch (Exception ex)
                {
                    _logger.Warning($"Failed to retrieve information from Sentral Server with error: {ex.Message}");
                    if (ex.InnerException != null)
                        _logger.Warning($"Inner Exception: {ex.InnerException.Message}");

                    // Wait and retry
                    await Task.Delay(5000);
                }
            }

            return null;
        }

        private async Task<HtmlDocument> PostPageAsync(string uri, List<KeyValuePair<string, string>> payload)
        {
            for (int i = 1; i < 6; i++)
            {
                try
                {
                    await Login();

                    var request = new HttpRequestMessage(HttpMethod.Post, uri)
                    {
                        Content = new FormUrlEncodedContent(payload)
                    };

                    var response = await _client.SendAsync(request);

                    //var response = await _client.PostAsJsonAsync(uri, content);

                    var page = new HtmlDocument();
                    page.LoadHtml(await response.Content.ReadAsStringAsync());

                    return page;
                }
                catch (Exception ex)
                {
                    _logger.Warning($"Failed to retrieve information from Sentral Server with error: {ex.Message}");
                    if (ex.InnerException != null)
                        _logger.Warning($"Inner Exception: {ex.InnerException.Message}");

                    // Wait and retry
                    await Task.Delay(5000);
                }
            }

            return null;
        }

        public async Task<string> GetSentralStudentIdAsync(string studentName)
        {
            if (StudentListPage == null)
                StudentListPage = await GetPageAsync($"{_settings.Server}/profiles/main/search?eduproq=&search=advanced&plan_type=plans");

            if (StudentListPage == null)
                return null;

            var page = StudentListPage;

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
            HtmlNode reportTable = null;
            var page = await GetPageAsync($"{_settings.Server}/profiles/students/{sentralStudentId}/academic-history");

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

                var reportPage = await GetPageAsync($"{_settings.Server}/profiles/students/{sentralStudentId}/academic-history?type=sreport&page=printed_report&reporting_period={linkRef}");
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
            await Login();

            var formData = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("file_id", reportId),
                new KeyValuePair<string, string>("action", "download_file")
            };
            var formDataEncoded = new FormUrlEncodedContent(formData);

            var response = await _client.PostAsync($"{_settings.Server}/profiles/students/{sentralStudentId}/academic-history?type=sreport&page=printed_report", formDataEncoded);

            return await response.Content.ReadAsByteArrayAsync();
        }

        public async Task<byte[]> GetSentralStudentPhoto(string studentId)
        {
            await Login();

            var response = await _client.GetAsync($"{_settings.Server}/_common/lib/photo?type=student&id={studentId}&w=250&h=250");

            return await response.Content.ReadAsByteArrayAsync();
        }

        public async Task<string> GetSentralStudentIdFromSRN(string srn, string grade)
        {
            var page = await GetPageAsync($"{_settings.Server}/admin/datasync/students?year={grade}&type=active");

            if (page == null)
                return null;

            var studentTable = page.DocumentNode.SelectSingleNode("/html/body/div[6]/div/div/div[3]/div/div/div/div[2]/table");

            if (studentTable != null)
            {
                foreach (var row in studentTable.Descendants("tr"))
                {
                    var cell = row.ChildNodes.FindFirst("td");
                    if (cell == null)
                        continue;

                    var sentralSRN = cell.InnerText.Trim();

                    if (sentralSRN == srn)
                    {
                        var href = cell.ChildNodes.FindFirst("a").GetAttributeValue("href", "");
                        if (string.IsNullOrWhiteSpace(href))
                        {
                            // Something went wrong? What now?
                            throw new NodeAttributeNotFoundException();
                        }
                        else
                        {
                            return href.Split('=').Last();
                        }
                    }
                }
            }

            return null;
        }

        public async Task<List<SentralPeriodAbsenceDto>> GetAbsenceDataAsync(string sentralStudentId)
        {
            var page = await GetPageAsync($"{_settings.Server}/attendancepxp/administration/student?id={sentralStudentId}");

            if (page == null)
                return new List<SentralPeriodAbsenceDto>();

            var absenceTable = page.DocumentNode.SelectSingleNode(_settings.XPaths.First(a => a.Key == "AbsenceTable").Value);

            var absences = new List<SentralPeriodAbsenceDto>();

            if (absenceTable != null)
            {
                var rows = absenceTable.Descendants("tr");
                var previousDate = new DateTime();

                foreach (var row in rows)
                {
                    var dateCell = row.ChildNodes.FindFirst("td");
                    var stringDate = dateCell.InnerText.Trim();
                    DateTime date;

                    if (stringDate == "No period absences have been recorded for this student.")
                        return absences;

                    if (string.IsNullOrWhiteSpace(stringDate) || stringDate == "&nbsp;")
                    {
                        if (previousDate == new DateTime())
                        {
                            continue;
                        }

                        //stringDate = previousDate.ToString("dd-MM-yyyy");
                        date = previousDate;
                    }
                    else
                    {
                        date = DateTime.Parse(stringDate).Date;
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
                                switch (absenceTypeText.Substring(0, 4))
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
            var page = await GetPageAsync($"{_settings.Server}/attendance/administration/student/{sentralStudentId}?term={term}");

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

                    var stringDate = rowData.Substring(0, rowData.IndexOf('_'));
                    var rowDate = DateTime.Parse(stringDate);
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
                                    var startDateTime = DateTime.ParseExact(absence.Timeframe.Split(' ')[0], "h:mmtt", CultureInfo.InvariantCulture);
                                    absence.StartTime = startDateTime.TimeOfDay;

                                    var endDateTime = DateTime.ParseExact(absence.Timeframe.Split(' ')[2], "h:mmtt", CultureInfo.InvariantCulture);
                                    absence.EndTime = endDateTime.TimeOfDay;
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

        public async Task<List<DateTime>> GetExcludedDatesFromCalendar(string year)
        {
            var page = await GetPageAsync($"{_settings.Server}/admin/settings/school/calendar/{year}/month");

            if (page == null)
                return new List<DateTime>();

            var calendarTable = page.DocumentNode.SelectSingleNode(_settings.XPaths.First(a => a.Key == "CalendarTable").Value);

            var nonSchoolDays = new List<DateTime>();

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
                                var date = DateTime.Parse(detectedDate);

                                nonSchoolDays.Add(date);
                            }
                        }
                    }
                }
            }

            return nonSchoolDays.OrderBy(a => a).ToList();
        }

        public async Task<List<ValidAttendenceReportDate>> GetValidAttendanceReportDatesFromCalendar(string year)
        {
            var validDates = new List<ValidAttendenceReportDate>();

            var page = await GetPageAsync($"{_settings.Server}/admin/settings/school/calendar/{year}/term");

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

                            var entry = new ValidAttendenceReportDate
                            {
                                Description = $"{TermName} {WeekName}",
                                TermGroup = TermName
                            };

                            var startDateAction = rows[row].ChildNodes.Where(node => node.Name != "#text").ToArray()[1].GetAttributeValue("onclick", "");
                            if (!string.IsNullOrWhiteSpace(startDateAction))
                            {
                                var detectedDate = startDateAction.Split('\'')[1];
                                var date = DateTime.Parse(detectedDate);

                                entry.StartDate = date;
                            }

                            var endDateAction = rows[row].ChildNodes.Where(node => node.Name != "#text").ToArray()[5].GetAttributeValue("onclick", "");
                            if (!string.IsNullOrWhiteSpace(endDateAction))
                            {
                                var detectedDate = endDateAction.Split('\'')[1];
                                var date = DateTime.Parse(detectedDate);

                                entry.EndDate = date;
                            }

                            validDates.Add(entry);
                        }
                        else
                        {
                            // This is a calendar row starting with the week number
                            WeekName = $"Week {firstChildNode.InnerText.Trim()} - Week {rows[row + 1].ChildNodes.Where(node => node.Name != "#text").First().InnerText.Trim()}";

                            var entry = new ValidAttendenceReportDate
                            {
                                Description = $"{TermName} {WeekName}",
                                TermGroup = TermName
                            };

                            var startDateAction = rows[row].ChildNodes.Where(node => node.Name != "#text").ToArray()[1].GetAttributeValue("onclick", "");
                            if (!string.IsNullOrWhiteSpace(startDateAction))
                            {
                                var detectedDate = startDateAction.Split('\'')[1];
                                var date = DateTime.Parse(detectedDate);

                                entry.StartDate = date;
                            }

                            var endDateAction = rows[row + 1].ChildNodes.Where(node => node.Name != "#text").ToArray()[5].GetAttributeValue("onclick", "");
                            if (!string.IsNullOrWhiteSpace(endDateAction))
                            {
                                var detectedDate = endDateAction.Split('\'')[1];
                                var date = DateTime.Parse(detectedDate);

                                entry.EndDate = date;
                            }

                            validDates.Add(entry);

                            row++;
                        }
                    }
                }
            }

            return validDates.OrderBy(date => date.StartDate).ToList();
        }

        public async Task<IDictionary<string, IDictionary<string, string>>> GetParentContactEntry(string sentralStudentId)
        {
            var result = new Dictionary<string, IDictionary<string, string>>();

            if (string.IsNullOrWhiteSpace(sentralStudentId))
                return result;

            var page = await GetPageAsync($"{_settings.Server}/profiles/students/{sentralStudentId}/family");

            if (page == null)
                return result;

            var familyName = HttpUtility.HtmlDecode(page.DocumentNode.SelectSingleNode(_settings.XPaths.First(a => a.Key == "FamilyName").Value).InnerHtml.Trim().Split('<')[0]);
            var lastName = familyName.Split(' ').Last();
            var firstName = familyName.Substring(0, familyName.Length - lastName.Length).Trim();

            var emailAddresses = new List<string>();

            var mothersEmail = page.DocumentNode.SelectSingleNode(_settings.XPaths.First(a => a.Key == "MothersEmail").Value).InnerText.Trim();
            var fathersEmail = page.DocumentNode.SelectSingleNode(_settings.XPaths.First(a => a.Key == "FathersEmail").Value).InnerText.Trim();


            switch (_settings.ContactPreference)
            {
                case ISentralGatewayConfiguration.ContactPreferenceOptions.MotherFirstThenFather:
                    if (!string.IsNullOrWhiteSpace(mothersEmail))
                    {
                        emailAddresses.Add(mothersEmail);
                    }
                    else if (!string.IsNullOrWhiteSpace(fathersEmail))
                    {
                        emailAddresses.Add(fathersEmail);
                    }

                    break;
                case ISentralGatewayConfiguration.ContactPreferenceOptions.FatherFirstThenMother:
                    if (!string.IsNullOrWhiteSpace(fathersEmail))
                    {
                        emailAddresses.Add(fathersEmail);
                    }
                    else if (!string.IsNullOrWhiteSpace(mothersEmail))
                    {
                        emailAddresses.Add(mothersEmail);
                    }

                    break;
                case ISentralGatewayConfiguration.ContactPreferenceOptions.BothParentsIfPresent:
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
            var sentralDate = date.ToString("yyyy-MM-dd");

            var primaryPage = await GetPageAsync($"{_settings.Server}/attendancepxp/period/administration/roll_report?campus_id={1}&range=single_day&date={sentralDate}&export=1");
            var secondaryPage = await GetPageAsync($"{_settings.Server}/attendancepxp/period/administration/roll_report?campus_id={2}&range=single_day&date={sentralDate}&export=1");

            if (primaryPage == null || secondaryPage == null)
                return new List<RollMarkReportDto>();

            var primaryList = new List<string>();
            if (!primaryPage.DocumentNode.InnerHtml.StartsWith("<"))
            {
                primaryList = primaryPage.DocumentNode.InnerHtml.Split('\u000A').ToList();
            }

            var secondaryList = new List<string>();
            if (!secondaryPage.DocumentNode.InnerHtml.StartsWith("<"))
            {
                secondaryList = secondaryPage.DocumentNode.InnerHtml.Split('\u000A').ToList();
            }

            var list = new List<RollMarkReportDto>();

            foreach (var entry in primaryList)
            {
                var splitString = entry.Split(',');
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

            foreach (var entry in secondaryList)
            {
                var splitString = entry.Split(',');
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
            var data = new List<FamilyDetailsDto>();

            var CSVParser = new Regex(",(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))");

            var familiesPage = await GetPageAsync($"{_settings.Server}/enquiry/export/view_export?name=families&format=csv&headings=1&action=Download");
            var emailPage = await GetPageAsync($"{_settings.Server}/enquiry/export/view_export?name=email&inputs[only_eldest]=no&inputs[addresses]=all&format=csv&headings=1&action=Download");
            var linkPage = await GetPageAsync($"{_settings.Server}/enquiry/export/view_export?name=advstudent&inputs%5Bclass%5D=&inputs%5Broll_class%5D=&format=csv&headings=1&action=Download");

            if (familiesPage == null || emailPage == null || linkPage == null)
                return data;

            var list = familiesPage.DocumentNode.InnerHtml.Split('\u000A').ToList();

            // Remove first and last entry
            list.RemoveAt(0);
            list.RemoveAt(list.Count - 1);

            for (var i = 0; i < list.Count; i++)
            {
                var entry = list[i];
                var split = CSVParser.Split(entry);

                if (split.Length != 41)
                {
                    split = entry.Split(',');

                    if (split.Length != 41)
                    {
                        continue;
                    }
                }

                var familyId = split[0].FormatField();

                if (data.Any(detail => detail.FamilyId == familyId))
                {
                    continue;
                }
                else
                {
                    var detail = new FamilyDetailsDto
                    {
                        FamilyId = split[0].FormatField(),
                        FatherTitle = split[9].FormatField(),
                        FatherFirstName = split[10].FormatField(),
                        FatherLastName = split[11].FormatField(),
                        FatherMobile = split[13].FormatField(),
                        MotherTitle = split[21].FormatField(),
                        MotherFirstName = split[22].FormatField(),
                        MotherLastName = split[23].FormatField(),
                        MotherMobile = split[25].FormatField(),
                        AddressName = split[1].FormatField(),
                        AddressLine1 = split[2].FormatField(),
                        AddressLine2 = split[3].FormatField(),
                        AddressTown = split[4].FormatField(),
                        AddressPostCode = split[5].FormatField()
                    };

                    data.Add(detail);
                }
            }

            var linkList = linkPage.DocumentNode.InnerHtml.Split('\u000A').ToList();
            linkList.RemoveAt(0);
            linkList.RemoveAt(linkList.Count - 1);

            var familyLink = new Dictionary<string, string>();

            foreach (var link in linkList)
            {
                var splitLink = CSVParser.Split(link);

                if (splitLink.Length < 4)
                    continue;

                if (!familyLink.ContainsKey(splitLink[0]))
                {
                    familyLink.Add(splitLink[0], splitLink[3]);
                }
            }

            list = emailPage.DocumentNode.InnerHtml.Split('\u000A').ToList();

            // Remove first and last entry
            list.RemoveAt(0);
            list.RemoveAt(list.Count - 1);

            foreach (var entry in list)
            {
                var split = CSVParser.Split(entry);

                if (split.Length != 11)
                {
                    split = entry.Split(',');

                    if (split.Length != 11)
                        continue;
                }

                var studentId = split[0].FormatField();

                var familyLinkItem = familyLink.FirstOrDefault(item => item.Key == studentId);

                if (familyLinkItem.Value == null)
                {
                    // Family was not found. Why?
                    logger.Warning("Student {studentId} is not found in any active family", studentId);

                    continue;
                }

                var dataItem = data.FirstOrDefault(item => item.FamilyId == familyLinkItem.Value);

                if (dataItem == null)
                {
                    // This student is not part of a family?
                    logger.Warning("Student {studentId} is not found in any active family", studentId);

                    continue;
                }

                dataItem.StudentIds.Add(studentId);
                dataItem.FamilyEmail = split[8].FormatEmail();

                if (dataItem.FatherFirstName != null && dataItem.MotherFirstName == null)
                {
                    // Single parent family, father has custody
                    dataItem.FatherEmail = split[9].FormatEmail();
                }
                else if (dataItem.FatherFirstName == null && dataItem.MotherFirstName != null)
                {
                    // Single parent family, mother has custody
                    dataItem.MotherEmail = split[9].FormatEmail();
                }
                else
                {
                    // Dual parent family, mother first, then father
                    dataItem.MotherEmail = split[9].FormatEmail();
                    dataItem.FatherEmail = split[10].FormatEmail();
                }
            }

            var noStudents = data.Where(entry => !entry.StudentIds.Any()).ToList();

            return data.Where(entry => entry.StudentIds.Any()).ToList();
        }

        public async Task<ICollection<AwardDetailDto>> GetAwardsReport()
        {
            var data = new List<AwardDetailDto>();

            var CSVParser = new Regex(",(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))");

            var payload = new List<KeyValuePair<string, string>>();
            payload.Add(new KeyValuePair<string, string>("action", "exportStudentAwards"));

            var report = await PostPageAsync($"{_settings.Server}/wellbeing/awards/export", payload);

            if (report == null)
                return data;

            var list = report.DocumentNode.InnerHtml.Split('\u000A').ToList();

            // Remove first and last entry
            list.RemoveAt(0);
            list.RemoveAt(list.Count - 1);

            for (var i = 0; i < list.Count; i++)
            {
                var entry = list[i];
                var split = CSVParser.Split(entry);

                // Index 0 = Award Category
                // Index 1 = Award Type
                // Index 2 = Awarded Date
                // Index 3 = Award Created DateTime
                // Index 4 = Award Source
                // Index 5 = Student Id (Sentral)
                // Index 6 = Student Id (SRN)
                // Index 7 = First Name
                // Index 8 = Last Name

                data.Add(AwardDetailDto.ConvertFromFileLine(split));
            }

            return data;
        }
    }
}
