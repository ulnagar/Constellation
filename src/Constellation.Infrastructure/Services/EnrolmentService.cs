namespace Constellation.Infrastructure.Services;

using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Interfaces.Services;
using Constellation.Core.Abstractions.Clock;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Models;
using Constellation.Core.Models.Subjects.Identifiers;
using Constellation.Infrastructure.DependencyInjection;

public class EnrolmentService : IEnrolmentService, IScopedService
{
    private readonly IOperationService _operationService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IOfferingRepository _offeringRepository;
    private readonly ILessonService _lessonService;
    private readonly IDateTimeProvider _dateTime;

    public EnrolmentService(
        IUnitOfWork unitOfWork, 
        IOfferingRepository offeringRepository,
        IOperationService operationService,
        ILessonService lessonService,
        IDateTimeProvider dateTime)
    {
        _unitOfWork = unitOfWork;
        _offeringRepository = offeringRepository;
        _lessonService = lessonService;
        _dateTime = dateTime;
        _operationService = operationService;
    }

    public async Task CreateEnrolment(string studentId, OfferingId offeringId, DateTime dateCreated)
    {
        //Validate entries
        var student = await _unitOfWork.Students.ForEditAsync(studentId);
        var offering = await _offeringRepository.GetById(offeringId);

        if (student == null || offering == null)
            return;

        // Ensure that there isn't already an enrolment for this combination
        if (await _unitOfWork.Enrolments.AnyForStudentAndOffering(studentId, offeringId))
            return;

        var enrolment = new Enrolment()
        {
            StudentId = studentId,
            OfferingId = offeringId,
            DateCreated = dateCreated
        };

        _unitOfWork.Add(enrolment);

        if (offering.IsCurrent)
        {
            await _lessonService.AddStudentToFutureRollsForCourse(studentId, student.SchoolCode, offeringId);

            foreach (var room in offering.Sessions.Where(s => !s.IsDeleted).Select(s => s.Room).Distinct().ToList())
                await _operationService.CreateStudentAdobeConnectAccess(student.StudentId, room.ScoId, _dateTime.Now);

            await _operationService.CreateStudentMSTeamMemberAccess(student.StudentId, offering.Id, _dateTime.Now);
            await _operationService.EnrolStudentInCanvasCourse(student, offering);
        }

        if (offering.StartDate >= _dateTime.Today)
        {
            foreach (var room in offering.Sessions.Where(s => !s.IsDeleted).Select(s => s.Room).Distinct().ToList())
                await _operationService.CreateStudentAdobeConnectAccess(student.StudentId, room.ScoId, offering.StartDate.ToDateTime(TimeOnly.MinValue));

            await _operationService.CreateStudentMSTeamMemberAccess(student.StudentId, offering.Id, offering.StartDate.ToDateTime(TimeOnly.MinValue));
            await _operationService.EnrolStudentInCanvasCourse(student, offering, offering.StartDate.ToDateTime(TimeOnly.MinValue));
        }

        await _unitOfWork.CompleteAsync();

    }

    public async Task RemoveEnrolment(int enrolmentId)
    {
        var enrolment = await _unitOfWork.Enrolments.ForEditing(enrolmentId);

        // Is this a valid enrolment?
        if (enrolment == null)
            return;

        // Has this already been deleted?
        if (enrolment.IsDeleted)
            return;

        // Is this a current offering?
        if (enrolment.Offering.IsCurrent)
        {
            // Remove the student from future rolls that are unsubmitted
            await _lessonService.RemoveStudentFromFutureRollsForCourse(enrolment.StudentId, enrolment.OfferingId);

            // Call OperationService.RemoveStudentAdobeConnectAccess(Student, Offering)
            foreach (var room in enrolment.Offering.Sessions.Where(s => !s.IsDeleted).Select(s => s.Room).Distinct().ToList())
                await _operationService.RemoveStudentAdobeConnectAccess(enrolment.Student.StudentId, room.ScoId, _dateTime.Now);

            await _operationService.RemoveStudentMSTeamAccess(enrolment.Student.StudentId, enrolment.Offering.Id, _dateTime.Now);
            await _operationService.UnenrolStudentFromCanvasCourse(enrolment.Student, enrolment.Offering);
        }

        enrolment.IsDeleted = true;
        enrolment.DateDeleted = _dateTime.Now;
    }
}