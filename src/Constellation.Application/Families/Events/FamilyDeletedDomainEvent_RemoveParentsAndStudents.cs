namespace Constellation.Application.Families.Events;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.DomainEvents;
using Constellation.Core.Errors;
using Constellation.Core.Models.Families;
using Constellation.Core.Shared;
using Serilog;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class FamilyDeletedDomainEvent_RemoveParentsAndStudents
    : IDomainEventHandler<FamilyDeletedDomainEvent>
{
    private readonly IFamilyRepository _familyRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public FamilyDeletedDomainEvent_RemoveParentsAndStudents(
        IFamilyRepository familyRepository,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _familyRepository = familyRepository;
        _unitOfWork = unitOfWork;
        _logger = logger.ForContext<FamilyDeletedDomainEvent>();
    }

    public async Task Handle(FamilyDeletedDomainEvent notification, CancellationToken cancellationToken)
    {
        Family family = await _familyRepository.GetFamilyById(notification.FamilyId, cancellationToken);

        if (family is null)
        {
            _logger
                .ForContext(nameof(FamilyDeletedDomainEvent), notification, true)
                .ForContext(nameof(Error), DomainErrors.Families.Family.NotFound(notification.FamilyId), true)
                .Warning("Could not find family with Id {familyId}", notification.FamilyId);

            return;
        }

        if (family.Parents.Any())
        {
            foreach (Parent parent in family.Parents)
            {
                family.RemoveParent(parent.Id);

                _familyRepository.Remove(parent);
            }
        }

        if (family.Students.Any())
        {
            foreach (StudentFamilyMembership student in family.Students)
            {
                family.RemoveStudent(student.StudentId);

                _familyRepository.Remove(student);
            }
        }

        await _unitOfWork.CompleteAsync(cancellationToken);
    }
}
