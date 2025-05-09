namespace Constellation.Application.Domains.Edval.Commands.RefreshDifferences;

using Abstractions.Messaging;
using Core.Models.Edval.Enums;
using Core.Models.Edval.Events;
using Core.Shared;
using Interfaces.Repositories;
using Repositories;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

public sealed record RefreshDifferencesCommand()
    : ICommand;

internal sealed class RefreshDifferencesCommandHandler : ICommandHandler<RefreshDifferencesCommand>
{
    private readonly IEdvalRepository _edvalRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public RefreshDifferencesCommandHandler(
        IEdvalRepository edvalRepository,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _edvalRepository = edvalRepository;
        _unitOfWork = unitOfWork;
        _logger = logger.ForContext<RefreshDifferencesCommand>();
    }

    public async Task<Result> Handle(RefreshDifferencesCommand request, CancellationToken cancellationToken)
    {
        await _edvalRepository.ClearDifferences(EdvalDifferenceType.EdvalClass, cancellationToken);
        await _edvalRepository.ClearDifferences(EdvalDifferenceType.EdvalClassMembership, cancellationToken);
        await _edvalRepository.ClearDifferences(EdvalDifferenceType.EdvalStudent, cancellationToken);
        await _edvalRepository.ClearDifferences(EdvalDifferenceType.EdvalTeacher, cancellationToken);
        await _edvalRepository.ClearDifferences(EdvalDifferenceType.EdvalTimetable, cancellationToken);

        _edvalRepository.AddIntegrationEvent(new EdvalClassesUpdatedIntegrationEvent(new()));
        _edvalRepository.AddIntegrationEvent(new EdvalClassMembershipsUpdatedIntegrationEvent(new()));
        _edvalRepository.AddIntegrationEvent(new EdvalStudentsUpdatedIntegrationEvent(new()));
        _edvalRepository.AddIntegrationEvent(new EdvalTeachersUpdatedIntegrationEvent(new()));
        _edvalRepository.AddIntegrationEvent(new EdvalTimetablesUpdatedIntegrationEvent(new()));

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
