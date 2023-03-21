#nullable enable
namespace Constellation.Core.Abstractions;

using Constellation.Core.Models.GroupTutorials;
using Constellation.Core.Models.Identifiers;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public interface ITutorialEnrolmentRepository
{
    Task<TutorialEnrolment?> GetById(TutorialEnrolmentId enrolmentId, CancellationToken cancellationToken = default);
    Task<List<TutorialEnrolment>> GetActiveForTutorial(GroupTutorialId tutorialId, CancellationToken cancellationToken = default);
    Task<int?> GetCountForTutorial(GroupTutorialId tutorialId, CancellationToken cancellationToken = default);
    void Insert(TutorialEnrolment enrolment);
}
