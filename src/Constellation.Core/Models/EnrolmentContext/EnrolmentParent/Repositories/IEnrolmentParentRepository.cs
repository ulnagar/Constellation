namespace Constellation.Core.Models.EnrolmentContext.EnrolmentParent.Repositories;

using Identifiers;
using System.Collections.Generic;
using ValueObjects;

public interface IEnrolmentParentRepository
{
    Task<EnrolmentParent?> GetById(EnrolmentParentId parentId, CancellationToken cancellationToken = default);
    Task<List<EnrolmentParent>> GetByUserId(Guid userId, CancellationToken cancellationToken = default);
    Task<List<EnrolmentParent>> GetByEmail(EmailAddress email, CancellationToken cancellationToken = default);

    Task<List<EnrolmentParent>> GetListFromIds(List<EnrolmentParentId> parentIds, CancellationToken cancellationToken = default);

    void Insert(EnrolmentParent parent);
    void Remove(EnrolmentParent parent);
}
