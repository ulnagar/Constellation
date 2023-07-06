namespace Constellation.Application.MissedWork.GetOutstandingNotificationsForTeacher;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.MissedWork.Models;
using Constellation.Core.Abstractions;
using Constellation.Core.Models;
using Constellation.Core.Models.MissedWork;
using Constellation.Core.Shared;
using Constellation.Core.ValueObjects;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetOutstandingNotificationsForTeacherQueryHandler
    : IQueryHandler<GetOutstandingNotificationsForTeacherQuery, List<NotificationSummary>>
{
    private readonly IClassworkNotificationRepository _missedWorkRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly ICourseOfferingRepository _offeringRepository;
    private readonly ILogger _logger;

    public GetOutstandingNotificationsForTeacherQueryHandler(
        IClassworkNotificationRepository missedWorkRepository,
        IStudentRepository studentRepository,
        ICourseOfferingRepository offeringRepository,
        ILogger logger)
    {
        _missedWorkRepository = missedWorkRepository;
        _studentRepository = studentRepository;
        _offeringRepository = offeringRepository;
        _logger = logger;
    }

    public async Task<Result<List<NotificationSummary>>> Handle(GetOutstandingNotificationsForTeacherQuery request, CancellationToken cancellationToken) 
    {
        List<NotificationSummary> result = new();
        
        List<ClassworkNotification> notifications = await _missedWorkRepository.GetOutstandingForTeacher(request.StaffId, cancellationToken);

        foreach (ClassworkNotification notification in notifications)
        {
            List<string> studentIds = notification
                .Absences
                .Select(absence => absence.StudentId)
                .ToList();

            List<Student> students = await _studentRepository.GetListFromIds(studentIds, cancellationToken);

            List<Name> studentNames = new();

            foreach (Student student in students)
                studentNames.Add(student.GetName());

            CourseOffering offering = await _offeringRepository.GetById(notification.OfferingId, cancellationToken);

            result.Add(new(
                notification.Id,
                offering.Name,
                notification.AbsenceDate,
                studentNames,
                notification.CompletedAt.HasValue));
        }

        return result;
    }
}
