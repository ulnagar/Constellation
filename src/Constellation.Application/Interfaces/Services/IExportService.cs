using Constellation.Application.DTOs;
using Constellation.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Constellation.Application.Interfaces.Services
{
    public interface IExportService
    {
        List<InterviewExportDto> CreatePTOExport(ICollection<Student> students, bool perFamily);
        Task<List<AbsenceExportDto>> CreateAbsenceExport(AbsenceFilterDto filter);
    }
}