using Constellation.Application.Features.Partners.Students.Notifications;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions;
using Constellation.Core.Models.Families;

namespace Constellation.Infrastructure.Features.Partners.Students.Notifications.StudentWithdrawn
{
    public class RemoveFromFamily : INotificationHandler<StudentWithdrawnNotification>
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
            _logger = logger.ForContext<StudentWithdrawnNotification>();
        }

        public async Task Handle(StudentWithdrawnNotification notification, CancellationToken cancellationToken)
        {
            _logger.Information("Attempting to remove student {studentId} from family due to withdrawal", notification.StudentId);

            List<Family> families = await _familyRepository.GetFamiliesByStudentId(notification.StudentId, cancellationToken);

            foreach (Family family in families)
            {
                var result = family.RemoveStudent(notification.StudentId);

                if (result.IsFailure)
                {
                    _logger.Warning("Failed to remove student {studentId} from family {familyId}", notification.StudentId, family.Id);

                    continue;
                }

                _logger.Warning("Student {studentId} removed from family {familyId}", notification.StudentId, family.Id);
            }
        }
    }
}
