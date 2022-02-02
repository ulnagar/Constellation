﻿using Constellation.Application.DTOs;
using Constellation.Application.Interfaces.Jobs;
using Constellation.Application.Interfaces.Services;
using Constellation.Infrastructure.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Constellation.Infrastructure.Jobs
{
    public class ClassMonitorJob : IClassMonitorJob, IScopedService
    {
        private readonly SemaphoreSlim _semaphore = new(5, 5);
        private ClassMonitorDtos _cache;
        private readonly IClassMonitorCacheService _monitorCacheService;
        private readonly IAdobeConnectService _adobeConnectService;
        private readonly ILogger<IClassMonitorJob> _logger;

        public ClassMonitorJob(IClassMonitorCacheService monitorCacheService, IAdobeConnectService adobeConnectService,
            ILogger<IClassMonitorJob> logger)
        {
            _monitorCacheService = monitorCacheService;
            _adobeConnectService = adobeConnectService;
            _logger = logger;
        }

        public async Task StartJob()
        {
            var scanTime = DateTime.Now;
            _logger.LogInformation($"Starting room monitor scan at {scanTime}");

            _cache = await _monitorCacheService.GetData();
            _logger.LogInformation($"Found {_cache.Courses.Count} rooms to check");

            var taskList = new List<Task>();
            foreach (var course in _cache.Courses)
            {
                taskList.Add(ScanRoom(course));
            }

            await Task.WhenAll(taskList);

            _logger.LogInformation($"Scanning of rooms complete.");

            _monitorCacheService.UpdateScan(_cache.Courses);

            _logger.LogInformation($"Stopping room monitor scan at {DateTime.Now}");
        }

        private async Task<ClassMonitorDtos.MonitorCourse> ScanRoom(ClassMonitorDtos.MonitorCourse listCourse)
        {
            await _semaphore.WaitAsync();

            var course = listCourse;

            _logger.LogInformation($"{course.Name} - Starting scan");

            course.LastScanTime = DateTime.Now;

            var periods = _cache.Periods;
            if (periods == null)
            {
                _semaphore.Release();
                return course;
            }

            var currentPeriods = periods.Where(period => period.IsCurrent).ToList();

            if (course.Sessions.Any(session => currentPeriods.Select(period => period.Id).Distinct().Contains(session.PeriodId) && !session.IsDeleted))
                course.StatusCode += ClassMonitorDtos.MonitorCourse.Current;

            if (course.Covers.Any(cover => cover.IsCurrent))
                course.StatusCode += ClassMonitorDtos.MonitorCourse.Covered;

            var assetId = await _adobeConnectService.GetCurrentSessionAsync(course.RoomScoId);

            if (string.IsNullOrWhiteSpace(assetId))
            {
                _logger.LogInformation($"{course.Name} - Stopping scan - No current session detected");

                _semaphore.Release();

                return course;
            }

            var attendees = await _adobeConnectService.GetCurrentSessionUsersAsync(course.RoomScoId, assetId);

            if (attendees == null)
            {
                _logger.LogInformation($"{course.Name} - Stopping scan - No attendees detected");

                _semaphore.Release();

                return course;
            }

            foreach (var attendee in attendees)
            {
                IdentifyUser(course, attendee);
            }

            if ((course.Enrolments.Count(enrol => enrol.IsPresent) + course.OtherAttendees.Count + course.Guests) > 0)
                course.StatusCode += ClassMonitorDtos.MonitorCourse.StudentsPresent;

            if ((course.Teachers.Count(teacher => teacher.IsPresent) + course.OtherStaff.Count) > 0)
                course.StatusCode += ClassMonitorDtos.MonitorCourse.TeachersPresent;

            _logger.LogInformation($"{course.Name} - Stopping scan");

            _semaphore.Release();

            return course;
        }

        private void IdentifyUser(ClassMonitorDtos.MonitorCourse course, string attendeeId)
        {
            if (_cache.Users == null)
                return;

            var users = _cache.Users.Where(u => !string.IsNullOrWhiteSpace(u.UserPrincipalId)).ToList();

            var user = users.FirstOrDefault(item => item.UserPrincipalId == attendeeId);
            if (user == null)
            {
                course.Guests++;
                return;
            }

            switch (user.UserType)
            {
                case "Student":
                    var student = course.Enrolments.FirstOrDefault(enrol => enrol.StudentId == user.Id);
                    if (student != null)
                    {
                        _logger.LogInformation($"{course.Name} - Detected enrolled student - {student.StudentDisplayName}");

                        student.IsPresent = true;
                    }
                    else
                    {
                        _logger.LogInformation($"{course.Name} - Detected non-enrolled student - {user.DisplayName}");

                        course.OtherAttendees.Add(new ClassMonitorDtos.MonitorCourseEnrolment
                        {
                            StudentDisplayName = user.DisplayName,
                            StudentGender = user.Gender,
                            StudentId = user.Id,
                            IsDeleted = user.IsDeleted,
                            IsPresent = true
                        });
                    }

                    break;
                case "Staff":
                    var staff = course.Teachers.FirstOrDefault(teacher => teacher.Id == user.Id);
                    if (staff != null)
                    {
                        _logger.LogInformation($"{course.Name} - Detected linked teacher - {staff.DisplayName}");

                        staff.IsPresent = true;
                    }
                    else
                    {
                        _logger.LogInformation($"{course.Name} - Detected non-linked teacher - {user.DisplayName}");

                        course.OtherStaff.Add(new ClassMonitorDtos.MonitorCourseTeacher
                        {
                            Id = user.Id,
                            DisplayName = user.DisplayName,
                            IsDeleted = user.IsDeleted,
                            IsPresent = true
                        });
                    }

                    break;
                case "Casual":
                    _logger.LogInformation($"{course.Name} - Detected casual teacher - {user.DisplayName}");

                    course.OtherStaff.Add(new ClassMonitorDtos.MonitorCourseTeacher
                    {
                        Id = user.Id,
                        DisplayName = user.DisplayName,
                        IsDeleted = user.IsDeleted,
                        IsPresent = true
                    });
                    break;
            }
        }
    }
}