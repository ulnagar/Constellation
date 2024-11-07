namespace Constellation.Application.Awards.CreateNominationPeriod;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Errors;
using Constellation.Core.Models.Awards;
using Constellation.Core.Shared;
using Core.Models.Awards.Errors;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

internal sealed class UpdateNominationPeriodCommandHandler
    : ICommandHandler<UpdateNominationPeriodCommand>
{
    private readonly IAwardNominationRepository _nominationRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public UpdateNominationPeriodCommandHandler(
        IAwardNominationRepository nominationRepository,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _nominationRepository = nominationRepository;
        _unitOfWork = unitOfWork;
        _logger = logger.ForContext<UpdateNominationPeriodCommand>();
    }

    public async Task<Result> Handle(UpdateNominationPeriodCommand request, CancellationToken cancellationToken)
    {
        NominationPeriod period = await _nominationRepository.GetById(request.PeriodId, cancellationToken);

        if (period is null)
        {
            _logger.Warning("Could not find Nomination Period with Id {id}", request.PeriodId);

            return Result.Failure(AwardNominationPeriodErrors.NotFound(request.PeriodId));
        }

        if (period.Name != request.Name)
        {
            _logger.Information("Updating Nomination Period {id}: Changing Name from {oldValue} to {newValue}", period.Id, period.Name, request.Name);
            period.UpdateName(request.Name);
        }

        if (period.LockoutDate != request.LockoutDate)
        {
            _logger.Information("Updating Nomination Period {id}: Changing LockoutDate from {oldValue} to {newValue}", period.Id, period.LockoutDate, request.LockoutDate);
            period.UpdateLockoutDate(request.LockoutDate);
        }

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
