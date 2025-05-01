namespace Constellation.Application.Domains.Casuals.Commands.DeleteCasual;

using Abstractions.Messaging;
using Core.Abstractions.Repositories;
using Core.Errors;
using Core.Shared;
using Interfaces.Repositories;
using System.Threading;
using System.Threading.Tasks;

internal sealed class DeleteCasualCommandHandler
    : ICommandHandler<DeleteCasualCommand>
{
    private readonly ICasualRepository _casualRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteCasualCommandHandler(
        ICasualRepository casualRepository,
        IUnitOfWork unitOfWork)
    {
        _casualRepository = casualRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(DeleteCasualCommand request, CancellationToken cancellationToken)
    {
        var casual = await _casualRepository.GetById(request.CasualId, cancellationToken);

        if (casual is null)
        {
            return Result.Failure(DomainErrors.Casuals.Casual.NotFound(request.CasualId));
        }

        casual.Delete();

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
