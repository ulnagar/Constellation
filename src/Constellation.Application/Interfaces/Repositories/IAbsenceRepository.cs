using Constellation.Application.DTOs;
using Constellation.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Constellation.Application.Interfaces.Repositories
{
    public interface IAbsenceRepository
    {
        Task<List<Absence>> GetForStudentFromCurrentYear(string StudentId, CancellationToken cancellationToken = default);
        Task<ICollection<Absence>> All();
        Task<ICollection<Absence>> AllWithFilter(Expression<Func<Absence, bool>> predicate);
        Task<Absence> WithDetails(string id);
        Absence WholeWithDetails(string id);
        Absence WholeWithFilter(Expression<Func<Absence, bool>> predicate);
        ICollection<Absence> AllWholeWithFilter(Expression<Func<Absence, bool>> predicate);
        ICollection<Absence> AllWhole();
        Absence PartialWithDetails(string id);
        Absence PartialWithFilter(Expression<Func<Absence, bool>> predicate);
        ICollection<Absence> AllPartialWithFilter(Expression<Func<Absence, bool>> predicate);
        ICollection<Absence> AllPartial();
        Task<Absence> ForSendingNotificationAsync(string id);
        Task<ICollection<Absence>> ForReportAsync(AbsenceFilterDto filter);
        Task<ICollection<Absence>> AllFromStudentForParentPortal(string studentId);
        Task<Absence> ForExplanationFromParent(Guid id);
        Task<Absence> ForExplanationFromStudent(Guid id);
        Task<ICollection<Absence>> AllFromSchoolForCoordinatorPortal(string schoolCode);
        Task<AbsenceResponse> AsResponseForVerificationByCoordinator(Guid id);
        Task<ICollection<Absence>> ForClassworkNotifications(DateTime scanDate);
        Task<ICollection<Absence>> ForStudentWithinTimePeriod(string studentId, DateTime startDate, DateTime endDate);
    }
}
