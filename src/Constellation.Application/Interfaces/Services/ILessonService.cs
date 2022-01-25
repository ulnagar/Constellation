using Constellation.Application.DTOs;
using Constellation.Core.Models;
using System;
using System.Threading.Tasks;

namespace Constellation.Application.Interfaces.Services
{
    public interface ILessonService
    {
        Task CreateNewLesson(string name, DateTime dueDate, bool skipRolls, int courseId);
        Task CreateNewLesson(LessonDto lesson);
        Task UpdateExistingLesson(Guid lessonId, string name, DateTime dueDate, bool skipRolls, int courseId);
        Task UpdateExistingLesson(LessonDto lesson);
        Task RemoveStudentFromFutureRollsForCourse(string studentId, int offeringId);
        Task AddStudentToFutureRollsForCourse(string studentId, string schoolCode, int offeringId);
        Task SubmitLessonRoll(LessonRollDto vm);
        Task SubmitLessonRoll(LessonRollDto vm, SchoolContact coordinator);
    }
}
