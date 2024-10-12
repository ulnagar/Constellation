namespace Constellation.Core.Abstractions.Repositories;

using Models.Identifiers;
using Models.Offerings.Identifiers;
using Models.SciencePracs;
using Models.Students.Identifiers;
using Models.Subjects.Identifiers;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public interface ILessonRepository
{
    Task<List<SciencePracLesson>> GetAll(CancellationToken cancellationToken = default);
    Task<List<SciencePracLesson>> GetAllCurrent(CancellationToken cancellationToken = default);
    Task<List<SciencePracLesson>> GetAllForSchool(string schoolCode, CancellationToken cancellationToken = default);
    Task<List<SciencePracLesson>> GetAllForOffering(OfferingId offeringId, CancellationToken cancellationToken = default);
    Task<List<SciencePracLesson>> GetAllForCourse(CourseId courseId, CancellationToken cancellationToken = default);
    Task<List<SciencePracLesson>> GetAllForStudent(StudentId studentId, CancellationToken cancellationToken = default);
    Task<List<SciencePracLesson>> GetAllWithOverdueRolls(CancellationToken cancellationToken = default);
    Task<SciencePracLesson> GetById(SciencePracLessonId lessonId, CancellationToken cancellationToken = default);
    Task<List<SciencePracLesson>> GetWithoutPresentStudents(CancellationToken cancellationToken = default);
    void Insert(SciencePracLesson lesson);
    void Delete(SciencePracLesson lesson);
    void Delete(SciencePracAttendance attendance);
}