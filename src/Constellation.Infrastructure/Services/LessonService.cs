namespace Constellation.Infrastructure.Services;

using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Interfaces.Services;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Enums;
using Constellation.Core.Models.Offerings.Identifiers;
using Constellation.Core.Models.SciencePracs;
using Constellation.Infrastructure.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;

public class LessonService : ILessonService, IScopedService
{
    private readonly ILessonRepository _lessonRepository;
    private readonly IUnitOfWork _unitOfWork;

    public LessonService(
        ILessonRepository lessonRepository,
        IUnitOfWork unitOfWork)
    {
        _lessonRepository = lessonRepository;
        _unitOfWork = unitOfWork;
    }

    // Rolls may need to be modified when
    //  1. Students withdraw from class or school (remove from all non-marked rolls)
    //  2. Students change partner school (remove from all non-marked rolls and re-add to new schools rolls)
    //  3. Students enrol in class (add to all future rolls)

    // A situation exists where a student might need to be removed from an overdue roll in school A
    // and then be added to future rolls in school B, but will forever not be linked to the overdue
    // lesson from the original school.

    // The solution is to leave them in the rolls for school A, as school B might have already completed that Prac.
    // That way, the student will be included in the Missed Lesson Report. (That report should include student enrolled
    // school as well as lesson roll school.

    public async Task RemoveStudentFromFutureRollsForCourse(string studentId, OfferingId offeringId)
    {
        List<SciencePracLesson> lessons = await _lessonRepository.GetAllForStudent(studentId);

        lessons = lessons
            .Where(lesson => 
                lesson.Offerings.Any(record => 
                    record.OfferingId == offeringId))
            .ToList();

        List<SciencePracRoll> rolls = lessons
            .SelectMany(lesson =>
                lesson.Rolls.Where(roll =>
                    roll.Attendance.Any(attendance =>
                        attendance.StudentId == studentId) &&
                    roll.Status == LessonStatus.Active))
            .ToList();

        foreach (SciencePracRoll roll in rolls)
        {
            if (roll.Attendance.Count == 1)
            {
                SciencePracLesson lesson = lessons.First(lesson => lesson.Id == roll.LessonId);

                lesson.Cancel();
            }

            roll.RemoveStudent(studentId);
        }
    }

    public async Task AddStudentToFutureRollsForCourse(string studentId, string schoolCode, OfferingId offeringId)
    {
        List<SciencePracLesson> lessons = await _lessonRepository.GetAllForOffering(offeringId);

        lessons = lessons
            .Where(lesson => 
                lesson.DueDate >= DateOnly.FromDateTime(DateTime.Today))
            .ToList();

        List<SciencePracRoll> rolls = lessons
            .SelectMany(lesson => 
                lesson.Rolls.Where(roll => 
                    roll.SchoolCode == schoolCode))
            .ToList();

        foreach (SciencePracRoll roll in rolls)
        {
            if (roll.Attendance.Any(attendance => attendance.StudentId == studentId))
                continue;

            roll.AddStudent(studentId);
        }

        List<SciencePracLesson> newRollsRequired = lessons
            .Where(lesson => 
                lesson.Rolls.All(roll => 
                    roll.SchoolCode != schoolCode))
            .ToList();

        foreach (SciencePracLesson lesson in newRollsRequired)
        {
            SciencePracRoll roll = new(
                lesson.Id,
                schoolCode);

            roll.AddStudent(studentId);

            lesson.AddRoll(roll);
        }

        await _unitOfWork.CompleteAsync();
    }
}
