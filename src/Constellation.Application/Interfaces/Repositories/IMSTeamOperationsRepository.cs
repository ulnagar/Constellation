using Constellation.Application.DTOs;
using Constellation.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Constellation.Application.Interfaces.Repositories
{
    public interface IMSTeamOperationsRepository
    {
        MSTeamOperation WithDetails(int id);
        MSTeamOperation WithFilter(Expression<Func<MSTeamOperation, bool>> predicate);
        ICollection<MSTeamOperation> All();
        ICollection<MSTeamOperation> AllWithFilter(Expression<Func<MSTeamOperation, bool>> predicate);
        MSTeamOperationsList ToProcess();
        MSTeamOperationsList OverdueToProcess();
        MSTeamOperationsList Recent();
        Task<MSTeamOperation> ForMarkingCompleteOrCancelled(int id);
    }
}