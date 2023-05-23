namespace Constellation.Core.Abstractions;

using Constellation.Core.Models.Absences;
using Constellation.Core.Models.Identifiers;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public interface IAbsenceResponseRepository
{
    Task<Response> GetById(AbsenceResponseId responseId, CancellationToken cancellationToken = default);
    Task<List<Response>> GetAllForAbsence(AbsenceId absenceId, CancellationToken cancellationToken = default);
    Task<int> GetCountForAbsence(AbsenceId absenceId, CancellationToken cancellationToken = default);
}