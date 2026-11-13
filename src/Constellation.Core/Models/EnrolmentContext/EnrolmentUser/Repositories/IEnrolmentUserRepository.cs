namespace Constellation.Core.Models.EnrolmentContext.EnrolmentUser.Repositories;

using Constellation.Core.ValueObjects;
using System;
using System.Collections.Generic;

public interface IEnrolmentUserRepository
{
    Task<EnrolmentUser?> GetById(Guid userId, CancellationToken cancellationToken = default);
    Task<EnrolmentUser?> GetByEmail(EmailAddress email, CancellationToken cancellationToken = default);
    Task<List<EnrolmentUser>> GetListFromIds(List<Guid> userIds, CancellationToken cancellationToken = default);
}
