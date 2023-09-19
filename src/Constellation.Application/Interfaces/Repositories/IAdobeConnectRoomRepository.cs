using Constellation.Core.Models;
namespace Constellation.Application.Interfaces.Repositories;

using Constellation.Core.Models.Offerings.Identifiers;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public interface IAdobeConnectRoomRepository
{
    Task<List<AdobeConnectRoom>> GetAll(CancellationToken cancellationToken = default);
    Task<List<AdobeConnectRoom>> GetByOfferingId(OfferingId offeringId, CancellationToken cancellationToken = default);
    Task<AdobeConnectRoom> GetById(string Id, CancellationToken cancellationToken = default);
    AdobeConnectRoom WithDetails(string id);
    Task<AdobeConnectRoom> GetForExistCheck(string id);
    ICollection<AdobeConnectRoom> AllActive();

}