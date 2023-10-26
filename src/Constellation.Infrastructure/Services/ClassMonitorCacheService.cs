using Constellation.Application.DTOs;
using Constellation.Application.Extensions;
using Constellation.Application.Interfaces.Services;
using Constellation.Application.Rooms.GetRoomById;
using Constellation.Application.Rooms.Models;
using Constellation.Core.Comparators;
using Constellation.Core.Models;
using Constellation.Core.Models.Casuals;
using Constellation.Core.Models.Enrolments;
using Constellation.Core.Models.Offerings;
using Constellation.Core.Models.Offerings.ValueObjects;
using Constellation.Core.Models.Students;
using Constellation.Core.Models.Subjects;
using Constellation.Infrastructure.DependencyInjection;
using Constellation.Infrastructure.Persistence.ConstellationContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Constellation.Infrastructure.Services
{
    // Reviewed for ASYNC Operations
    public class ClassMonitorCacheService : IClassMonitorCacheService, ISingletonService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private ClassMonitorDtos Cache { get; set; }

        private ICollection<ClassMonitorDtos.MonitorCourse> Statuses { get; set; }

        public ClassMonitorCacheService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
            Statuses = new List<ClassMonitorDtos.MonitorCourse>();
        }

        public async Task<ClassMonitorDtos> GetData()
        {
            if (Cache == null || DateTime.Now.Subtract(Cache.Refreshed) > new TimeSpan(1, 0, 0))
            {
                await InitCache();
            }

            return Cache;
        }

        public void UpdateScan(ICollection<ClassMonitorDtos.MonitorCourse> courses)
        {
            Statuses = courses;
        }

        public async Task<ICollection<ClassMonitorDtos.MonitorCourse>> GetCurrentStatus()
        {
            return await Task.FromResult(Statuses);
        }

        private async Task InitCache()
        {
            Cache = new ClassMonitorDtos();

            var cacheTasks = new List<Task>
            {
                GetCourses(),
                GetPeriods(),
                GetUsers()
            };

            await Task.WhenAll(cacheTasks);
            Cache.Refreshed = DateTime.Now;
        }

        private async Task GetUsers()
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var students = await context.Students
                .Where(student => !student.IsDeleted)
                .AsNoTracking()
                .ToListAsync();

            var staff = await context.Staff
                .Where(teacher => !teacher.IsDeleted)
                .AsNoTracking()
                .ToListAsync();

            var casuals = await context.Set<Casual>()
                .Where(casual => !casual.IsDeleted)
                .AsNoTracking()
                .ToListAsync();

            foreach (var student in students)
            {
                var dto = new ClassMonitorDtos.MonitorUser
                {
                    Id = student.StudentId,
                    DisplayName = student.DisplayName,
                    Gender = student.Gender,
                    UserType = "Student",
                    UserPrincipalId = student.AdobeConnectPrincipalId,
                    IsDeleted = student.IsDeleted
                };

                Cache.Users.Add(dto);
            }

            foreach (var teacher in staff)
            {
                var dto = new ClassMonitorDtos.MonitorUser
                {
                    Id = teacher.StaffId,
                    DisplayName = teacher.DisplayName,
                    UserType = "Staff",
                    UserPrincipalId = teacher.AdobeConnectPrincipalId,
                    IsDeleted = teacher.IsDeleted
                };

                Cache.Users.Add(dto);
            }

            foreach (var casual in casuals)
            {
                var dto = new ClassMonitorDtos.MonitorUser
                {
                    Id = casual.Id.ToString(),
                    DisplayName = casual.DisplayName,
                    UserType = "Casual",
                    UserPrincipalId = casual.AdobeConnectId,
                    IsDeleted = casual.IsDeleted
                };

                Cache.Users.Add(dto);
            }
        }

        private async Task GetPeriods()
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var periods = await context.Periods
                .AsNoTracking()
                .ToListAsync();

            foreach (var period in periods)
            {
                var dto = new ClassMonitorDtos.MonitorPeriod
                {
                    Id = period.Id,
                    Name = period.Name,
                    Timetable = period.Timetable,
                    Day = period.Day,
                    Type = period.Type,
                    StartTime = period.StartTime,
                    EndTime = period.EndTime,
                    IsCurrent = period.Day == DateTime.Now.GetDayNumber() && period.StartTime <= DateTime.Now.TimeOfDay && period.EndTime >= DateTime.Now.TimeOfDay,
                    IsDeleted = period.IsDeleted
                };

                Cache.Periods.Add(dto);
            }
        }

        private async Task GetCourses()
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var offerings = await context.Set<Offering>()
                .AsNoTracking()
                .ToListAsync();

            foreach (var offering in offerings.Where(course => course.IsCurrent))
            {
                List<AdobeConnectRoomResource> resources = offering
                    .Resources
                    .Where(resource => resource.Type == ResourceType.AdobeConnectRoom)
                    .Select(resource => resource as AdobeConnectRoomResource)
                    .ToList();
                
                foreach (AdobeConnectRoomResource resource in resources)
                {
                    AdobeConnectRoom room = await context
                        .Set<AdobeConnectRoom>()
                        .FirstOrDefaultAsync(room => room.ScoId == resource.ScoId);

                    Course course = await context
                        .Set<Course>()
                        .FirstOrDefaultAsync(course => course.Id == offering.CourseId);

                    var dto = new ClassMonitorDtos.MonitorCourse
                    {
                        Id = offering.Id,
                        Name = offering.Name,
                        StartDate = offering.StartDate,
                        EndDate = offering.EndDate,
                        IsCurrent = offering.IsCurrent,
                        GradeName = $"Year {course.Grade.ToString().Substring(1, 2)}",
                        GradeShortCode = course.Grade.ToString(),
                        RoomScoId = room.ScoId,
                        RoomName = room.Name,
                        RoomUrlPath = room.UrlPath
                    };

                    List<Enrolment> enrolments = await context
                        .Set<Enrolment>()
                        .Where(enrolment => !enrolment.IsDeleted && enrolment.OfferingId == offering.Id)
                        .ToListAsync();

                    foreach (Enrolment enrol in enrolments)
                    {
                        Student student = await context
                            .Set<Student>()
                            .FirstOrDefaultAsync(student => student.StudentId == enrol.StudentId);

                        dto.Enrolments.Add(new ClassMonitorDtos.MonitorCourseEnrolment
                        {
                            Id = enrol.Id,
                            StudentId = enrol.StudentId,
                            StudentDisplayName = student.DisplayName,
                            StudentLastName = student.LastName,
                            StudentGender = student.Gender,
                            IsDeleted = enrol.IsDeleted
                        });
                    }

                    foreach (var session in offering.Sessions.Where(session => !session.IsDeleted))
                    {
                        dto.Sessions.Add(new ClassMonitorDtos.MonitorCourseSession
                        {
                            Id = session.Id,
                            PeriodId = session.PeriodId,
                            IsDeleted = session.IsDeleted
                        });
                    }

                    //foreach (var cover in course.ClassCovers)
                    //{
                    //    var entry = new ClassMonitorDtos.MonitorCourseCover
                    //    {
                    //        Id = cover.Id,
                    //        StartDate = cover.StartDate,
                    //        EndDate = cover.EndDate,
                    //        PersonId = cover.TeacherId,
                    //        IsCurrent = cover.StartDate <= DateOnly.FromDateTime(DateTime.Today) && DateOnly.FromDateTime(DateTime.Today) <= cover.EndDate
                    //    };

                    //    // TODO: retrieve teacher name and add to dto
                    //    if (cover.TeacherType == CoverTeacherType.Casual)
                    //    {
                    //        entry.PersonName = string.Empty;
                    //    }
                    //    else
                    //    {
                    //        entry.PersonName = string.Empty;
                    //    }

                    //    dto.Covers.Add(entry);
                    //}

                    List<TeacherAssignment> assignments = offering
                        .Teachers
                        .Where(assignment => 
                            assignment.Type == AssignmentType.ClassroomTeacher && 
                            !assignment.IsDeleted)
                        .ToList();

                    List<Staff> teachers = await context
                        .Set<Staff>()
                        .Where(staff => assignments.Select(assignment => assignment.StaffId).ToList().Contains(staff.StaffId))
                        .ToListAsync();

                    foreach (var teacher in teachers)
                    {
                        dto.Teachers.Add(new ClassMonitorDtos.MonitorCourseTeacher
                        {
                            Id = teacher.StaffId,
                            DisplayName = teacher.DisplayName,
                            LastName = teacher.LastName,
                            IsDeleted = teacher.IsDeleted
                        });
                    }

                    Cache.Courses.Add(dto);
                }
            }
        }
    }
}
