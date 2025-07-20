namespace Constellation.Core.Models.Tutorials.Repositories;

using Identifiers;
using StaffMembers.Identifiers;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public interface ITutorialRepository
{
    Task<Tutorial> GetById(TutorialId tutorialId, CancellationToken cancellationToken = default);
    Task<List<Tutorial>> GetAll(CancellationToken cancellationToken = default);
    Task<List<Tutorial>> GetAllActive(CancellationToken cancellationToken = default);
    Task<List<Tutorial>> GetInactive(CancellationToken cancellationToken = default);
    Task<List<Tutorial>> GetActiveForTeacher(StaffId staffId, CancellationToken cancellationToken = default);

    void Insert(Tutorial tutorial);
}
