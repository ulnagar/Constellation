namespace Constellation.Core.Abstractions;

using Constellation.Core.Models.Casuals;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System;

public interface ICasualRepository
{
    Task<Casual?> GetById(Guid id, CancellationToken cancellationToken = default);
    Task<List<Casual>> GetAllActive(CancellationToken cancellationToken = default);
    Task<List<Casual>> GetAll(CancellationToken cancellationToken = default);
    Task<List<Casual>> GetAllInactive(CancellationToken cancellationToken = default);
    Task<List<Casual>> GetWithoutAdobeConnectId(CancellationToken cancellationToken = default);
    void Insert(Casual casual);
}