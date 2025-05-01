namespace Constellation.Application.Domains.Casuals.Commands.RestoreCasual;

using Abstractions.Messaging;
using Core.Abstractions.Repositories;
using Core.Errors;
using Core.Shared;
using Interfaces.Repositories;
using System.Threading;
using System.Threading.Tasks;

internal sealed class RestoreCasualCommandHandler
    : ICommandHandler<RestoreCasualCommand>
{
    private readonly ICasualRepository _casualRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RestoreCasualCommandHandler(
        ICasualRepository casualRepository,
        IUnitOfWork unitOfWork)
    {
        _casualRepository = casualRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(RestoreCasualCommand request, CancellationToken cancellationToken)
    {
        var casual = await _casualRepository.GetById(request.CasualId, cancellationToken);

        if (casual is null)
        {
            return Result.Failure(DomainErrors.Casuals.Casual.NotFound(request.CasualId));
        }

        if (casual.IsDeleted)
        {
            casual.Restore();
            await _unitOfWork.CompleteAsync(cancellationToken);
        }

        return Result.Success();
    }
}
