namespace Constellation.Application.Domains.Families.Events.FamilyDeleted;

using Abstractions.Messaging;
using Core.Abstractions.Repositories;
using Core.Models.Families;
using Core.Models.Families.Errors;
using Core.Models.Families.Events;
using Core.Shared;
using Interfaces.Repositories;
using Serilog;

internal sealed class RemoveParentsAndStudents
    : IDomainEventHandler<FamilyDeletedDomainEvent>
{
    private readonly IFamilyRepository _familyRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public RemoveParentsAndStudents(
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
        Family? family = await _familyRepository.GetFamilyById(notification.FamilyId, cancellationToken);

        if (family is null)
        {
            _logger
                .ForContext(nameof(FamilyDeletedDomainEvent), notification, true)
                .ForContext(nameof(Error), FamilyErrors.NotFound(notification.FamilyId), true)
                .Warning("Could not find family with Id {familyId}", notification.FamilyId);

            return;
        }

        if (family.Parents.Count > 0)
        {
            foreach (Parent parent in family.Parents)
            {
                family.RemoveParent(parent.Id);

                _familyRepository.Remove(parent);
            }
        }

        if (family.Students.Count > 0)
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
