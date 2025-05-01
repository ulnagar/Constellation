namespace Constellation.Application.Domains.ClassCovers.Commands.CancelCover;

using Abstractions.Messaging;
using Core.Abstractions.Repositories;
using Core.Errors;
using Core.Shared;
using Interfaces.Repositories;
using System.Threading;
using System.Threading.Tasks;

public class CancelCoverCommandHandler : ICommandHandler<CancelCoverCommand>
{
    private readonly IClassCoverRepository _classCoverRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CancelCoverCommandHandler(IUnitOfWork unitOfWork, IClassCoverRepository classCoverRepository)
    {
        _unitOfWork = unitOfWork;
        _classCoverRepository = classCoverRepository;
    }

    public async Task<Result> Handle(CancelCoverCommand request, CancellationToken cancellationToken)
    {
        var cover = await _classCoverRepository
            .GetById(request.CoverId, cancellationToken);

        if (cover is null)
        {
            return Result.Failure(DomainErrors.ClassCovers.Cover.NotFound(request.CoverId));
        }

        cover.Delete();

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
