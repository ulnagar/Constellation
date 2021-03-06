using Constellation.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Constellation.Application.Interfaces.Repositories
{
    public interface IAdobeConnectRoomRepository
    {
        AdobeConnectRoom WithDetails(string id);
        AdobeConnectRoom WithFilter(Expression<Func<AdobeConnectRoom, bool>> predicate);
        Task<AdobeConnectRoom> GetForExistCheck(string id);
        ICollection<AdobeConnectRoom> All();
        ICollection<AdobeConnectRoom> AllWithFilter(Expression<Func<AdobeConnectRoom, bool>> predicate);
        ICollection<AdobeConnectRoom> AllFromSession(OfferingSession session);
        ICollection<AdobeConnectRoom> AllActive();
        ICollection<AdobeConnectRoom> AllWithSession();
        ICollection<AdobeConnectRoom> AllWithActiveSession();
        ICollection<AdobeConnectRoom> AllForOffering(int offeringId);
        Task<ICollection<AdobeConnectRoom>> ForSelectionAsync();
    }
}