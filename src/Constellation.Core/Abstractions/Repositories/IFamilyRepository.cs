namespace Constellation.Core.Abstractions.Repositories;

using Models.Families;
using Models.Identifiers;
using Models.Students.Identifiers;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ValueObjects;

public interface IFamilyRepository
{
    Task<bool> DoesFamilyWithEmailExist(EmailAddress email, CancellationToken cancellationToken = default);
    Task<List<Family>> GetAll(CancellationToken cancellationToken = default);
    Task<List<Family>> GetAllCurrent(CancellationToken cancellationToken = default);
    Task<Family> GetFamilyBySentralId(string SentralId, CancellationToken cancellationToken = default);
    Task<Family> GetFamilyById(FamilyId Id, CancellationToken cancellationToken = default);
    Task<Family> GetFamilyByEmail(EmailAddress email, CancellationToken cancellationToken = default);
    Task<List<Family>> GetFamiliesByStudentId(StudentId studentId, CancellationToken cancellationToken = default);
    Task<bool> DoesEmailBelongToParentOrFamily(string email, CancellationToken cancellationToken = default);
    Task<Dictionary<StudentId, bool>> GetStudentIdsFromFamilyWithEmail(string email, CancellationToken cancellation = default);
    Task<int> CountOfParentsWithEmailAddress(string email, CancellationToken cancellationToken = default);
    void Insert(Family family);
    void Remove(Parent parent);
    void Remove(StudentFamilyMembership student);
}
