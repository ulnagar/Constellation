namespace Constellation.Application.Domains.Covers.Commands.CancelCover;

using Abstractions.Messaging;
using Constellation.Core.Models.Covers.Repositories;
using Core.Models.Covers;
using Core.Models.Covers.Errors;
using Core.Shared;
using Interfaces.Repositories;
using System.Threading;
using System.Threading.Tasks;

public class CancelCoverCommandHandler : ICommandHandler<CancelCoverCommand>
{
    private readonly ICoverRepository _coverRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CancelCoverCommandHandler(IUnitOfWork unitOfWork, ICoverRepository coverRepository)
    {
        _unitOfWork = unitOfWork;
        _coverRepository = coverRepository;
    }

    public async Task<Result> Handle(CancelCoverCommand request, CancellationToken cancellationToken)
    {
        Cover cover = await _coverRepository
            .GetById(request.CoverId, cancellationToken);

        if (cover is null)
        {
            return Result.Failure(CoverErrors.NotFound(request.CoverId));
        }

        cover.Delete();

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
