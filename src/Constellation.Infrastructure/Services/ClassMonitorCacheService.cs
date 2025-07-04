namespace Constellation.Infrastructure.Services;

using Constellation.Application.DTOs;
using Constellation.Application.Extensions;
using Constellation.Application.Interfaces.Services;
using Constellation.Core.Models.Casuals;
using Constellation.Infrastructure.DependencyInjection;
using Constellation.Infrastructure.Persistence.ConstellationContext;
using Core.Models;
using Core.Models.StaffMembers;
using Core.Models.Students;
using Core.Models.Timetables;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

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

        var students = await context
            .Set<Student>()
            .Where(student => !student.IsDeleted)
            .AsNoTracking()
            .ToListAsync();

        var staff = await context
            .Set<StaffMember>()
            .Where(teacher => !teacher.IsDeleted)
            .AsNoTracking()
            .ToListAsync();

        var casuals = await context
            .Set<Casual>()
            .Where(casual => !casual.IsDeleted)
            .AsNoTracking()
            .ToListAsync();

        foreach (var student in students)
        {
            //var dto = new ClassMonitorDtos.MonitorUser
            //{
            //    Id = student.StudentId,
            //    DisplayName = student.DisplayName,
            //    Gender = student.Gender,
            //    UserType = "Student",
            //    UserPrincipalId = student.AdobeConnectPrincipalId,
            //    IsDeleted = student.IsDeleted
            //};

            //Cache.Users.Add(dto);
        }

        foreach (var teacher in staff)
        {
            //var dto = new ClassMonitorDtos.MonitorUser
            //{
            //    Id = teacher.Id.ToString(),
            //    DisplayName = teacher.Name.DisplayName,
            //    UserType = "Staff",
            //    UserPrincipalId = teacher.AdobeConnectPrincipalId,
            //    IsDeleted = teacher.IsDeleted
            //};

            //Cache.Users.Add(dto);
        }

        foreach (var casual in casuals)
        {
            //var dto = new ClassMonitorDtos.MonitorUser
            //{
            //    Id = casual.Id.ToString(),
            //    DisplayName = casual.Name.DisplayName,
            //    UserType = "Casual",
            //    UserPrincipalId = casual.AdobeConnectId,
            //    IsDeleted = casual.IsDeleted
            //};

            //Cache.Users.Add(dto);
        }
    }

    private async Task GetPeriods()
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var periods = await context
            .Set<Period>()
            .AsNoTracking()
            .ToListAsync();

        foreach (var period in periods)
        {
            var dto = new ClassMonitorDtos.MonitorPeriod
            {
                Id = period.Id,
                Name = period.Name,
                Timetable = period.Timetable,
                Day = period.DayNumber,
                Type = period.Type,
                StartTime = period.StartTime,
                EndTime = period.EndTime,
                IsCurrent = period.DayNumber == DateTime.Now.GetDayNumber() && period.StartTime <= DateTime.Now.TimeOfDay && period.EndTime >= DateTime.Now.TimeOfDay,
                IsDeleted = period.IsDeleted
            };

            Cache.Periods.Add(dto);
        }
    }

    private async Task GetCourses()
    {

    }
}