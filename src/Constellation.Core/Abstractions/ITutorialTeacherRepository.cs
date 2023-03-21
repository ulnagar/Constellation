#nullable enable
namespace Constellation.Core.Abstractions;

using Constellation.Core.Models.GroupTutorials;
using Constellation.Core.Models.Identifiers;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public interface ITutorialTeacherRepository
{
    Task<TutorialTeacher?> GetById(TutorialTeacherId Id, CancellationToken cancellationToken = default);
    Task<List<TutorialTeacher>> GetActiveForTutorial(GroupTutorialId tutorialId, CancellationToken cancellationToken = default);
    Task<int?> GetCountForTutorial(GroupTutorialId tutorialId, CancellationToken cancellationToken = default);
    void Insert(TutorialTeacher teacher);
}
