using Constellation.Application.Absences.GetAbsencesForExport;
using Constellation.Application.DTOs;
using Constellation.Core.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Constellation.Application.Interfaces.Services
{
    public interface IExportService
    {
        Task<List<InterviewExportDto>> CreatePTOExport(List<Student> students, bool perFamily, bool residentialFamilyOnly, CancellationToken cancellationToken = default);
    }
}