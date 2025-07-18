﻿namespace Constellation.Application.Interfaces.Services;

using Constellation.Application.Domains.AssetManagement.Stocktake.Queries.ExportStocktakeSightingsAndDifferences;
using Constellation.Application.Domains.Attendance.Absences.Queries.ExportUnexplainedPartialAbsencesReport;
using Constellation.Application.Domains.Attendance.Absences.Queries.GetAbsencesWithFilterForReport;
using Constellation.Application.Domains.Attendance.Reports.Queries.GenerateAttendanceReportForPeriod;
using Constellation.Application.Domains.Attendance.Reports.Queries.GenerateCustomReportForPeriod;
using Constellation.Application.Domains.GroupTutorials.Queries.GenerateTutorialAttendanceReport;
using Constellation.Application.Domains.MeritAwards.Nominations.Queries.ExportAwardNominations;
using Constellation.Application.Domains.Students.Commands.ImportStudentsFromFile;
using Constellation.Core.Models.Students;
using Constellation.Infrastructure.Jobs;
using Core.Models.Assets;
using Core.Models.Training;
using Core.Shared;
using Domains.AssetManagement.Assets.Commands.ImportAssetsFromFile;
using Domains.Attendance.Reports.Commands.UpdateAttendanceDataForPeriodFromSentral;
using Domains.Compliance.Wellbeing.Queries.GetWellbeingReportFromSentral;
using Domains.Contacts.Models;
using Domains.ExternalSystems.Masterfile.Commands;
using Domains.ExternalSystems.Masterfile.Models;
using Domains.LinkedSystems.Canvas.Models;
using Domains.SchoolContacts.Queries.GetContactsBySchool;
using Domains.SciencePracs.Queries.GenerateOverdueReport;
using Domains.SciencePracs.Queries.GenerateYTDStatusReport;
using Domains.Training.Models;
using Domains.Training.Queries.GenerateOverallReport;
using Domains.WorkFlows.Queries.ExportOpenCaseReport;
using DTOs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

public interface IExcelService
{
    Task<MemoryStream> CreatePTOFile(List<InterviewExportDto> exportLines, CancellationToken cancellationToken = default);
    Task<MemoryStream> CreateAbsencesReportFile(List<FilteredAbsenceResponse> exportAbsences, CancellationToken cancellationToken = default);
    Task<MemoryStream> CreateUnexplainedPartialAbsencesReportFile(List<UnexplainedPartialAbsenceResponse> absences, CancellationToken cancellationToken = default);
    Task<MemoryStream> CreateTrainingModuleReportFile(ModuleDetailsDto data);
    Task<MemoryStream> CreateTrainingModuleStaffReportFile(StaffCompletionListDto data);
    List<TrainingModule> ImportMandatoryTrainingDataFromFile(MemoryStream excelFile);
    Task<Result<MemoryStream>> CreateGroupTutorialAttendanceFile(TutorialDetailsDto data);
    Task<MemoryStream> CreateFamilyContactDetailsChangeReport(List<ParentContactChangeDto> changes, CancellationToken cancellationToken = default);
    Task<List<MasterFileSchool>> GetSchoolsFromMasterFile(MemoryStream stream);
    Task<List<MasterFileStudent>> GetStudentsFromMasterFile(MemoryStream stream);
    Task<MemoryStream> CreateMasterFileConsistencyReport(List<UpdateItem> updateItems, CancellationToken cancellationToken = default);
    Task<MemoryStream> CreateContactExportFile(List<ContactResponse> contacts, CancellationToken cancellationToken = default);
    Task<MemoryStream> CreateAwardNominationsExportFileByStudent(List<AwardNominationExportByStudentDto> nominations, CancellationToken cancellationToken = default);
    Task<MemoryStream> CreateAwardNominationsExportFileBySchool(List<AwardNominationExportBySchoolDto> nominations, CancellationToken cancellationToken = default);
    Task<MemoryStream> CreateAwardNominationsExportFileBySubject(List<AwardNominationExportBySubjectDto> nominations, CancellationToken cancellationToken = default);
    Task<MemoryStream> CreateAwardNominationsExportFile(List<AwardNominationExportDto> nominations, CancellationToken cancellationToken = default);
    List<StudentAttendanceData> ExtractPerDayYearToDateAttendanceData(SystemAttendanceData systemData, List<StudentAttendanceData> data);
    List<StudentAttendanceData> ExtractPerMinuteYearToDateAttendanceData(SystemAttendanceData systemData, List<StudentAttendanceData> data);
    List<StudentAttendanceData> ExtractPerDayWeekAttendanceData(SystemAttendanceData systemData, List<StudentAttendanceData> data);
    List<StudentAttendanceData> ExtractPerMinuteWeekAttendanceData(SystemAttendanceData systemData, List<StudentAttendanceData> data);
    Task<List<SentralIncidentDetails>> ConvertSentralIncidentReport(Stream reportFile, Stream detailFile, List<DateOnly> excludedDates, CancellationToken cancellationToken = default);
    Task<MemoryStream> CreateWellbeingExportFile(List<SentralIncidentDetails> records, CancellationToken cancellationToken = default);
    Task<MemoryStream> CreateStudentAttendanceReport(string periodLabel, List<AttendanceRecord> records, List<AbsenceRecord> absenceRecords, CancellationToken cancellationToken = default);
    Task<MemoryStream> CreateSciencePracOverdueReport(List<OverdueRollResponse> records, CancellationToken cancellationToken = default);
    Task<MemoryStream> CreateSciencePracYTDReport(List<RollStatusResponse> records, CancellationToken cancellationToken = default);
    Task<MemoryStream> CreateTrainingModuleOverallReportFile(List<ModuleDetails> moduleDetails, List<StaffStatus> staffStatuses, CancellationToken cancellationToken = default);
    Task<MemoryStream> CreateWorkFlowReport(List<CaseReportItem> records, CancellationToken cancellationToken = default);
    Task<MemoryStream> CreateSchoolContactExport(List<SchoolWithContactsResponse> records, CancellationToken cancellationToken = default);
    Task<List<ImportAssetDto>> ImportAssetsFromFile(MemoryStream stream, CancellationToken cancellationToken = default);
    Task<List<ImportStudentDto>> ImportStudentsFromFile(MemoryStream stream, CancellationToken cancellationToken = default);
    Task<MemoryStream> CreateAssetExportFile(List<Asset> assets, CancellationToken cancellationToken = default);
    Task<MemoryStream> CreateCanvasRubricResultExport(RubricEntry rubric, List<CourseEnrolmentEntry> enrolments, List<AssignmentResultEntry> results, List<Student> students, CancellationToken cancellationToken = default);
    Task<MemoryStream> CreateCanvasAssignmentCommentExport(List<CourseEnrolmentEntry> enrolments, List<AssignmentResultEntry> results, Dictionary<string, List<Student>> students, CancellationToken cancellationToken = default);
    Task<MemoryStream> CreateCustomAttendanceReport(string periodLabel, List<ExportRecord> records, CancellationToken cancellationToken = default);
    Task<MemoryStream> CreateSefAttendanceDataExport(List<SefAttendanceData> attendanceData, CancellationToken cancellationToken = default);
    Task<MemoryStream> CreateStocktakeSightingsReport(List<StocktakeSightingWithDifferenceResponse> items, CancellationToken cancellationToken = default);
}