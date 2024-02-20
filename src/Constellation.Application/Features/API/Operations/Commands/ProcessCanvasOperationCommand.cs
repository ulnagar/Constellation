namespace Constellation.Application.Features.API.Operations.Commands;

using Constellation.Application.DTOs;
using Constellation.Core.Models.Operations;
using MediatR;

public sealed class ProcessCanvasOperationCommand : IRequest<ServiceOperationResult<CanvasOperation>>
{
    public CanvasOperation Operation { get; set; }
}