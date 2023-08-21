namespace Constellation.Application.Interfaces.Repositories;

using Constellation.Core.Enums;
using Constellation.Core.Models;
using Constellation.Core.Models.Subjects;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

public interface IOfferingSessionsRepository
{
    Task<List<Session>> GetByOfferingId(int offeringId, CancellationToken cancellationToken = default);
    Task<List<string>> GetTimetableByOfferingId(int offeringId, CancellationToken cancellationToken = default);
    Task<List<Session>> GetAllForStudentAndDayDuringTime(string studentId, int day, DateOnly date, CancellationToken cancellationToken = default);

    Session WithDetails(int id);
    Session WithFilter(Expression<Func<Session, bool>> predicate);
    ICollection<Session> All();
    ICollection<Session> AllWithFilter(Expression<Func<Session, bool>> predicate);
    ICollection<Session> AllForOffering(int id);
    ICollection<Session> AllForPeriod(int id);
    ICollection<Session> AllFromFaculty(Faculty faculty);
    ICollection<Session> AllFromGrade(Grade grade);
    ICollection<Session> AllForOfferingAndTeacher(int offeringId, string staffId);
    ICollection<Session> AllForOfferingAndRoom(int offeringId, string roomId);
    Task<ICollection<Session>> ForOfferingAndDay(int offeringId, int day);
    Task<ICollection<Session>> ForOfferingAndPeriod(int offeringId, int periodId);
    Task<Session> ForExistCheckAsync(int sessionId);
    Task<bool> AnyForOffering(int offeringId);
    Task<bool> AnyForOfferingAndTeacher(int offeringId, string staffId);
    Task<bool> AnyForOfferingAndRoom(int offeringId, string roomId);
    Task<Session> ForEditAsync(int sessionId);
}