namespace Constellation.Application.Interfaces.Services;

using Absences.ExportUnexplainedPartialAbsencesReport;
using Attendance.GetAttendanceDataFromSentral;
using Constellation.Application.Absences.GetAbsencesWithFilterForReport;
using Constellation.Application.Attendance.GenerateAttendanceReportForPeriod;
using Constellation.Application.Awards.ExportAwardNominations;
using Constellation.Application.Compliance.GetWellbeingReportFromSentral;
using Constellation.Application.Contacts.GetContactList;
using Constellation.Application.DTOs;
using Constellation.Application.DTOs.CSV;
using Constellation.Application.ExternalDataConsistency;
using Constellation.Application.GroupTutorials.GenerateTutorialAttendanceReport;
using Constellation.Application.Rollover.ImportStudents;
using Constellation.Application.Training.Modules.GenerateOverallReport;
using Constellation.Core.Models.Training.Contexts.Modules;
using Constellation.Infrastructure.Jobs;
using SciencePracs.GenerateOverdueReport;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Training.Models;

public interface IExcelService
{
    Task<MemoryStream> CreatePTOFile(ICollection<InterviewExportDto> exportLines);
    Task<MemoryStream> CreateAbsencesReportFile(List<FilteredAbsenceResponse> exportAbsences, CancellationToken cancellationToken = default);
    Task<MemoryStream> CreateUnexplainedPartialAbsencesReportFile(List<UnexplainedPartialAbsenceResponse> absences, CancellationToken cancellationToken = default);
    Task<MemoryStream> CreateAwardsCalculationFile(MemoryStream stream);
    Task<MemoryStream> CreateTrainingModuleReportFile(ModuleDetailsDto data);
    Task<MemoryStream> CreateTrainingModuleStaffReportFile(StaffCompletionListDto data);
    List<TrainingModule> ImportMandatoryTrainingDataFromFile(MemoryStream excelFile);
    Task<MemoryStream> CreateGroupTutorialAttendanceFile(TutorialDetailsDto data);
    Task<MemoryStream> CreateFamilyContactDetailsChangeReport(List<ParentContactChangeDto> changes, CancellationToken cancellationToken = default);
    Task<List<MasterFileSchool>> GetSchoolsFromMasterFile(MemoryStream stream);
    Task<List<MasterFileStudent>> GetStudentsFromMasterFile(MemoryStream stream);
    Task<MemoryStream> CreateMasterFileConsistencyReport(List<UpdateItem> updateItems, CancellationToken cancellationToken = default);
    Task<MemoryStream> CreateContactExportFile(List<ContactResponse> contacts, CancellationToken cancellationToken = default);
    Task<MemoryStream> CreateAwardNominationsExportFile(List<AwardNominationExportDto> nominations, CancellationToken cancellationToken = default);
    List<StudentAttendanceData> ExtractPerDayYearToDateAttendanceData(SystemAttendanceData systemData, List<StudentAttendanceData> data);
    List<StudentAttendanceData> ExtractPerMinuteYearToDateAttendanceData(SystemAttendanceData systemData, List<StudentAttendanceData> data);
    List<StudentAttendanceData> ExtractPerDayWeekAttendanceData(SystemAttendanceData systemData, List<StudentAttendanceData> data);
    List<StudentAttendanceData> ExtractPerMinuteWeekAttendanceData(SystemAttendanceData systemData, List<StudentAttendanceData> data);
    Task<List<StudentImportRecord>> ConvertStudentImportFile(MemoryStream importFile, CancellationToken cancellationToken = default);
    Task<List<SentralIncidentDetails>> ConvertSentralIncidentReport(Stream reportFile, CancellationToken cancellationToken = default);
    Task<MemoryStream> CreateWellbeingExportFile(List<SentralIncidentDetails> records, CancellationToken cancellationToken = default);
    Task<MemoryStream> CreateStudentAttendanceReport(string periodLabel, List<AttendanceRecord> records, List<AbsenceRecord> absenceRecords, CancellationToken cancellationToken = default);
    Task<MemoryStream> CreateSciencePracOverdueReport(List<OverdueRollResponse> records, CancellationToken cancellationToken = default);
    Task<MemoryStream> CreateTrainingModuleOverallReportFile(List<ModuleDetails> moduleDetails, List<StaffStatus> staffStatuses, CancellationToken cancellationToken = default);
}