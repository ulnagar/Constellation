#nullable enable
namespace Constellation.Core.Abstractions;

using Constellation.Core.Models.GroupTutorials;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public interface ITutorialEnrolmentRepository
{
    Task<TutorialEnrolment?> GetById(Guid enrolmentId, CancellationToken cancellationToken = default);
    Task<List<TutorialEnrolment>> GetActiveForTutorial(Guid tutorialId, CancellationToken cancellationToken = default);
    Task<int?> GetCountForTutorial(Guid tutorialId, CancellationToken cancellationToken = default);
    void Insert(TutorialEnrolment enrolment);
}
