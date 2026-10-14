namespace Constellation.Core.Models.EnrolmentContext.EnrolmentStudent.Repositories;

using Identifiers;
using Students.ValueObjects;
using System.Collections.Generic;
using ValueObjects;

public interface IEnrolmentStudentRepository
{
    Task<List<EnrolmentStudent>> GetAll(CancellationToken cancellationToken = default);
    Task<EnrolmentStudent?> GetById(EnrolmentStudentId studentId, CancellationToken cancellationToken = default);
    Task<EnrolmentStudent?> GetByEmail(EmailAddress email, CancellationToken cancellationToken = default);
    Task<EnrolmentStudent?> GetBySRN(StudentReferenceNumber srn, CancellationToken cancellationToken = default);

    Task<List<EnrolmentStudent>> GetListFromIds(List<EnrolmentStudentId> studentIds, CancellationToken cancellationToken = default);

    void Insert(EnrolmentStudent student);
    void Remove(EnrolmentStudent student);
}
