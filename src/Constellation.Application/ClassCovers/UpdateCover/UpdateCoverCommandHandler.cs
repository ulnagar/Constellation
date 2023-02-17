namespace Constellation.Application.ClassCovers.UpdateCover;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions;
using Constellation.Core.Errors;
using Constellation.Core.Models.Covers;
using Constellation.Core.Shared;
using System.Threading;
using System.Threading.Tasks;

internal sealed class UpdateCoverCommandHandler
    : ICommandHandler<UpdateCoverCommand, ClassCover>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IClassCoverRepository _classCoverRepository;

    public UpdateCoverCommandHandler(
        IUnitOfWork unitOfWork,
        IClassCoverRepository classCoverRepository)
    {
        _unitOfWork = unitOfWork;
        _classCoverRepository = classCoverRepository;
    }

    public async Task<Result<ClassCover>> Handle(UpdateCoverCommand request, CancellationToken cancellationToken)
    {
        var cover = await _classCoverRepository.GetById(request.CoverId, cancellationToken);

        if (cover is null)
        {
            return Result.Failure<ClassCover>(DomainErrors.ClassCovers.Cover.NotFound(request.CoverId));
        }

        cover.EditDates(request.StartDate, request.EndDate);

        await _unitOfWork.CompleteAsync(cancellationToken);

        return cover;
    }
}
