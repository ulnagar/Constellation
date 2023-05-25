namespace Constellation.Core.Abstractions;

using Constellation.Core.Models.Absences;
using Constellation.Core.Models.Identifiers;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public interface IAbsenceRepository
{
    Task<Absence> GetById(AbsenceId absenceId, CancellationToken cancellationToken = default);
    Task<List<Absence>> GetForStudentFromCurrentYear(string StudentId, CancellationToken cancellationToken = default);
    Task<List<Absence>> GetAllFromCurrentYear(CancellationToken cancellationToken = default);
    void Insert(Absence absence);
}
