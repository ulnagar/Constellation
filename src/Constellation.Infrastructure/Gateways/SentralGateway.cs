using Constellation.Application.DTOs;
using Constellation.Application.Extensions;
using Constellation.Application.Interfaces.GatewayConfigurations;
using Constellation.Application.Interfaces.Gateways;
using Constellation.Infrastructure.DependencyInjection;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace Constellation.Infrastructure.Gateways
{
    public class SentralGateway : ISentralGateway, IScopedService
    {
        private readonly HttpClient _client;
        private readonly ISentralGatewayConfiguration _settings;
        private HtmlDocument StudentListPage;

        public SentralGateway(ISentralGatewayConfiguration settings)
        {
            _settings = settings;

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
                catch
                {
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
                catch
                {
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

                    if (stringDate == "No period absences have been recorded for this student.")
                        return absences;

                    if (string.IsNullOrWhiteSpace(stringDate) || stringDate == "&nbsp;")
                    {
                        if (previousDate == new DateTime())
                        {
                            continue;
                        }

                        stringDate = previousDate.ToString("dd-MM-yyyy");
                    }

                    var date = DateTime.Parse(stringDate).Date;
                    previousDate = date;

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

        public async Task<List<string>> GetContactNumbersAsync(string sentralStudentId)
        {
            if (string.IsNullOrWhiteSpace(sentralStudentId))
                return new List<string>();

            var page = await GetPageAsync($"{_settings.Server}/profiles/students/{sentralStudentId}/family");

            if (page == null)
                return new List<string>();

            var phoneNumbers = new List<string>();

            var mothersMobile = page.DocumentNode.SelectSingleNode(_settings.XPaths.First(a => a.Key == "MothersMobilePhone").Value).InnerText.Trim().Replace(" ", "");
            var mothersHome = page.DocumentNode.SelectSingleNode(_settings.XPaths.First(a => a.Key == "MothersHomePhone").Value).InnerText.Trim().Replace(" ", "");
            var mothersWork = page.DocumentNode.SelectSingleNode(_settings.XPaths.First(a => a.Key == "MothersWorkPhone").Value).InnerText.Trim().Replace(" ", "");
            var fathersMobile = page.DocumentNode.SelectSingleNode(_settings.XPaths.First(a => a.Key == "FathersMobilePhone").Value).InnerText.Trim().Replace(" ", "");
            var fathersHome = page.DocumentNode.SelectSingleNode(_settings.XPaths.First(a => a.Key == "FathersHomePhone").Value).InnerText.Trim().Replace(" ", "");
            var fathersWork = page.DocumentNode.SelectSingleNode(_settings.XPaths.First(a => a.Key == "FathersWorkPhone").Value).InnerText.Trim().Replace(" ", "");

            switch (_settings.ContactPreference)
            {
                case ISentralGatewayConfiguration.ContactPreferenceOptions.MotherFirstThenFather:
                    if (mothersMobile.StartsWith("04"))
                    {
                        phoneNumbers.Add(mothersMobile);
                    }
                    else if (mothersHome.StartsWith("04"))
                    {
                        phoneNumbers.Add(mothersHome);
                    }
                    else if (mothersWork.StartsWith("04"))
                    {
                        phoneNumbers.Add(mothersWork);
                    }
                    else if (fathersMobile.StartsWith("04"))
                    {
                        phoneNumbers.Add(fathersMobile);
                    }
                    else if (fathersHome.StartsWith("04"))
                    {
                        phoneNumbers.Add(fathersHome);
                    }
                    else if (fathersWork.StartsWith("04"))
                    {
                        phoneNumbers.Add(fathersWork);
                    }

                    break;
                case ISentralGatewayConfiguration.ContactPreferenceOptions.FatherFirstThenMother:
                    if (fathersMobile.StartsWith("04"))
                    {
                        phoneNumbers.Add(fathersMobile);
                    }
                    else if (fathersHome.StartsWith("04"))
                    {
                        phoneNumbers.Add(fathersHome);
                    }
                    else if (fathersWork.StartsWith("04"))
                    {
                        phoneNumbers.Add(fathersWork);
                    }
                    else if (mothersMobile.StartsWith("04"))
                    {
                        phoneNumbers.Add(mothersMobile);
                    }
                    else if (mothersHome.StartsWith("04"))
                    {
                        phoneNumbers.Add(mothersHome);
                    }
                    else if (mothersWork.StartsWith("04"))
                    {
                        phoneNumbers.Add(mothersWork);
                    }

                    break;
                case ISentralGatewayConfiguration.ContactPreferenceOptions.BothParentsIfPresent:
                    if (mothersMobile.StartsWith("04"))
                    {
                        phoneNumbers.Add(mothersMobile);
                    }
                    else if (mothersHome.StartsWith("04"))
                    {
                        phoneNumbers.Add(mothersHome);
                    }
                    else if (mothersWork.StartsWith("04"))
                    {
                        phoneNumbers.Add(mothersWork);
                    }

                    if (fathersMobile.StartsWith("04"))
                    {
                        phoneNumbers.Add(fathersMobile);
                    }
                    else if (fathersHome.StartsWith("04"))
                    {
                        phoneNumbers.Add(fathersHome);
                    }
                    else if (fathersWork.StartsWith("04"))
                    {
                        phoneNumbers.Add(fathersWork);
                    }

                    break;
            }

            return phoneNumbers;
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

        public async Task<List<string>> GetContactEmailsAsync(string sentralStudentId)
        {
            if (string.IsNullOrWhiteSpace(sentralStudentId))
                return new List<string>();

            var page = await GetPageAsync($"{_settings.Server}/profiles/students/{sentralStudentId}/family");

            if (page == null)
                return new List<string>();

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

            return emailAddresses;
        }

        public async Task<ICollection<RollMarkReportDto>> GetRollMarkingReportAsync(DateTime date)
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
                    Submitted = (splitString[6].TrimStart('"').TrimEnd('"') == "Submitted")
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
                    Submitted = (splitString[6].TrimStart('"').TrimEnd('"') == "Submitted")
                });
            }

            return list;
        }

        public async Task<ICollection<FamilyDetailsDto>> GetFamilyDetailsReport()
        {
            var data = new List<FamilyDetailsDto>();
            
            var page = await GetPageAsync($"{_settings.Server}/enquiry/export/view_export?name=complete&inputs[class]=&inputs[roll_class]=&inputs[schyear]=&format=csv&headings=1&action=Download");

            if (page == null)
                return data;

            var list = page.DocumentNode.InnerHtml.Split('\u000A').ToList();

            // Remove first and last entry
            list.RemoveAt(0);
            list.RemoveAt(list.Count - 1);

            foreach (var entry in list)
            {
                //var split = entry.Split(',');

                var CSVParser = new Regex(",(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))");
                var split = CSVParser.Split(entry);

                if (split.Length != 90)
                {
                    split = entry.Split(',');

                    if (split.Length != 90)
                        continue;
                }

                var familyId = split[4].RemoveQuotes().RemoveWhitespace();

                if (data.Any(detail => detail.FamilyId == familyId))
                {
                    // Update existing family with additional StudentId
                    var detail = data.First(detail => detail.FamilyId == familyId);
                    detail.StudentIds.Add(split[0].RemoveQuotes().RemoveWhitespace());

                    continue;
                } else
                {
                    var detail = new FamilyDetailsDto
                    {
                        FamilyId = split[4].RemoveQuotes().RemoveWhitespace(),
                        FatherTitle = split[49].RemoveQuotes().RemoveWhitespace(),
                        FatherFirstName = split[50].RemoveQuotes().RemoveWhitespace(),
                        FatherLastName = split[51].RemoveQuotes().RemoveWhitespace(),
                        FatherMobile = split[53].RemoveQuotes().RemoveWhitespace(),
                        MotherTitle = split[61].RemoveQuotes().RemoveWhitespace(),
                        MotherFirstName = split[62].RemoveQuotes().RemoveWhitespace(),
                        MotherLastName = split[63].RemoveQuotes().RemoveWhitespace(),
                        MotherMobile = split[65].RemoveQuotes().RemoveWhitespace(),
                        FamilyEmail = split[82].RemoveQuotes().RemoveWhitespace(),
                        AddressName = split[83].RemoveQuotes().RemoveWhitespace(),
                        AddressLine1 = split[85].RemoveQuotes().RemoveWhitespace(),
                        AddressLine2 = split[86].RemoveQuotes().RemoveWhitespace(),
                        AddressTown = split[88].RemoveQuotes().RemoveWhitespace(),
                        AddressPostCode = split[89].RemoveQuotes().RemoveWhitespace()
                    };

                    detail.StudentIds.Add(split[0].RemoveQuotes().RemoveWhitespace());

                    data.Add(detail);
                }
            }

            return data;
        }
    }
}
