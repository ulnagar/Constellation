namespace Constellation.Core.Abstractions;

using Constellation.Core.Models.Absences;
using Constellation.Core.Models.Identifiers;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public interface IAbsenceNotificationRepository
{
    Task<int> GetCountForAbsence(AbsenceId absenceId, CancellationToken cancellationToken = default);
    Task<List<Notification>> GetAllForAbsence(AbsenceId absenceId, CancellationToken cancellationToken = default);
}
