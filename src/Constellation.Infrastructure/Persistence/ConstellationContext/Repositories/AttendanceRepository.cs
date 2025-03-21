﻿namespace Constellation.Infrastructure.Persistence.ConstellationContext.Repositories;

using Constellation.Core.Models.Attendance.Identifiers;
using Core.Abstractions.Clock;
using Core.Enums;
using Core.Models.Attendance;
using Core.Models.Attendance.Repositories;
using Core.Models.Students.Identifiers;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

internal class AttendanceRepository : IAttendanceRepository
{
    private readonly AppDbContext _context;
    private readonly IDateTimeProvider _dateTime;

    public AttendanceRepository(
        AppDbContext context,
        IDateTimeProvider dateTime)
    {
        _context = context;
        _dateTime = dateTime;
    }

    public async Task<AttendanceValue> GetById(
        AttendanceValueId id,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<AttendanceValue>()
            .SingleOrDefaultAsync(value => value.Id == id, cancellationToken);

    public async Task<List<AttendanceValue>> GetAll(
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<AttendanceValue>()
            .ToListAsync(cancellationToken);

    public async Task<List<AttendanceValue>> GetAllRecent(
        CancellationToken cancellationToken = default)
    {
        List<DateOnly> listOfStartDates = await _context
            .Set<AttendanceValue>()
            .Where(entry => entry.StartDate.Year == _dateTime.CurrentYear)
            .Select(entry => entry.StartDate)
            .Distinct()
            .OrderByDescending(entry => entry)
            .Take(5)
            .ToListAsync(cancellationToken);

        return await _context
            .Set<AttendanceValue>()
            .Where(entry => listOfStartDates.Contains(entry.StartDate))
            .ToListAsync(cancellationToken);
    }

    public async Task<List<AttendanceValue>> GetAllForStudent(
        int year,
        StudentId studentId,
        CancellationToken cancellationToken = default)
    {
        DateOnly startOfYear = _dateTime.GetFirstDayOfYear(year);
        DateOnly endOfYear = _dateTime.GetLastDayOfYear(year);

        return await _context
            .Set<AttendanceValue>()
            .Where(entry =>
                entry.StudentId == studentId &&
                startOfYear <= entry.StartDate &&
                endOfYear >= entry.EndDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<AttendanceValue> GetLatestForStudent(
        StudentId studentId,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<AttendanceValue>()
            .Where(entry => entry.StudentId == studentId)
            .OrderByDescending(entry => entry.EndDate)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<List<AttendanceValue>> GetAllForDate(
        DateOnly selectedDate,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<AttendanceValue>()
            .Where(entry => 
                entry.StartDate <= selectedDate &&
                entry.EndDate >= selectedDate)
            .ToListAsync(cancellationToken);

    public async Task<List<AttendanceValue>> GetAllForStudentAndDate(
        StudentId studentId,
        DateOnly selectedDate,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<AttendanceValue>()
            .Where(entry =>
                entry.StudentId == studentId &&
                entry.StartDate <= selectedDate &&
                entry.EndDate >= selectedDate)
            .ToListAsync(cancellationToken);

    public async Task<List<AttendanceValue>> GetAllForGrade(
        int year, 
        Grade selectedGrade, 
        CancellationToken cancellationToken = default)
    {
        DateOnly startOfYear = _dateTime.GetFirstDayOfYear(year);
        DateOnly endOfYear = _dateTime.GetLastDayOfYear(year);

        return await _context
            .Set<AttendanceValue>()
            .Where(entry =>
                startOfYear <= entry.StartDate &&
                endOfYear >= entry.EndDate &&
                entry.Grade == selectedGrade)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<AttendanceValue>> GetAllForGradeAndDate(
        Grade selectedGrade, 
        DateOnly selectedDate, 
        CancellationToken cancellationToken = default) => 
        await _context
            .Set<AttendanceValue>()
            .Where(entry => 
                entry.Grade == selectedGrade &&
                entry.StartDate <= selectedDate &&
                entry.EndDate >= selectedDate)
            .ToListAsync(cancellationToken);

    public async Task<List<AttendanceValue>> GetForReportWithTitle(
        string periodLabel, 
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<AttendanceValue>()
            .Where(entry => entry.PeriodLabel == periodLabel)
            .ToListAsync(cancellationToken);


    public async Task<List<DateOnly>> GetPeriodEndDates(
        int year,
        CancellationToken cancellationToken = default)
    {
        DateOnly startOfYear = _dateTime.GetFirstDayOfYear(year);
        DateOnly endOfYear = _dateTime.GetLastDayOfYear(year);

        return await _context
            .Set<AttendanceValue>()
            .Where(entry => 
                startOfYear <= entry.StartDate &&
                endOfYear >= entry.EndDate)
            .Select(entry => entry.EndDate)
            .Distinct()
            .OrderBy(entry => entry)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<string>> GetPeriodNames(
        int year,
        CancellationToken cancellationToken = default)
    { 
        DateOnly startOfYear = _dateTime.GetFirstDayOfYear(year);
        DateOnly endOfYear = _dateTime.GetLastDayOfYear(year);

        return await _context
            .Set<AttendanceValue>()
            .Where(entry =>
                startOfYear <= entry.StartDate &&
                endOfYear >= entry.EndDate)
            .Select(entry => new { entry.PeriodLabel, entry.EndDate })
            .Distinct()
            .OrderBy(entry => entry.EndDate)
            .Select(entry => entry.PeriodLabel)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<AttendanceValue>> GetForStudentBetweenDates(
        StudentId studentId, 
        DateOnly earlierEndDate, 
        DateOnly laterEndDate, 
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<AttendanceValue>()
            .Where(entry => entry.StudentId == studentId)
            .Where(entry => 
                entry.EndDate >= earlierEndDate &&
                entry.EndDate <= laterEndDate)
            .OrderBy(entry => entry.EndDate)
            .ToListAsync(cancellationToken);

    public void Insert(AttendanceValue item)
    {
        bool existing = _context
            .Set<AttendanceValue>()
            .Any(entry =>
                entry.StudentId == item.StudentId &&
                entry.PeriodLabel == item.PeriodLabel);

        if (existing)
        {
            AttendanceValue entry = _context
                .Set<AttendanceValue>()
                .First(entry =>
                    entry.StudentId == item.StudentId &&
                    entry.PeriodLabel == item.PeriodLabel);

            _context
                .Set<AttendanceValue>()
                .Remove(entry);
        }

        _context.Set<AttendanceValue>().Add(item);
    } 

    public void Insert(List<AttendanceValue> items) => _context.Set<AttendanceValue>().AddRange(items);
}
