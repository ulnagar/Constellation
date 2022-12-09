namespace Constellation.Application.Abstractions.Messaging;

using Constellation.Core.Shared;
using MediatR;

public interface ICommand : IRequest<Result>
{
}

public interface ICommand<TResponse> : IRequest<Result<TResponse>>
{

}
