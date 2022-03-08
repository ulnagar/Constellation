using Constellation.Application.DTOs;
using Constellation.Application.Extensions;
using Constellation.Application.Interfaces.Services;
using Constellation.Core.Comparators;
using Constellation.Core.Models;
using Constellation.Infrastructure.DependencyInjection;
using Constellation.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

            var casuals = await context.Casuals
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
                    UserPrincipalId = casual.AdobeConnectPrincipalId,
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

            var courses = await context.Offerings
                .Include(course => course.Enrolments)
                .ThenInclude(enrol => enrol.Student)
                .Include(course => course.Sessions)
                .ThenInclude(session => session.Room)
                .Include(course => course.Sessions)
                .ThenInclude(session => session.Teacher)
                .Include(course => course.Course)
                .AsNoTracking()
                .ToListAsync();

            foreach (var course in courses.Where(course => course.IsCurrent()))
            {
                var rooms = course.Sessions.Where(session => !session.IsDeleted).Select(session => session.Room).Distinct(new AdobeConnectRoomComparator()).ToList();
                foreach (var room in rooms)
                {
                    var dto = new ClassMonitorDtos.MonitorCourse
                    {
                        Id = course.Id,
                        Name = course.Name,
                        StartDate = course.StartDate,
                        EndDate = course.EndDate,
                        IsCurrent = course.IsCurrent(),
                        GradeName = $"Year {course.Course.Grade.ToString().Substring(1, 2)}",
                        GradeShortCode = course.Course.Grade.ToString(),
                        RoomScoId = room.ScoId,
                        RoomName = room.Name,
                        RoomUrlPath = room.UrlPath
                    };

                    foreach (var enrol in course.Enrolments.Where(enrolment => !enrolment.IsDeleted))
                    {
                        dto.Enrolments.Add(new ClassMonitorDtos.MonitorCourseEnrolment
                        {
                            Id = enrol.Id,
                            StudentId = enrol.StudentId,
                            StudentDisplayName = enrol.Student.DisplayName,
                            StudentLastName = enrol.Student.LastName,
                            StudentGender = enrol.Student.Gender,
                            IsDeleted = enrol.IsDeleted
                        });
                    }

                    foreach (var session in course.Sessions.Where(session => !session.IsDeleted))
                    {
                        dto.Sessions.Add(new ClassMonitorDtos.MonitorCourseSession
                        {
                            Id = session.Id,
                            PeriodId = session.PeriodId,
                            IsDeleted = session.IsDeleted
                        });
                    }

                    foreach (var cover in course.ClassCovers)
                    {
                        switch (cover)
                        {
                            case CasualClassCover cCover:
                                dto.Covers.Add(new ClassMonitorDtos.MonitorCourseCover
                                {
                                    Id = cCover.Id,
                                    StartDate = cCover.StartDate,
                                    EndDate = cCover.EndDate,
                                    PersonId = cCover.CasualId.ToString(),
                                    PersonName = cCover.Casual.DisplayName,
                                    IsCurrent = cCover.StartDate <= DateTime.Today && DateTime.Today <= cCover.EndDate
                                });
                                break;
                            case TeacherClassCover sCover:
                                dto.Covers.Add(new ClassMonitorDtos.MonitorCourseCover
                                {
                                    Id = sCover.Id,
                                    StartDate = sCover.StartDate,
                                    EndDate = sCover.EndDate,
                                    PersonId = sCover.StaffId,
                                    PersonName = sCover.Staff.DisplayName
                                });
                                break;
                        }
                    }

                    foreach (var teacher in course.Sessions.Where(session => !session.IsDeleted).Select(session => session.Teacher).Distinct(new StaffComparator()).ToList())
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
