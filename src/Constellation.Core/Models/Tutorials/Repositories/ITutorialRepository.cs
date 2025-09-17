namespace Constellation.Core.Models.Tutorials.Repositories;

using Identifiers;
using StaffMembers.Identifiers;
using Students.Identifiers;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Timetables.Enums;
using ValueObjects;

public interface ITutorialRepository
{
    Task<Tutorial> GetById(TutorialId tutorialId, CancellationToken cancellationToken = default);
    Task<List<Tutorial>> GetAll(CancellationToken cancellationToken = default);
    Task<List<Tutorial>> GetAllActive(CancellationToken cancellationToken = default);
    Task<List<Tutorial>> GetInactive(CancellationToken cancellationToken = default);
    Task<List<Tutorial>> GetActiveForTeacher(StaffId staffId, CancellationToken cancellationToken = default);
    Task<List<Tutorial>> GetAllForStudent(StudentId studentId, CancellationToken cancellationToken = default);
    Task<List<Tutorial>> GetCurrentEnrolmentsFromStudentForDate(StudentId studentId, DateOnly absenceDate, PeriodWeek week, PeriodDay day, CancellationToken cancellationToken = default);

    Task<bool> DoesTutorialAlreadyExist(TutorialName name, DateOnly startDate, DateOnly endDate, CancellationToken cancellationToken = default);
    void Insert(Tutorial tutorial);

    Task<Request> GetRequestById(RequestId requestId, CancellationToken cancellationToken = default);
    Task<List<Request>> GetAllRequests(CancellationToken cancellationToken = default);
    Task<List<Request>> GetPendingRequests(CancellationToken cancellationToken = default);
    Task<List<Request>> GetApprovedRequests(CancellationToken cancellationToken = default);
    Task<List<Request>> GetRequestsForStudent(StudentId studentId, CancellationToken cancellationToken = default);
    void Insert(Request request);
}
