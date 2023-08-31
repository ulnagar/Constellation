using Constellation.Core.Models;
using Constellation.Core.Models.Offerings;
using Constellation.Core.Models.Offerings.Identifiers;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Constellation.Application.Interfaces.Repositories
{
    public interface IAdobeConnectRoomRepository
    {
        Task<List<AdobeConnectRoom>> GetByOfferingId(OfferingId offeringId, CancellationToken cancellationToken = default);
        Task<AdobeConnectRoom> GetById(string Id, CancellationToken cancellationToken = default);
        AdobeConnectRoom WithDetails(string id);
        AdobeConnectRoom WithFilter(Expression<Func<AdobeConnectRoom, bool>> predicate);
        Task<AdobeConnectRoom> GetForExistCheck(string id);
        ICollection<AdobeConnectRoom> All();
        ICollection<AdobeConnectRoom> AllWithFilter(Expression<Func<AdobeConnectRoom, bool>> predicate);
        ICollection<AdobeConnectRoom> AllFromSession(Session session);
        ICollection<AdobeConnectRoom> AllActive();
        ICollection<AdobeConnectRoom> AllWithSession();
        ICollection<AdobeConnectRoom> AllWithActiveSession();
        ICollection<AdobeConnectRoom> AllForOffering(OfferingId offeringId);
        Task<ICollection<AdobeConnectRoom>> ForSelectionAsync();
    }
}