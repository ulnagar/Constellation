namespace Constellation.Application.Abstractions.Messaging;

using Constellation.Core.Shared;
using MediatR;

public interface IQuery<TResponse> : IRequest<Result<TResponse>>
{
}
