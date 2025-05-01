namespace Constellation.Application.Domains.Students.Events.StudentWithdrawnDomainEvent;

using Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Models.Families;
using Core.Models.Students.Events;
using Core.Shared;
using Serilog;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

internal sealed class RemoveFromFamily 
    : IDomainEventHandler<StudentWithdrawnDomainEvent>
{
    private readonly IFamilyRepository _familyRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public RemoveFromFamily(
        IFamilyRepository familyRepository,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _familyRepository = familyRepository;
        _unitOfWork = unitOfWork;
        _logger = logger.ForContext<StudentWithdrawnDomainEvent>();
    }

    public async Task Handle(StudentWithdrawnDomainEvent notification, CancellationToken cancellationToken)
    {
        _logger.Information("Attempting to remove student {studentId} from family due to withdrawal", notification.StudentId);

        List<Family> families = await _familyRepository.GetFamiliesByStudentId(notification.StudentId, cancellationToken);

        foreach (Family family in families)
        {
            Result result = family.RemoveStudent(notification.StudentId);

            if (result.IsFailure)
            {
                _logger.Warning("Failed to remove student {studentId} from family {familyId}", notification.StudentId, family.Id);

                continue;
            }

            _logger.Warning("Student {studentId} removed from family {familyId}", notification.StudentId, family.Id);
        }

        await _unitOfWork.CompleteAsync(cancellationToken);
    }
}
