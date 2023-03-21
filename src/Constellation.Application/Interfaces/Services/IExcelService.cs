using Constellation.Application.DTOs;
using Constellation.Application.DTOs.CSV;
using Constellation.Application.ExternalDataConsistency;
using Constellation.Application.GroupTutorials.GenerateTutorialAttendanceReport;
using Constellation.Application.MandatoryTraining.Models;
using Constellation.Core.Models.MandatoryTraining;
using Constellation.Infrastructure.Jobs;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Constellation.Application.Interfaces.Services
{
    public interface IExcelService
    {
        Task<MemoryStream> CreatePTOFile(ICollection<InterviewExportDto> exportLines);
        Task<MemoryStream> CreateAbsencesFile(ICollection<AbsenceExportDto> exportAbsences, string title);
        Task<MemoryStream> CreateAwardsCalculationFile(MemoryStream stream);
        Task<MemoryStream> CreateTrainingModuleReportFile(ModuleDetailsDto data);
        Task<MemoryStream> CreateTrainingModuleStaffReportFile(StaffCompletionListDto data);
        List<TrainingModule> ImportMandatoryTrainingDataFromFile(MemoryStream excelFile);
        Task<MemoryStream> CreateGroupTutorialAttendanceFile(TutorialDetailsDto data);
        Task<MemoryStream> CreateFamilyContactDetailsChangeReport(List<ParentContactChangeDto> changes, CancellationToken cancellationToken = default);
        Task<List<MasterFileSchool>> GetSchoolsFromMasterFile(MemoryStream stream);
        Task<List<MasterFileStudent>> GetStudentsFromMasterFile(MemoryStream stream);
        Task<MemoryStream> CreateMasterFileConsistencyReport(List<UpdateItem> updateItems, CancellationToken cancellationToken = default);
    }
}