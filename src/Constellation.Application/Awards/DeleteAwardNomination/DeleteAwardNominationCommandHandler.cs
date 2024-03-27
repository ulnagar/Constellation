namespace Constellation.Application.Awards.DeleteAwardNomination;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Errors;
using Constellation.Core.Models.Awards;
using Constellation.Core.Shared;
using Serilog;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class DeleteAwardNominationCommandHandler
    : ICommandHandler<DeleteAwardNominationCommand>
{
    private readonly IAwardNominationRepository _nominationRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public DeleteAwardNominationCommandHandler(
        IAwardNominationRepository nominationRepository,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _nominationRepository = nominationRepository;
        _unitOfWork = unitOfWork;
        _logger = logger.ForContext<DeleteAwardNominationCommand>();
    }

    public async Task<Result> Handle(DeleteAwardNominationCommand request, CancellationToken cancellationToken)
    {
        NominationPeriod period = await _nominationRepository.GetById(request.PeriodId, cancellationToken);

        if (period is null)
        {
            _logger.Warning("Could not find Award Nomination Period with Id {id}", request.PeriodId);

            return Result.Failure(DomainErrors.Awards.NominationPeriod.NotFound(request.PeriodId));
        }

        Nomination nomination = period.Nominations.FirstOrDefault(nomination => nomination.Id == request.NominationId);

        if (nomination is null)
        {
            _logger.Warning("Could not find Award Nomination with Id {id} within Period", request.NominationId);

            return Result.Failure(DomainErrors.Awards.Nomination.NotFound(request.NominationId));
        }

        nomination.Delete();

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
