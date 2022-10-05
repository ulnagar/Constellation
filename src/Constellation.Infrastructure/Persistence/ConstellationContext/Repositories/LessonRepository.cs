using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Enums;
using Constellation.Core.Models;
using Constellation.Infrastructure.Persistence.ConstellationContext;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Constellation.Infrastructure.Persistence.ConstellationContext.Repositories
{
    public class LessonRepository : ILessonRepository
    {
        private readonly AppDbContext _context;

        public LessonRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Lesson> Get(Guid id)
        {
            return await _context.Lessons.FirstOrDefaultAsync(lesson => lesson.Id == id);
        }

        public async Task<ICollection<Lesson>> GetAll()
        {
            return await _context.Lessons.ToListAsync();
        }

        public async Task<ICollection<Lesson>> GetWithRollsForSchool(string schoolCode)
        {
            return await _context.Lessons
                .Include(lesson => lesson.Offerings)
                    .ThenInclude(offering => offering.Course)
                .Include(lesson => lesson.Rolls)
                    .ThenInclude(roll => roll.Attendance)
                .Where(lesson => lesson.Rolls.Any(roll => roll.SchoolCode == schoolCode))
                .ToListAsync();
        }

        public async Task<ICollection<Lesson>> GetWithActiveRollsForSchool(string schoolCode)
        {
            return await _context.Lessons
                .Include(lesson => lesson.Offerings)
                    .ThenInclude(offering => offering.Course)
                .Include(lesson => lesson.Rolls)
                    .ThenInclude(roll => roll.Attendance)
                .Where(lesson => lesson.Rolls.Any(roll => roll.SchoolCode == schoolCode && roll.Status == LessonStatus.Active))
                .ToListAsync();
        }

        public async Task<ICollection<Lesson>> GetWithAllRollsForSchool(string schoolCode)
        {
            return await _context.Lessons
                .Include(lesson => lesson.Offerings)
                    .ThenInclude(offering => offering.Course)
                .Include(lesson => lesson.Rolls)
                    .ThenInclude(roll => roll.Attendance)
                .Where(lesson => lesson.Rolls.Any(roll => roll.SchoolCode == schoolCode) && lesson.DueDate.Year == DateTime.Now.Year)
                .ToListAsync();
        }

        public async Task<LessonRoll> GetRollForPortal(Guid id)
        {
            return await _context.LessonRolls
                .Include(roll => roll.Lesson)
                .Include(roll => roll.Attendance)
                    .ThenInclude(attendance => attendance.Student)
                .Include(roll => roll.School)
                .Include(roll => roll.SchoolContact)
                .SingleOrDefaultAsync(roll => roll.Id == id);
        }

        public async Task<ICollection<Lesson>> GetAllForPortalAdmin()
        {
            return await _context.Lessons
                .Include(lesson => lesson.Offerings)
                    .ThenInclude(offering => offering.Course)
                .Include(lesson => lesson.Rolls)
                    .ThenInclude(roll => roll.Attendance)
                .ToListAsync();
        }

        public async Task<Lesson> GetForEdit(Guid id)
        {
            return await _context.Lessons
                .Include(lesson => lesson.Offerings)
                    .ThenInclude(offering => offering.Course)
                .Include(lesson => lesson.Rolls)
                .SingleOrDefaultAsync(lesson => lesson.Id == id);
        }

        public async Task<Lesson> GetForDelete(Guid id)
        {
            return await _context.Lessons
                .Include(lesson => lesson.Rolls)
                    .ThenInclude(roll => roll.Attendance)
                .SingleOrDefaultAsync(lesson => lesson.Id == id);
        }

        public async Task<Lesson> GetWithDetailsForLessonsPortal(Guid id)
        {
            return await _context.Lessons
                .Include(lesson => lesson.Offerings)
                    .ThenInclude(offering => offering.Course)
                .Include(lesson => lesson.Rolls)
                    .ThenInclude(roll => roll.Attendance)
                        .ThenInclude(attend => attend.Student)
                .Include(lesson => lesson.Rolls)
                    .ThenInclude(roll => roll.School)
                .Include(lesson => lesson.Rolls)
                    .ThenInclude(roll => roll.SchoolContact)
                .SingleOrDefaultAsync(lesson => lesson.Id == id);
        }

        public async Task<ICollection<Lesson>> GetAllForCourse(int courseId)
        {
            return await _context.Lessons
                .Include(lesson => lesson.Offerings)
                .Include(lesson => lesson.Rolls)
                    .ThenInclude(roll => roll.Attendance)
                .Where(lesson => lesson.Offerings.Any(offering => offering.CourseId == courseId))
                .ToListAsync();
        }

        public async Task<ICollection<LessonRoll>> GetRollsForStudent(string studentId)
        {
            return await _context.LessonRolls
                .Include(roll => roll.Lesson)
                    .ThenInclude(lesson => lesson.Offerings)
                .Include(roll => roll.Lesson)
                    .ThenInclude(lesson => lesson.Rolls)
                .Include(roll => roll.Attendance)
                .Where(roll => roll.Attendance.Any(attend => attend.StudentId == studentId))
                .ToListAsync();
        }

        public async Task<ICollection<Lesson>> GetAllForNotifications()
        {
            return await _context.Lessons
                .Include(lesson => lesson.Rolls)
                    .ThenInclude(roll => roll.School)
                .Include(lesson => lesson.Offerings)
                    .ThenInclude(offering => offering.Course)
                .ToListAsync();
        }

        public async Task<ICollection<Lesson>> GetForClass(int code)
        {
            return await _context.Lessons
                .Include(lesson => lesson.Rolls)
                    .ThenInclude(roll => roll.School)
                .Include(lesson => lesson.Rolls)
                    .ThenInclude(roll => roll.Attendance)
                        .ThenInclude(attend => attend.Student)
                .Include(lesson => lesson.Offerings)
                .Where(lesson => lesson.Offerings.Any(offering => offering.Id == code))
                .ToListAsync();
        }
    }
}