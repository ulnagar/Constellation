﻿namespace Constellation.Application.Interfaces.Gateways;

using Constellation.Application.Domains.Attendance.Reports.Queries.GetValidAttendanceReportDates;
using Constellation.Application.Domains.MeritAwards.Awards.Enums;
using Constellation.Application.DTOs;
using Constellation.Core.Enums;
using Constellation.Core.Models.Students.Enums;
using Constellation.Core.Models.Students.ValueObjects;
using Constellation.Core.Shared;
using Domains.Attendance.Reports.Commands.UpdateAttendanceDataForPeriodFromSentral;
using HtmlAgilityPack;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

public interface ISentralGateway
{
    Task<string> GetSentralStudentIdAsync(string studentName);
    Task<List<SentralPeriodAbsenceDto>> GetAbsenceDataAsync(string sentralStudentId);
    Task<List<SentralPeriodAbsenceDto>> GetPartialAbsenceDataAsync(string sentralStudentId);
    Task<string> GetSentralStudentIdFromSRN(string srn, string grade);
    Task<Dictionary<string, List<string>>> GetFamilyGroupings();
    Task<FamilyDetailsDto> GetParentContactEntry(string sentralStudentId);
    Task<List<DateOnly>> GetExcludedDatesFromCalendar(string year);
    Task<List<ValidAttendenceReportDate>> GetValidAttendanceReportDatesFromCalendar(string year);
    Task<ICollection<RollMarkReportDto>> GetRollMarkingReportAsync(DateOnly date);
    Task<ICollection<FamilyDetailsDto>> GetFamilyDetailsReport(ILogger logger);
    Task<byte[]> GetSentralStudentPhoto(string studentId);
    Task<ICollection<SentralReportDto>> GetStudentReportList(string sentralStudentId);
    Task<byte[]> GetStudentReport(string sentralStudentId, string reportId);
    Task<HtmlDocument> GetAwardsReport(CancellationToken cancellationToken = default);
    Task<HtmlDocument> GetAwardsListing(string sentralStudentId, string calYear, CancellationToken cancellationToken = default);
    Task<HtmlDocument> GetIncidentDetailsPage(string uri, CancellationToken cancellationToken = default);
    Task<byte[]> GetAwardDocument(string sentralStudentId, string incidentId);
    Task<SystemAttendanceData> GetAttendancePercentages(SchoolTerm term, SchoolWeek week, string year, DateOnly startDate, DateOnly endDate);
    Task<Result<(DateOnly StartDate, DateOnly EndDate)>> GetDatesForWeek(string year, SchoolTerm term, SchoolWeek week);
    Task<Result<(SchoolWeek Week, SchoolTerm Term)>> GetWeekForDate(DateOnly date);
    Task<(Stream BasicFile, Stream DetailFile)> GetNAwardReport(CancellationToken cancellationToken = default);
    Task<IndigenousStatus> GetStudentIndigenousStatus(string sentralStudentId);

    Task<Result<DateTime>> IssueAward(List<string> studentSentralIds, IssueAwardType awardType);
    Task<Result<List<DateOnly>>> GetEnrolledDatesForStudent(string sentralId, string year, DateOnly startDate, DateOnly endDate);
    Task<Result<List<SentralPeriodAbsenceDto>>> GetAbsenceDataAsync(string sentralStudentId, string year, CancellationToken cancellationToken = default);
    Task<Dictionary<StudentReferenceNumber, List<SentralPeriodAbsenceDto>>> GetAttendanceModuleAbsenceDataForSchool(CancellationToken cancellationToken = default);

    // API methods
    Task<ICollection<FamilyDetailsDto>> GetFamilyDetailsReportFromApi(ILogger logger, CancellationToken cancellationToken = default);
    Task<FamilyDetailsDto> GetParentContactEntryFromApi(string sentralStudentId, CancellationToken cancellationToken = default);
    Task<byte[]> GetSentralStudentPhotoFromApi(string sentralStudentId, CancellationToken cancellationToken = default);
}
