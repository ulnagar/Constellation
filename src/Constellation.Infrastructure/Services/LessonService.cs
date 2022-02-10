using Constellation.Application.DTOs;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Interfaces.Services;
using Constellation.Core.Enums;
using Constellation.Core.Models;
using Constellation.Infrastructure.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Constellation.Infrastructure.Services
{
    // Reviewed for ASYNC Operations
    public class LessonService : ILessonService, IScopedService
    {
        private readonly IUnitOfWork _unitOfWork;

        public LessonService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task CreateNewLesson(string name, DateTime dueDate, bool skipRolls, int courseId)
        {
            // New lesson
            var lesson = new Lesson
            {
                Name = name,
                DueDate = dueDate,
                DoNotGenerateRolls = skipRolls
            };

            var course = await _unitOfWork.Courses.WithOfferingsForLessonsPortal(courseId);
            foreach (var offering in course.Offerings.Where(offering => offering.StartDate <= DateTime.Today && offering.EndDate >= DateTime.Today))
            {
                lesson.Offerings.Add(offering);
            }

            if (!lesson.DoNotGenerateRolls)
            {
                // Get all students enrolled in selected course
                var students = await _unitOfWork.Students.AllEnrolledInCourse(courseId);
                var schools = students.GroupBy(student => student.SchoolCode);

                foreach (var school in schools)
                {
                    var roll = new LessonRoll
                    {
                        SchoolCode = school.Key,
                        Status = LessonStatus.Active
                    };

                    foreach (var student in school)
                    {
                        roll.Attendance.Add(new LessonRoll.LessonRollStudentAttendance
                        {
                            StudentId = student.StudentId
                        });
                    }

                    lesson.Rolls.Add(roll);
                }
            }

            _unitOfWork.Add(lesson);
        }

        public async Task CreateNewLesson(LessonDto lesson)
        {
            // New lesson
            var newLesson = new Lesson
            {
                Name = lesson.Name,
                DueDate = lesson.DueDate,
                DoNotGenerateRolls = lesson.DoNotGenerateRolls
            };

            var course = await _unitOfWork.Courses.WithOfferingsForLessonsPortal(lesson.CourseId);
            foreach (var offering in course.Offerings.Where(offering => offering.StartDate <= DateTime.Today && offering.EndDate >= DateTime.Today))
            {
                newLesson.Offerings.Add(offering);
            }

            if (!lesson.DoNotGenerateRolls)
            {
                // Get all students enrolled in selected course
                var students = await _unitOfWork.Students.AllEnrolledInCourse(lesson.CourseId);
                var schools = students.GroupBy(student => student.SchoolCode);

                foreach (var school in schools)
                {
                    var roll = new LessonRoll
                    {
                        SchoolCode = school.Key,
                        Status = LessonStatus.Active
                    };

                    foreach (var student in school)
                    {
                        roll.Attendance.Add(new LessonRoll.LessonRollStudentAttendance
                        {
                            StudentId = student.StudentId
                        });
                    }

                    newLesson.Rolls.Add(roll);
                }
            }

            _unitOfWork.Add(newLesson);
        }

        public async Task UpdateExistingLesson(Guid lessonId, string name, DateTime dueDate, bool skipRolls, int courseId)
        {
            // Existing lesson needs to be updated
            var lesson = await _unitOfWork.Lessons.GetForEdit(lessonId);

            lesson.Name = name;
            lesson.DueDate = dueDate;
            lesson.DoNotGenerateRolls = skipRolls;

            foreach (var roll in lesson.Rolls)
            {
                _unitOfWork.Remove(roll);
            }

            if (!skipRolls)
            {
                // Get all students enrolled in selected course
                var students = await _unitOfWork.Students.AllEnrolledInCourse(courseId);
                var schools = students.GroupBy(student => student.SchoolCode);

                foreach (var school in schools)
                {
                    var roll = new LessonRoll
                    {
                        SchoolCode = school.Key
                    };

                    foreach (var student in school)
                    {
                        roll.Attendance.Add(new LessonRoll.LessonRollStudentAttendance
                        {
                            StudentId = student.StudentId
                        });
                    }

                    lesson.Rolls.Add(roll);
                }
            }
        }

        public async Task UpdateExistingLesson(LessonDto lesson)
        {
            // Existing lesson needs to be updated
            var existingLesson = await _unitOfWork.Lessons.GetForEdit(lesson.Id);

            existingLesson.Name = lesson.Name;
            existingLesson.DueDate = lesson.DueDate;
            existingLesson.DoNotGenerateRolls = lesson.DoNotGenerateRolls;

            foreach (var roll in existingLesson.Rolls)
            {
                _unitOfWork.Remove(roll);
            }

            if (!lesson.DoNotGenerateRolls)
            {
                // Get all students enrolled in selected course
                var students = await _unitOfWork.Students.AllEnrolledInCourse(lesson.CourseId);
                var schools = students.GroupBy(student => student.SchoolCode);

                foreach (var school in schools)
                {
                    var roll = new LessonRoll
                    {
                        SchoolCode = school.Key
                    };

                    foreach (var student in school)
                    {
                        roll.Attendance.Add(new LessonRoll.LessonRollStudentAttendance
                        {
                            StudentId = student.StudentId
                        });
                    }

                    existingLesson.Rolls.Add(roll);
                }
            }
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

        public async Task RemoveStudentFromFutureRollsForCourse(string studentId, int offeringId)
        {
            var rolls = await _unitOfWork.Lessons.GetRollsForStudent(studentId);

            // This should remove the user from ALL non-submitted rolls in the system.
            var rollsToRemove = rolls.Where(roll => roll.Lesson.Offerings.Any(offering => offering.Id == offeringId) && roll.Status == LessonStatus.Active).ToList();

            foreach (var roll in rollsToRemove)
            {
                if (roll.Attendance.Count == 1)
                {
                    _unitOfWork.Remove(roll);
                    continue;
                }

                var attendance = roll.Attendance.First(attend => attend.StudentId == studentId);

                _unitOfWork.Remove(attendance);
            }
        }

        public async Task AddStudentToFutureRollsForCourse(string studentId, string schoolCode, int offeringId)
        {
            var lessons = await _unitOfWork.Lessons.GetAllForCourse(offeringId);

            foreach (var lesson in lessons.Where(lesson => lesson.DueDate > DateTime.Today))
            {
                var roll = lesson.Rolls.FirstOrDefault(innerRoll => innerRoll.SchoolCode == schoolCode);

                if (roll != null)
                {
                    if (roll.Attendance.Any(attendance => attendance.StudentId == studentId))
                        continue;

                    roll.Attendance.Add(new LessonRoll.LessonRollStudentAttendance
                    {
                        StudentId = studentId
                    });
                }
                else
                {
                    roll = new LessonRoll
                    {
                        SchoolCode = schoolCode,
                        Status = LessonStatus.Active
                    };

                    roll.Attendance.Add(new LessonRoll.LessonRollStudentAttendance
                    {
                        StudentId = studentId
                    });

                    lesson.Rolls.Add(roll);
                }
            }
        }

        public async Task SubmitLessonRoll(LessonRollDto vm)
        {
            var roll = await _unitOfWork.Lessons.GetRollForPortal(vm.RollId);
            roll.LessonDate = vm.LessonDate;
            roll.SubmittedDate = DateTime.Today;
            roll.Comment = vm.Comment;
            roll.Status = LessonStatus.Completed;

            foreach (var attendanceRecord in roll.Attendance)
            {
                var vmRecord = vm.Attendance.FirstOrDefault(attendance => attendance.Id == attendanceRecord.Id);

                if (vmRecord != null)
                {
                    attendanceRecord.Present = vmRecord.Present;
                }
            }

            await _unitOfWork.CompleteAsync();
        }

        public async Task SubmitLessonRoll(LessonRollDto vm, SchoolContact coordinator)
        {
            var roll = await _unitOfWork.Lessons.GetRollForPortal(vm.RollId);
            roll.LessonDate = vm.LessonDate;
            roll.SubmittedDate = DateTime.Today;
            roll.SchoolContactId = coordinator.Id;
            roll.Comment = vm.Comment;
            roll.Status = LessonStatus.Completed;

            foreach (var attendanceRecord in roll.Attendance)
            {
                var vmRecord = vm.Attendance.FirstOrDefault(attendance => attendance.Id == attendanceRecord.Id);

                if (vmRecord != null)
                {
                    attendanceRecord.Present = vmRecord.Present;
                }
            }

            await _unitOfWork.CompleteAsync();
        }
    }
}
