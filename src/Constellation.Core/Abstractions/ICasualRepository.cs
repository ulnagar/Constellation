namespace Constellation.Core.Abstractions;

using Constellation.Core.Models.Casuals;
using Constellation.Core.Models.Identifiers;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public interface ICasualRepository
{
    Task<Casual?> GetById(CasualId id, CancellationToken cancellationToken = default);
    Task<List<Casual>> GetAllActive(CancellationToken cancellationToken = default);
    Task<List<Casual>> GetAll(CancellationToken cancellationToken = default);
    Task<List<Casual>> GetAllInactive(CancellationToken cancellationToken = default);
    Task<List<Casual>> GetWithoutAdobeConnectId(CancellationToken cancellationToken = default);
    void Insert(Casual casual);
}