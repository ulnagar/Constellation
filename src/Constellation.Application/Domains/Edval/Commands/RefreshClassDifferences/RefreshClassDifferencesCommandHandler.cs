namespace Constellation.Application.Domains.Edval.Commands.RefreshClassDifferences;

using Abstractions.Messaging;
using Constellation.Core.Models.Edval.Events;
using Core.Models.Edval.Enums;
using Core.Shared;
using Interfaces.Repositories;
using Repositories;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

internal sealed class RefreshClassDifferencesCommandHandler
: ICommandHandler<RefreshClassDifferencesCommand>
{
    private readonly IEdvalRepository _edvalRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public RefreshClassDifferencesCommandHandler(
        IEdvalRepository edvalRepository,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _edvalRepository = edvalRepository;
        _unitOfWork = unitOfWork;
        _logger = logger
            .ForContext<RefreshClassDifferencesCommand>();
    }

    public async Task<Result> Handle(RefreshClassDifferencesCommand request, CancellationToken cancellationToken)
    {
        await _edvalRepository.ClearDifferences(EdvalDifferenceType.EdvalClass, cancellationToken);

        _edvalRepository.AddIntegrationEvent(new EdvalClassesUpdatedIntegrationEvent(new()));

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
