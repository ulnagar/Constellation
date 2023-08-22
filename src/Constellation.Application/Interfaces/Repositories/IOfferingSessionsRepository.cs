namespace Constellation.Application.Interfaces.Repositories;

using Constellation.Core.Enums;
using Constellation.Core.Models;
using Constellation.Core.Models.Subjects;
using Constellation.Core.Models.Subjects.Identifiers;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

public interface IOfferingSessionsRepository
{
    Task<List<Session>> GetByOfferingId(OfferingId offeringId, CancellationToken cancellationToken = default);
    Task<List<string>> GetTimetableByOfferingId(OfferingId offeringId, CancellationToken cancellationToken = default);
    Task<List<Session>> GetAllForStudentAndDayDuringTime(string studentId, int day, DateOnly date, CancellationToken cancellationToken = default);

    Session WithDetails(int id);
    Session WithFilter(Expression<Func<Session, bool>> predicate);
    ICollection<Session> All();
    ICollection<Session> AllWithFilter(Expression<Func<Session, bool>> predicate);
    ICollection<Session> AllForOffering(OfferingId id);
    ICollection<Session> AllForPeriod(int id);
    ICollection<Session> AllFromFaculty(Faculty faculty);
    ICollection<Session> AllFromGrade(Grade grade);
    ICollection<Session> AllForOfferingAndTeacher(OfferingId offeringId, string staffId);
    ICollection<Session> AllForOfferingAndRoom(OfferingId offeringId, string roomId);
    Task<ICollection<Session>> ForOfferingAndDay(OfferingId offeringId, int day);
    Task<ICollection<Session>> ForOfferingAndPeriod(OfferingId offeringId, int periodId);
    Task<Session> ForExistCheckAsync(int sessionId);
    Task<bool> AnyForOffering(OfferingId offeringId);
    Task<bool> AnyForOfferingAndTeacher(OfferingId offeringId, string staffId);
    Task<bool> AnyForOfferingAndRoom(OfferingId offeringId, string roomId);
    Task<Session> ForEditAsync(int sessionId);
}