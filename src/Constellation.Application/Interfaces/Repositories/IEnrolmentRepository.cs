namespace Constellation.Application.Interfaces.Repositories;

using Constellation.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

public interface IEnrolmentRepository
{
    Task<List<Enrolment>> GetCurrentByStudentId(string studentId, CancellationToken cancellationToken = default);
    Task<List<Enrolment>> GetCurrentByOfferingId(int offeringId, CancellationToken cancellationToken = default);
    Enrolment WithDetails(int id);
    Enrolment WithFilter(Expression<Func<Enrolment, bool>> predicate);
    ICollection<Enrolment> All();
    ICollection<Enrolment> AllWithFilter(Expression<Func<Enrolment, bool>> predicate);
    ICollection<Enrolment> AllFromOffering(int id);
    ICollection<Enrolment> CurrentForStudent(string id);
    ICollection<Enrolment> AllForStudent(string id);
    Task<Enrolment> ForEditing(int id);
    Task<bool> AnyForStudentAndOffering(string studentId, int offeringId);
}