namespace Constellation.Application.Domains.Edval.Commands.RefreshClassMembershipDifferences;

using Abstractions.Messaging;
using Core.Models.Edval.Enums;
using Core.Models.Edval.Events;
using Core.Shared;
using Interfaces.Repositories;
using Repositories;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

internal sealed class RefreshClassMembershipDifferencesCommandHandler : ICommandHandler<RefreshClassMembershipDifferencesCommand>
{
    private readonly IEdvalRepository _edvalRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public RefreshClassMembershipDifferencesCommandHandler(
        IEdvalRepository edvalRepository,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _edvalRepository = edvalRepository;
        _unitOfWork = unitOfWork;
        _logger = logger
            .ForContext<RefreshClassMembershipDifferencesCommand>();
    }

    public async Task<Result> Handle(RefreshClassMembershipDifferencesCommand request, CancellationToken cancellationToken)
    {
        await _edvalRepository.ClearDifferences(EdvalDifferenceType.EdvalClassMembership, cancellationToken);

        _edvalRepository.AddIntegrationEvent(new EdvalClassMembershipsUpdatedIntegrationEvent(new()));

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}