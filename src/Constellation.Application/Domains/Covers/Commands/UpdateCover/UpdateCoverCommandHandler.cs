namespace Constellation.Application.Domains.Covers.Commands.UpdateCover;

using Abstractions.Messaging;
using Constellation.Core.Models.Covers.Repositories;
using Core.Models.Covers;
using Core.Models.Covers.Errors;
using Core.Shared;
using Interfaces.Repositories;
using System.Threading;
using System.Threading.Tasks;

internal sealed class UpdateCoverCommandHandler
    : ICommandHandler<UpdateCoverCommand, Cover>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICoverRepository _coverRepository;

    public UpdateCoverCommandHandler(
        IUnitOfWork unitOfWork,
        ICoverRepository coverRepository)
    {
        _unitOfWork = unitOfWork;
        _coverRepository = coverRepository;
    }

    public async Task<Result<Cover>> Handle(UpdateCoverCommand request, CancellationToken cancellationToken)
    {
        Cover cover = await _coverRepository.GetById(request.CoverId, cancellationToken);

        if (cover is null)
        {
            return Result.Failure<Cover>(CoverErrors.NotFound(request.CoverId));
        }

        cover.EditDates(request.StartDate, request.EndDate);

        await _unitOfWork.CompleteAsync(cancellationToken);

        return cover;
    }
}
