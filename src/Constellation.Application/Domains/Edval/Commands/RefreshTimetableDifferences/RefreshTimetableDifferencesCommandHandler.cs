namespace Constellation.Application.Domains.Edval.Commands.RefreshTimetableDifferences;

using Abstractions.Messaging;
using Core.Models.Edval.Enums;
using Core.Models.Edval.Events;
using Core.Shared;
using Interfaces.Repositories;
using Repositories;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

internal sealed class RefreshTimetableDifferencesCommandHandler : ICommandHandler<RefreshTimetableDifferencesCommand>
{
    private readonly IEdvalRepository _edvalRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public RefreshTimetableDifferencesCommandHandler(
        IEdvalRepository edvalRepository,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _edvalRepository = edvalRepository;
        _unitOfWork = unitOfWork;
        _logger = logger
            .ForContext<RefreshTimetableDifferencesCommand>();
    }

    public async Task<Result> Handle(RefreshTimetableDifferencesCommand request, CancellationToken cancellationToken)
    {
        await _edvalRepository.ClearDifferences(EdvalDifferenceType.EdvalTimetable, cancellationToken);

        _edvalRepository.AddIntegrationEvent(new EdvalTimetablesUpdatedIntegrationEvent(new()));

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}