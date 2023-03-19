using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions;
using Constellation.Core.Errors;
using Constellation.Core.Shared;
using System.Threading;
using System.Threading.Tasks;

namespace Constellation.Application.ClassCovers.CancelCover;

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
            return Result.Failure(DomainErrors.ClassCovers.Cover.NotFound(request.CoverId.Value));
        }

        cover.Delete();

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
