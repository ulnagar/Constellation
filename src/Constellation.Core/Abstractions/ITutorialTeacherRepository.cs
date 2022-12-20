#nullable enable
namespace Constellation.Core.Abstractions;

using Constellation.Core.Models.GroupTutorials;
using System;
using System.Threading;
using System.Threading.Tasks;

public interface ITutorialTeacherRepository
{
    Task<TutorialTeacher?> GetById(Guid Id, CancellationToken cancellationToken = default);
    void Insert(TutorialTeacher teacher);
}
