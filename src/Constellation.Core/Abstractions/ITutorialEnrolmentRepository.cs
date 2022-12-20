#nullable enable
namespace Constellation.Core.Abstractions;

using Constellation.Core.Models.GroupTutorials;
using System;
using System.Threading;
using System.Threading.Tasks;

public interface ITutorialEnrolmentRepository
{
    Task<TutorialEnrolment?> GetById(Guid enrolmentId, CancellationToken cancellationToken = default);
    void Insert(TutorialEnrolment enrolment);
}
