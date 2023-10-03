namespace Constellation.Application.Interfaces.Repositories;

using Constellation.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

public interface ICanvasOperationsRepository
{
    Task<CanvasOperation> WithDetails(int id);
    Task<CanvasOperation> WithFilter(Expression<Func<CanvasOperation, bool>> predicate);
    Task<ICollection<CanvasOperation>> All();
    Task<ICollection<CanvasOperation>> AllWithFilter(Expression<Func<CanvasOperation, bool>> predicate);
    Task<ICollection<CanvasOperation>> AllToProcess();

    void Insert(CanvasOperation operation);
}
