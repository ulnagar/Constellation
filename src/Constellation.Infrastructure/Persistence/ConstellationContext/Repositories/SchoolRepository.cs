namespace Constellation.Infrastructure.Persistence.ConstellationContext.Repositories;

using Application.Domains.Schools.Enums;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Models;
using Core.Abstractions.Clock;
using Core.Enums;
using Core.Models.StaffMembers;
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
                (enrolment.EndDate == null || enrolment.EndDate >= _dateTime.Today))
            .Select(enrolment => enrolment.SchoolCode)
            .Distinct()
            .ToListAsync(cancellationToken);

        List<string> staffSchoolCodes = await _context
            .Set<StaffMember>()
            .Where(member => !member.IsDeleted)
            .SelectMany(member => member.SchoolAssignments)
            .Where(assignment =>
                !assignment.IsDeleted &&
                assignment.StartDate <= _dateTime.Today &&
                (assignment.EndDate == null || assignment.EndDate >= _dateTime.Today))
            .Select(assignment => assignment.SchoolCode)
            .Distinct()
            .ToListAsync(cancellationToken);

        return await _context
            .Set<School>()
            .Where(school =>
                studentSchoolCodes.Contains(school.Code) ||
                staffSchoolCodes.Contains(school.Code))
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
                (enrolment.EndDate == null || enrolment.EndDate >= _dateTime.Today))
            .Select(enrolment => enrolment.SchoolCode)
            .ToListAsync(cancellationToken);

        List<string> staffSchoolCodes = await _context
            .Set<StaffMember>()
            .Where(member => !member.IsDeleted)
            .SelectMany(member => member.SchoolAssignments)
            .Where(assignment =>
                !assignment.IsDeleted &&
                assignment.StartDate <= _dateTime.Today &&
                (assignment.EndDate == null || assignment.EndDate >= _dateTime.Today))
            .Select(assignment => assignment.SchoolCode)
            .Distinct()
            .ToListAsync(cancellationToken);

        return await _context
            .Set<School>()
            .Where(school =>
                !studentSchoolCodes.Contains(school.Code) &&
                !staffSchoolCodes.Contains(school.Code))
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
                        enrolment.SchoolCode == schoolCode &&
                        !enrolment.IsDeleted &&
                        enrolment.StartDate <= _dateTime.Today &&
                        (enrolment.EndDate == null || enrolment.EndDate >= _dateTime.Today)))
            .ToListAsync(cancellationToken);
        
        if (students.All(student => student.CurrentEnrolment.Grade >= Grade.Y07))
            return SchoolType.Secondary;

        if (students.All(student => student.CurrentEnrolment.Grade <= Grade.Y06))
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
                (enrolment.EndDate == null || enrolment.EndDate >= _dateTime.Today))
            .Select(enrolment => enrolment.SchoolCode)
            .ToListAsync(cancellationToken);

        return await _context
            .Set<School>()
            .Where(school => schoolCodes.Contains(school.Code))
            .ToListAsync(cancellationToken);
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
                (enrolment.EndDate == null || enrolment.EndDate >= _dateTime.Today))
            .Select(enrolment => enrolment.SchoolCode)
            .AnyAsync(schoolCode => schoolCode == code, cancellationToken);
}