namespace Constellation.Infrastructure.Persistence.ConstellationContext.Repositories;

using Application.Schools.Enums;
using Constellation.Application.DTOs;
using Constellation.Application.Helpers;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Models;
using Core.Abstractions.Clock;
using Core.Enums;
using Core.Models.Students;
using Microsoft.EntityFrameworkCore;

public class SchoolRepository : ISchoolRepository
{
    private readonly AppDbContext _context;
    private readonly IDateTimeProvider _dateTime;

    public SchoolRepository(
        AppDbContext context,
        IDateTimeProvider dateTime)
    {
        _context = context;
        _dateTime = dateTime;
    }

    public async Task<List<School>> GetAllActive(
        CancellationToken cancellationToken = default)
    {
        List<string> studentSchoolCodes = await _context
            .Set<Student>()
            .Where(student => !student.IsDeleted)
            .SelectMany(student => student.SchoolEnrolments)
            .Where(enrolment =>
                !enrolment.IsDeleted &&
                enrolment.StartDate <= _dateTime.Today &&
                enrolment.EndDate >= _dateTime.Today)
            .Select(enrolment => enrolment.SchoolCode)
            .ToListAsync(cancellationToken);
        
        return await _context
            .Set<School>()
            .Where(school =>
                studentSchoolCodes.Contains(school.Code) &&
                school.Staff.Any(staff => !staff.IsDeleted))
            .ToListAsync(cancellationToken);
    }

    public async Task<List<School>> GetAllInactive(
        CancellationToken cancellationToken = default)
    {
        List<string> studentSchoolCodes = await _context
            .Set<Student>()
            .Where(student => !student.IsDeleted)
            .SelectMany(student => student.SchoolEnrolments)
            .Where(enrolment =>
                !enrolment.IsDeleted &&
                enrolment.StartDate <= _dateTime.Today &&
                enrolment.EndDate >= _dateTime.Today)
            .Select(enrolment => enrolment.SchoolCode)
            .ToListAsync(cancellationToken);
        
        return await _context
            .Set<School>()
            .Where(school =>
                !studentSchoolCodes.Contains(school.Code) &&
                school.Staff.All(staff => staff.IsDeleted))
            .ToListAsync(cancellationToken);
    }

    public async Task<List<School>> GetListFromIds(
        List<string> schoolCodes, 
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<School>()
            .Where(school => schoolCodes.Contains(school.Code))
            .ToListAsync(cancellationToken);

    public async Task<SchoolType> GetSchoolType(
        string schoolCode,
        CancellationToken cancellationToken = default)
    {
        List<Student> students = await _context
            .Set<Student>()
            .Where(student => 
                !student.IsDeleted &&
                student.SchoolEnrolments
                    .Any(enrolment =>
                        !enrolment.IsDeleted &&
                        enrolment.StartDate <= _dateTime.Today &&
                        enrolment.EndDate >= _dateTime.Today))
            .ToListAsync(cancellationToken);
        
        if (students.All(student => student.CurrentEnrolment?.Grade >= Grade.Y07))
            return SchoolType.Secondary;

        if (students.All(student => student.CurrentEnrolment?.Grade <= Grade.Y06))
            return SchoolType.Primary;

        return SchoolType.Central;
    }

    public void Insert(School school) =>
        _context.Set<School>().Add(school);

    public async Task<School?> GetById(
        string id,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<School>()
            .FirstOrDefaultAsync(school => school.Code == id, cancellationToken);

    public async Task<List<School>> GetAll(
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<School>()
            .ToListAsync(cancellationToken);

    public async Task<List<School>> GetWithCurrentStudents(
        CancellationToken cancellationToken = default)
    {
        List<string> schoolCodes = await _context
            .Set<Student>()
            .Where(student => !student.IsDeleted)
            .SelectMany(student => student.SchoolEnrolments)
            .Where(enrolment =>
                !enrolment.IsDeleted &&
                enrolment.StartDate <= _dateTime.Today &&
                enrolment.EndDate >= _dateTime.Today)
            .Select(enrolment => enrolment.SchoolCode)
            .ToListAsync(cancellationToken);

        return await _context
            .Set<School>()
            .Where(school => schoolCodes.Contains(school.Code))
            .ToListAsync(cancellationToken);
    }
        

    public async Task<School> ForEditAsync(string id)
    {
        return await _context.Schools
            .SingleOrDefaultAsync(school => school.Code == id);
    }

    public async Task<bool> IsPartnerSchoolWithStudents(
        string code,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Student>()
            .Where(student => !student.IsDeleted)
            .SelectMany(student => student.SchoolEnrolments)
            .Where(enrolment =>
                !enrolment.IsDeleted &&
                enrolment.StartDate <= _dateTime.Today &&
                enrolment.EndDate >= _dateTime.Today)
            .Select(enrolment => enrolment.SchoolCode)
            .AnyAsync(schoolCode => schoolCode == code, cancellationToken);

    public async Task<bool> AnyWithId(string id)
    {
        return await _context.Schools
            .AnyAsync(school => school.Code == id);
    }

    public IList<MapLayer> GetForMapping(IList<string> schoolCodes)
    {
        var vm = new List<MapLayer>();

        var schools = _context.Schools
            .Include(school => school.Staff);

        List<string> studentSchoolCodes = _context
            .Set<Student>()
            .Where(student => !student.IsDeleted)
            .SelectMany(student => student.SchoolEnrolments)
            .Where(enrolment =>
                !enrolment.IsDeleted &&
                enrolment.StartDate <= _dateTime.Today &&
                enrolment.EndDate >= _dateTime.Today)
            .Select(enrolment => enrolment.SchoolCode)
            .ToList();
        
        var filteredSchools = schoolCodes.Count > 0
            ? schools.Where(school => schoolCodes.Any(code => code == school.Code))
            : schools;

        vm.Add(MapHelpers.MapLayerBuilder(
            filteredSchools.Where(school =>
                studentSchoolCodes.Contains(school.Code) &&
                school.Staff.All(staff => staff.IsDeleted)),
            "Students only",
            "blue"));

        vm.Add(MapHelpers.MapLayerBuilder(
            filteredSchools.Where(school =>
                !studentSchoolCodes.Contains(school.Code) &&
                school.Staff.Any(staff => !staff.IsDeleted)),
            "Staff only",
            "red"));

        vm.Add(MapHelpers.MapLayerBuilder(
            filteredSchools.Where(school =>
                studentSchoolCodes.Contains(school.Code) &&
                school.Staff.Any(staff => !staff.IsDeleted)),
            "Both Students and Staff",
            "green"));

        return vm;
    }
}