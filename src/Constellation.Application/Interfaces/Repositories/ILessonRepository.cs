namespace Constellation.Application.Interfaces.Repositories;

using Constellation.Core.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public interface ILessonRepository
{
    Task<List<Lesson>> GetOverdueForSchool(string schoolCode, CancellationToken cancellationToken = default);
    Task<Lesson> Get(Guid id);
    Task<ICollection<Lesson>> GetAll();
    Task<ICollection<Lesson>> GetWithRollsForSchool(string schoolCode);
    Task<ICollection<Lesson>> GetWithActiveRollsForSchool(string schoolCode);
    Task<LessonRoll> GetRollForPortal(Guid id);
    Task<ICollection<Lesson>> GetAllForPortalAdmin();
    Task<Lesson> GetForEdit(Guid id);
    Task<Lesson> GetForDelete(Guid id);
    Task<Lesson> GetWithDetailsForLessonsPortal(Guid id);
    Task<ICollection<Lesson>> GetAllForCourse(int courseId);
    Task<ICollection<LessonRoll>> GetRollsForStudent(string studentId);
    Task<ICollection<Lesson>> GetAllForNotifications();
    Task<ICollection<Lesson>> GetForClass(int code);
    Task<ICollection<Lesson>> GetWithAllRollsForSchool(string schoolCode);
}