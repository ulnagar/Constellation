namespace Constellation.Application.Interfaces.Repositories;

using Constellation.Core.Models.Operations;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

public interface ICanvasOperationsRepository
{
    Task<CanvasOperation> WithDetails(int id, CancellationToken cancellationToken = default);
    Task<CanvasOperation> WithFilter(Expression<Func<CanvasOperation, bool>> predicate, CancellationToken cancellationToken = default);
    Task<List<CanvasOperation>> All(CancellationToken cancellationToken = default);
    Task<List<CanvasOperation>> AllWithFilter(Expression<Func<CanvasOperation, bool>> predicate, CancellationToken cancellationToken = default);
    Task<List<CanvasOperation>> AllToProcess(CancellationToken cancellationToken = default);

    void Insert(CanvasOperation operation);
}
