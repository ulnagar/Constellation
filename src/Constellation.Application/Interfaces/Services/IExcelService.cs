﻿namespace Constellation.Application.Interfaces.Services;

using Absences.ExportUnexplainedPartialAbsencesReport;
using Absences.GetAbsencesWithFilterForReport;
using Assets.ImportAssetsFromFile;
using Attendance.GenerateAttendanceReportForPeriod;
using Attendance.GetAttendanceDataFromSentral;
using Awards.ExportAwardNominations;
using Compliance.GetWellbeingReportFromSentral;
using Constellation.Application.Contacts.Models;
using Constellation.Application.Training.GenerateOverallReport;
using Constellation.Infrastructure.Jobs;
using Core.Models.Assets;
using Core.Models.Training;
using DTOs;
using DTOs.CSV;
using ExternalDataConsistency;
using GroupTutorials.GenerateTutorialAttendanceReport;
using Rollover.ImportStudents;
using SchoolContacts.GetContactsBySchool;
using SciencePracs.GenerateOverdueReport;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Training.Models;
using WorkFlows.ExportOpenCaseReport;

public interface IExcelService
{
    Task<MemoryStream> CreatePTOFile(List<InterviewExportDto> exportLines, CancellationToken cancellationToken = default);
    Task<MemoryStream> CreateAbsencesReportFile(List<FilteredAbsenceResponse> exportAbsences, CancellationToken cancellationToken = default);
    Task<MemoryStream> CreateUnexplainedPartialAbsencesReportFile(List<UnexplainedPartialAbsenceResponse> absences, CancellationToken cancellationToken = default);
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
    Task<List<SentralIncidentDetails>> ConvertSentralIncidentReport(Stream reportFile, Stream detailFile, List<DateOnly> excludedDates, CancellationToken cancellationToken = default);
    Task<MemoryStream> CreateWellbeingExportFile(List<SentralIncidentDetails> records, CancellationToken cancellationToken = default);
    Task<MemoryStream> CreateStudentAttendanceReport(string periodLabel, List<AttendanceRecord> records, List<AbsenceRecord> absenceRecords, CancellationToken cancellationToken = default);
    Task<MemoryStream> CreateSciencePracOverdueReport(List<OverdueRollResponse> records, CancellationToken cancellationToken = default);
    Task<MemoryStream> CreateTrainingModuleOverallReportFile(List<ModuleDetails> moduleDetails, List<StaffStatus> staffStatuses, CancellationToken cancellationToken = default);
    Task<MemoryStream> CreateWorkFlowReport(List<CaseReportItem> records, CancellationToken cancellationToken = default);
    Task<MemoryStream> CreateSchoolContactExport(List<SchoolWithContactsResponse> records, CancellationToken cancellationToken = default);
    Task<List<ImportAssetDto>> ImportAssetsFromFile(MemoryStream stream, CancellationToken cancellationToken = default);
    Task<MemoryStream> CreateAssetExportFile(List<Asset> assets, CancellationToken cancellationToken = default);
}