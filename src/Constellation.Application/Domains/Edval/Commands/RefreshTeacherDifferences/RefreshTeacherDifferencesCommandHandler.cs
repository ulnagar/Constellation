namespace Constellation.Application.Domains.Edval.Commands.RefreshTeacherDifferences;

using Abstractions.Messaging;
using Core.Models.Edval.Enums;
using Core.Models.Edval.Events;
using Core.Shared;
using Interfaces.Repositories;
using Repositories;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

internal sealed class RefreshTeacherDifferencesCommandHandler : ICommandHandler<RefreshTeacherDifferencesCommand>
{
    private readonly IEdvalRepository _edvalRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public RefreshTeacherDifferencesCommandHandler(
        IEdvalRepository edvalRepository,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _edvalRepository = edvalRepository;
        _unitOfWork = unitOfWork;
        _logger = logger
            .ForContext<RefreshTeacherDifferencesCommand>();
    }

    public async Task<Result> Handle(RefreshTeacherDifferencesCommand request, CancellationToken cancellationToken)
    {
        await _edvalRepository.ClearDifferences(EdvalDifferenceType.EdvalTeacher, cancellationToken);

        _edvalRepository.AddIntegrationEvent(new EdvalTeachersUpdatedIntegrationEvent(new()));

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}