namespace Constellation.Application.Students.Events.StudentEmailAddressChanged;

using Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Models.Operations;
using Core.Models.Students.Events;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

internal sealed class UpdateCanvasAccount
    :IDomainEventHandler<StudentEmailAddressChangedDomainEvent>
{
    private readonly ICanvasOperationsRepository _operationsRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public UpdateCanvasAccount(
        ICanvasOperationsRepository operationsRepository,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _operationsRepository = operationsRepository;
        _unitOfWork = unitOfWork;
        _logger = logger
            .ForContext<StudentEmailAddressChangedDomainEvent>();
    }

    public async Task Handle(StudentEmailAddressChangedDomainEvent notification, CancellationToken cancellationToken)
    {
        _logger.Information("Attempting to update student ({studentId}) to new email address in Canvas", notification.StudentId);

        UpdateUserEmailCanvasOperation operation = new(
            notification.StudentId.ToString(),
            notification.NewAddress.Email.Substring(0, notification.NewAddress.Email.IndexOf('@')));

        _operationsRepository.Insert(operation);
        await _unitOfWork.CompleteAsync(cancellationToken);

        _logger.Information("Scheduled student ({studentId}) email address update in Canvas", notification.StudentId);
    }
}