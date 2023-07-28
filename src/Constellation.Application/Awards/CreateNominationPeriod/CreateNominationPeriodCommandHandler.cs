namespace Constellation.Application.Awards.CreateNominationPeriod;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions;
using Constellation.Core.Models.Awards;
using Constellation.Core.Shared;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

internal sealed class CreateNominationPeriodCommandHandler
    : ICommandHandler<CreateNominationPeriodCommand>
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

    public async Task<Result> Handle(CreateNominationPeriodCommand request, CancellationToken cancellationToken)
    {
        Result<NominationPeriod> periodRequest = NominationPeriod.Create(request.Grades, request.LockoutDate);

        if (periodRequest.IsFailure)
        {
            _logger
                .ForContext(nameof(periodRequest.Error), periodRequest.Error)
                .ForContext("Request", request)
                .Warning("Could not create Nomiination Period from request");

            return Result.Failure(periodRequest.Error);
        }

        _nominationRepository.Insert(periodRequest.Value);
        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
