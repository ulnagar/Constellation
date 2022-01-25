using Constellation.Application.DTOs;
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
    }
}