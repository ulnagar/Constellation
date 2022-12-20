namespace Constellation.Application.Abstractions.Messaging;

using Constellation.Core.Shared;
using MediatR;

public interface IQueryHandler<TQuery, TResponse>
    : IRequestHandler<TQuery, Result<TResponse>>
    where TQuery : IQuery<TResponse>
{
}
