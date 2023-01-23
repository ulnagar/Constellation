#nullable enable
namespace Constellation.Core.Abstractions;

using Constellation.Core.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public interface ITeamRepository
{
    Task<List<Team>> GetAll(CancellationToken cancellationToken = default);
    Task<Team?> GetById(Guid teamId, CancellationToken cancellationToken = default);
    Task<Guid?> GetIdByOffering(string offeringName, string offeringYear, CancellationToken cancellationToken = default);
    Task<string?> GetLinkById(Guid teamId, CancellationToken cancellationToken = default);
    Task<string?> GetLinkByOffering(string offeringName, string offeringYear, CancellationToken cancellationToken = default);
    void Insert(Team team);
    void Remove(Team team);
}
