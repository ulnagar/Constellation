using Constellation.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Constellation.Application.Interfaces.Repositories
{
    public interface IEnrolmentRepository
    {
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
}