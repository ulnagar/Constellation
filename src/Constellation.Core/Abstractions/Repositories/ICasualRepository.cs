namespace Constellation.Core.Abstractions.Repositories;

using Constellation.Core.Models.Casuals;
using Constellation.Core.Models.Identifiers;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ValueObjects;

public interface ICasualRepository
{
    Task<Casual> GetById(CasualId id, CancellationToken cancellationToken = default);
    Task<List<Casual>> GetAllActive(CancellationToken cancellationToken = default);
    Task<List<Casual>> GetAll(CancellationToken cancellationToken = default);
    Task<List<Casual>> GetAllInactive(CancellationToken cancellationToken = default);
    Task<Casual> GetByEmailAddress(EmailAddress email, CancellationToken cancellationToken = default);
    Task<Casual> GetByEdvalCode(string edvalCode, CancellationToken cancellationToken = default);
    void Insert(Casual casual);
}