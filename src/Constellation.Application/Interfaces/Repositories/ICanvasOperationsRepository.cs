using Constellation.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Constellation.Application.Interfaces.Repositories
{
    public interface ICanvasOperationsRepository
    {
        Task<CanvasOperation> WithDetails(int id);
        Task<CanvasOperation> WithFilter(Expression<Func<CanvasOperation, bool>> predicate);
        Task<ICollection<CanvasOperation>> All();
        Task<ICollection<CanvasOperation>> AllWithFilter(Expression<Func<CanvasOperation, bool>> predicate);
        Task<ICollection<CanvasOperation>> AllToProcess();
    }
}
