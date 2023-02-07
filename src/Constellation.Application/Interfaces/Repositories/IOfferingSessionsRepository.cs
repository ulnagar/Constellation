using Constellation.Core.Enums;
using Constellation.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Constellation.Application.Interfaces.Repositories
{
    public interface IOfferingSessionsRepository
    {
        Task<List<OfferingSession>> GetByOfferingId(int offeringId, CancellationToken cancellationToken = default);
        Task<List<string>> GetTimetableByOfferingId(int offeringId, CancellationToken cancellationToken = default);
        OfferingSession WithDetails(int id);
        OfferingSession WithFilter(Expression<Func<OfferingSession, bool>> predicate);
        ICollection<OfferingSession> All();
        ICollection<OfferingSession> AllWithFilter(Expression<Func<OfferingSession, bool>> predicate);
        ICollection<OfferingSession> AllForOffering(int id);
        ICollection<OfferingSession> AllForPeriod(int id);
        ICollection<OfferingSession> AllFromFaculty(Faculty faculty);
        ICollection<OfferingSession> AllFromGrade(Grade grade);
        ICollection<OfferingSession> AllForOfferingAndTeacher(int offeringId, string staffId);
        ICollection<OfferingSession> AllForOfferingAndRoom(int offeringId, string roomId);
        Task<ICollection<OfferingSession>> ForOfferingAndDay(int offeringId, int day);
        Task<ICollection<OfferingSession>> ForStudentAndDayAtTime(string studentId, int day, DateTime date);
        Task<ICollection<OfferingSession>> ForOfferingAndPeriod(int offeringId, int periodId);
        Task<OfferingSession> ForExistCheckAsync(int sessionId);
        Task<bool> AnyForOffering(int offeringId);
        Task<bool> AnyForOfferingAndTeacher(int offeringId, string staffId);
        Task<bool> AnyForOfferingAndRoom(int offeringId, string roomId);
        Task<OfferingSession> ForEditAsync(int sessionId);
    }
}