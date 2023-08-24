namespace Constellation.Core.Abstractions.Repositories;

using Constellation.Core.Models.Identifiers;
using Constellation.Core.Models.SciencePracs;
using Constellation.Core.Models.Subjects.Identifiers;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public interface ILessonRepository
{
    Task<List<SciencePracLesson>> GetAll(CancellationToken cancellationToken = default);
    Task<List<SciencePracLesson>> GetAllCurrent(CancellationToken cancellationToken = default);
    Task<List<SciencePracLesson>> GetAllForSchool(string SchoolCode, CancellationToken cancellationToken = default);
    Task<List<SciencePracLesson>> GetAllForOffering(OfferingId OfferingId, CancellationToken cancellationToken = default);
    Task<List<SciencePracLesson>> GetAllForCourse(int CourseId, CancellationToken cancellationToken = default);
    Task<List<SciencePracLesson>> GetAllForStudent(string StudentId, CancellationToken cancellationToken = default);
    Task<SciencePracLesson> GetById(SciencePracLessonId LessonId, CancellationToken cancellationToken = default);
    void Insert(SciencePracLesson lesson);
    void Delete(SciencePracLesson lesson);
    void Delete(SciencePracAttendance attendance);
}