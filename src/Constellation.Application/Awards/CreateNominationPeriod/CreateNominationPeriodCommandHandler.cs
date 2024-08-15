namespace Constellation.Application.Awards.CreateNominationPeriod;

using Abstractions.Messaging;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Models.Awards;
using Core.Models.Identifiers;
using Core.Shared;
using Interfaces.Repositories;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

internal sealed class CreateNominationPeriodCommandHandler
    : ICommandHandler<CreateNominationPeriodCommand, AwardNominationPeriodId>
{
    private readonly IAwardNominationRepository _nominationRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public CreateNominationPeriodCommandHandler(
        IAwardNominationRepository nominationRepository,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _nominationRepository = nominationRepository;
        _unitOfWork = unitOfWork;
        _logger = logger.ForContext<CreateNominationPeriodCommand>();
    }

    public async Task<Result<AwardNominationPeriodId>> Handle(CreateNominationPeriodCommand request, CancellationToken cancellationToken)
    {
        Result<NominationPeriod> periodRequest = NominationPeriod.Create(request.Name, request.Grades, request.LockoutDate);

        if (periodRequest.IsFailure)
        {
            _logger
                .ForContext(nameof(periodRequest.Error), periodRequest.Error, true)
                .ForContext(nameof(CreateNominationPeriodCommand), request, true)
                .Warning("Could not create Nomination Period from request");

            return Result.Failure<AwardNominationPeriodId>(periodRequest.Error);
        }

        _nominationRepository.Insert(periodRequest.Value);
        await _unitOfWork.CompleteAsync(cancellationToken);

        return periodRequest.Value.Id;
    }
}
