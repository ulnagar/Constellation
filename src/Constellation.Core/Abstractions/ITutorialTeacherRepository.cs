#nullable enable
namespace Constellation.Core.Abstractions;

using Constellation.Core.Models.GroupTutorials;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public interface ITutorialTeacherRepository
{
    Task<TutorialTeacher?> GetById(Guid Id, CancellationToken cancellationToken = default);
    Task<List<TutorialTeacher>> GetActiveForTutorial(Guid tutorialId, CancellationToken cancellationToken = default);
    Task<int?> GetCountForTutorial(Guid tutorialId, CancellationToken cancellationToken = default);
    void Insert(TutorialTeacher teacher);
}
