namespace Constellation.Application.Interfaces.Repositories;

using Constellation.Core.Models.SciencePracs;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public interface ILessonRepository
{
    Task<List<SciencePracLesson>> GetOverdueForSchool(string schoolCode, CancellationToken cancellationToken = default);
    Task<SciencePracLesson> Get(Guid id);
    Task<ICollection<SciencePracLesson>> GetAll();
    Task<ICollection<SciencePracLesson>> GetWithRollsForSchool(string schoolCode);
    Task<ICollection<SciencePracLesson>> GetWithActiveRollsForSchool(string schoolCode);
    Task<SciencePracRoll> GetRollForPortal(Guid id);
    Task<ICollection<SciencePracLesson>> GetAllForPortalAdmin();
    Task<SciencePracLesson> GetForEdit(Guid id);
    Task<SciencePracLesson> GetForDelete(Guid id);
    Task<SciencePracLesson> GetWithDetailsForLessonsPortal(Guid id);
    Task<ICollection<SciencePracLesson>> GetAllForCourse(int courseId);
    Task<ICollection<SciencePracRoll>> GetRollsForStudent(string studentId);
    Task<ICollection<SciencePracLesson>> GetAllForNotifications();
    Task<ICollection<SciencePracLesson>> GetForClass(int code);
    Task<ICollection<SciencePracLesson>> GetWithAllRollsForSchool(string schoolCode);
}