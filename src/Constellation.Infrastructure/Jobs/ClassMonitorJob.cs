namespace Constellation.Infrastructure.Jobs;

using Constellation.Application.DTOs;
using Constellation.Application.Interfaces.Jobs;
using Constellation.Application.Interfaces.Services;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class ClassMonitorJob : IClassMonitorJob
{
    private readonly SemaphoreSlim _semaphore = new(5, 5);
    private ClassMonitorDtos _cache;
    private readonly IClassMonitorCacheService _monitorCacheService;
    private readonly IAdobeConnectService _adobeConnectService;
    private readonly ILogger _logger;

    private Guid JobId { get; set; }

    public ClassMonitorJob(
        IClassMonitorCacheService monitorCacheService, 
        IAdobeConnectService adobeConnectService,
        ILogger logger)
    {
        _monitorCacheService = monitorCacheService;
        _adobeConnectService = adobeConnectService;
        _logger = logger.ForContext<IClassMonitorJob>();
    }

    public async Task StartJob(Guid jobId, CancellationToken token)
    { 
        JobId = jobId;

        DateTime scanTime = DateTime.Now;
        _logger
            .Information("{id}: Starting room monitor scan at {scanTime}", jobId, scanTime);

        if (token.IsCancellationRequested)
            return;

        _cache = await _monitorCacheService.GetData();
        _logger
            .Information("{id}: Found {courses} rooms to check", jobId, _cache.Courses.Count);

        List<Task> taskList = new();
        foreach (ClassMonitorDtos.MonitorCourse course in _cache.Courses)
        {
            if (token.IsCancellationRequested)
                return;

            taskList.Add(ScanRoom(course, token));
        }

        if (token.IsCancellationRequested)
            return;

        await Task.WhenAll(taskList);

        _logger
            .Information("{id}: Scanning of rooms complete.", jobId);

        if (token.IsCancellationRequested)
            return;

        _monitorCacheService.UpdateScan(_cache.Courses);

        _logger
            .Information("{id}: Stopping room monitor scan at {time}", jobId, DateTime.Now);
    }

    private async Task<ClassMonitorDtos.MonitorCourse> ScanRoom(ClassMonitorDtos.MonitorCourse listCourse, CancellationToken token)
    {
        await _semaphore.WaitAsync(token);

        if (token.IsCancellationRequested)
            return listCourse;

        ClassMonitorDtos.MonitorCourse course = listCourse;

        _logger
            .Information("{id}: {course} - Starting scan", JobId, course.Name);
        
        course.LastScanTime = DateTime.Now;

        ICollection<ClassMonitorDtos.MonitorPeriod> periods = _cache.Periods;
        if (periods == null)
        {
            _semaphore.Release();
            return course;
        }

        List<ClassMonitorDtos.MonitorPeriod> currentPeriods = periods
            .Where(period => period.IsCurrent)
            .ToList();

        if (course
                .Sessions
                .Any(session => 
                    currentPeriods
                        .Select(period => period.Id)
                        .Distinct()
                        .Contains(session.PeriodId) &&
                    !session.IsDeleted))
            course.StatusCode += ClassMonitorDtos.MonitorCourse.Current;

        if (course
                .Covers
                .Any(cover => cover.IsCurrent))
            course.StatusCode += ClassMonitorDtos.MonitorCourse.Covered;

        if (token.IsCancellationRequested)
        {
            _semaphore.Release();
            return course;
        }

        string assetId = await _adobeConnectService
            .GetCurrentSessionAsync(course.RoomScoId);

        if (string.IsNullOrWhiteSpace(assetId))
        {
            _logger
                .Information("{id}: {course} - Stopping scan - No current session detected", JobId, course.Name);

            _semaphore.Release();

            return course;
        }

        if (token.IsCancellationRequested)
        {
            _semaphore.Release();
            return course;
        }

        ICollection<string> attendees = await _adobeConnectService
            .GetCurrentSessionUsersAsync(course.RoomScoId, assetId);

        if (attendees == null)
        {
            _logger
                .Information("{id}: {course} - Stopping scan - No attendees detected", JobId, course.Name);

            _semaphore.Release();

            return course;
        }

        foreach (string attendee in attendees)
        {
            if (token.IsCancellationRequested)
            {
                _semaphore.Release();
                return course;
            }

            IdentifyUser(course, attendee);
        }

        if ((course.Enrolments.Count(enrol => enrol.IsPresent) + course.OtherAttendees.Count + course.Guests) > 0)
            course.StatusCode += ClassMonitorDtos.MonitorCourse.StudentsPresent;

        if ((course.Teachers.Count(teacher => teacher.IsPresent) + course.OtherStaff.Count) > 0)
            course.StatusCode += ClassMonitorDtos.MonitorCourse.TeachersPresent;

        _logger
            .Information("{id}: {course} - Stopping scan", JobId, course.Name);

        _semaphore.Release();

        return course;
    }

    private void IdentifyUser(ClassMonitorDtos.MonitorCourse course, string attendeeId)
    {
        if (_cache.Users == null)
            return;

        List<ClassMonitorDtos.MonitorUser> users = _cache.Users.Where(u => !string.IsNullOrWhiteSpace(u.UserPrincipalId)).ToList();

        ClassMonitorDtos.MonitorUser user = users.FirstOrDefault(item => item.UserPrincipalId == attendeeId);
        if (user == null)
        {
            course.Guests++;
            return;
        }

        switch (user.UserType)
        {
            case "Student":
                ClassMonitorDtos.MonitorCourseEnrolment student = course.Enrolments.FirstOrDefault(enrol => enrol.StudentId == user.Id);
                if (student != null)
                {
                    _logger.Information("{id}: {course} - Detected enrolled student - {student}", JobId, course.Name, student.StudentDisplayName);

                    student.IsPresent = true;
                }
                else
                {
                    _logger.Information("{id}: {course} - Detected non-enrolled student - {user}", JobId, course.Name, user.DisplayName);

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
                ClassMonitorDtos.MonitorCourseTeacher staff = course.Teachers.FirstOrDefault(teacher => teacher.Id == user.Id);
                if (staff != null)
                {
                    _logger
                        .Information("{id}: {course} - Detected linked teacher - {staff}", JobId, course.Name, staff.DisplayName);

                    staff.IsPresent = true;
                }
                else
                {
                    _logger
                        .Information("{id}: {course} - Detected non-linked teacher - {user}", JobId, course.Name, user.DisplayName);

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
                _logger
                    .Information("{id}: {course} - Detected casual teacher - {user}", JobId, course.Name, user.DisplayName);

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