namespace Constellation.Application.Interfaces.Services;

using Attendance.GetAttendanceDataFromSentral;
using Constellation.Application.Absences.GetAbsencesWithFilterForReport;
using Constellation.Application.Awards.ExportAwardNominations;
using Constellation.Application.Contacts.GetContactList;
using Constellation.Application.DTOs;
using Constellation.Application.DTOs.CSV;
using Constellation.Application.ExternalDataConsistency;
using Constellation.Application.GroupTutorials.GenerateTutorialAttendanceReport;
using Constellation.Application.MandatoryTraining.Models;
using Constellation.Application.Rollover.ImportStudents;
using Constellation.Core.Models.MandatoryTraining;
using Constellation.Infrastructure.Jobs;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

public interface IExcelService
{
    Task<MemoryStream> CreatePTOFile(ICollection<InterviewExportDto> exportLines);
    Task<MemoryStream> CreateAbsencesReportFile(List<FilteredAbsenceResponse> exportAbsences, CancellationToken cancellationToken = default);
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
    Task<List<StudentImportRecord>> ConvertStudentImportFile(MemoryStream importFile, CancellationToken cancellationToken = default);

    Task<List<StudentAttendanceData>> ReadSystemAttendanceData(List<StudentAttendanceData> data, SystemAttendanceData systemData, CancellationToken cancellationToken = default);
}