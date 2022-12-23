using Constellation.Application.DTOs;
using Constellation.Application.Features.MandatoryTraining.Models;
using Constellation.Application.GroupTutorials.GenerateTutorialAttendanceReport;
using Constellation.Core.Models.MandatoryTraining;
using System.Collections.Generic;
using System.IO;
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
    }
}