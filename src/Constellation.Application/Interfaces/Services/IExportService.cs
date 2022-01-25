using Constellation.Application.DTOs;
using Constellation.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Constellation.Application.Interfaces.Services
{
    public interface IExportService
    {
        Task<List<InterviewExportDto>> CreatePTOExport(ICollection<Student> students);
        Task<List<AbsenceExportDto>> CreateAbsenceExport(AbsenceFilterDto filter);
    }
}