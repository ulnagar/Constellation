namespace Constellation.Application.Interfaces.Gateways;

using Constellation.Application.Attendance.GetAttendanceDataFromSentral;
using Constellation.Application.Attendance.GetValidAttendanceReportDates;
using Constellation.Application.DTOs;
using Constellation.Application.DTOs.Awards;
using Serilog;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public interface ISentralGateway
{
    Task<string> GetSentralStudentIdAsync(string studentName);
    Task<List<SentralPeriodAbsenceDto>> GetAbsenceDataAsync(string sentralStudentId);
    Task<List<SentralPeriodAbsenceDto>> GetPartialAbsenceDataAsync(string sentralStudentId);
    Task<string> GetSentralStudentIdFromSRN(string srn, string grade);
    Task<IDictionary<string, IDictionary<string, string>>> GetParentContactEntry(string sentralStudentId);
    Task<List<DateOnly>> GetExcludedDatesFromCalendar(string year);
    Task<List<ValidAttendenceReportDate>> GetValidAttendanceReportDatesFromCalendar(string year);
    Task<ICollection<RollMarkReportDto>> GetRollMarkingReportAsync(DateOnly date);
    Task<ICollection<FamilyDetailsDto>> GetFamilyDetailsReport(ILogger logger);
    Task<byte[]> GetSentralStudentPhoto(string studentId);
    Task<ICollection<SentralReportDto>> GetStudentReportList(string sentralStudentId);
    Task<byte[]> GetStudentReport(string sentralStudentId, string reportId);
    Task<ICollection<AwardDetailDto>> GetAwardsReport();
    Task<List<AwardIncidentDto>> GetAwardsListing(string sentralStudentId, string calYear);
    Task<byte[]> GetAwardDocument(string sentralStudentId, string incidentId);
    Task<SystemAttendanceData> GetAttendancePercentages(string term, string week, string year);
}
