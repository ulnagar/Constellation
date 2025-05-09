namespace Constellation.Application.Domains.Edval.Commands.RefreshStudentDifferences;

using Abstractions.Messaging;
using Core.Models.Edval.Enums;
using Core.Models.Edval.Events;
using Core.Shared;
using Interfaces.Repositories;
using Repositories;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

internal sealed class RefreshStudentDifferencesCommandHandler : ICommandHandler<RefreshStudentDifferencesCommand>
{
    private readonly IEdvalRepository _edvalRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public RefreshStudentDifferencesCommandHandler(
        IEdvalRepository edvalRepository,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _edvalRepository = edvalRepository;
        _unitOfWork = unitOfWork;
        _logger = logger
            .ForContext<RefreshStudentDifferencesCommand>();
    }

    public async Task<Result> Handle(RefreshStudentDifferencesCommand request, CancellationToken cancellationToken)
    {
        await _edvalRepository.ClearDifferences(EdvalDifferenceType.EdvalStudent, cancellationToken);

        _edvalRepository.AddIntegrationEvent(new EdvalStudentsUpdatedIntegrationEvent(new()));

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}