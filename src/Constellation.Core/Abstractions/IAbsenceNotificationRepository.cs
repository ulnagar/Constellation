namespace Constellation.Core.Abstractions;

using Constellation.Core.Models.Identifiers;
using System.Threading;
using System.Threading.Tasks;

public interface IAbsenceNotificationRepository
{
    Task<int> GetCountForAbsence(AbsenceId absenceId, CancellationToken cancellationToken = default);
}
