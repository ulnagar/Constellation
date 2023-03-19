using Constellation.Application.DTOs;
using Constellation.Core.Models;
using Constellation.Core.Models.Identifiers;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Constellation.Application.Interfaces.Repositories
{
    public interface IAdobeConnectOperationsRepository
    {
        AdobeConnectOperation WithDetails(int id);
        AdobeConnectOperation WithFilter(Expression<Func<AdobeConnectOperation, bool>> predicate);
        ICollection<AdobeConnectOperation> All();
        ICollection<AdobeConnectOperation> AllWithFilter(Expression<Func<AdobeConnectOperation, bool>> predicate);
        Task<ICollection<AdobeConnectOperation>> AllToProcess();
        Task<ICollection<AdobeConnectOperation>> AllOverdue();
        AdobeConnectOperationsList AllRecent();
        Task<ICollection<AdobeConnectOperation>> AllRecentAsync();
        Task<AdobeConnectOperation> ForProcessingAsync(int id);

        Task<List<AdobeConnectOperation>> GetByCoverId(ClassCoverId coverId, CancellationToken cancellationToken = default);
        void Insert(AdobeConnectOperation adobeConnectOperation);
    }
}