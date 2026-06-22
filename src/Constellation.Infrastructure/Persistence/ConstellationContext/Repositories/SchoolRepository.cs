namespace Constellation.Infrastructure.Persistence.ConstellationContext.Repositories;

using Application.Domains.Schools.Enums;
using Constellation.Application.Interfaces.Repositories;
using Core.Abstractions.Clock;
using Core.Enums;
using Core.Models;
using Core.Models.Identifiers;
using Core.Models.StaffMembers;
using Core.Models.Students;
using Microsoft.EntityFrameworkCore;

public class SchoolRepository : ISchoolRepository
{
    private readonly ConstellationDbContext _context;
    private readonly IDateTimeProvider _dateTime;

    public SchoolRepository(
        ConstellationDbContext context,
        IDateTimeProvider dateTime)
    {
        _context = context;
        _dateTime = dateTime;
    }

    public async Task<List<School>> GetAllActive(
        CancellationToken cancellationToken = default)
    {
        List<SchoolCode> studentSchoolCodes = await _context
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

        List<SchoolCode> staffSchoolCodes = await _context
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
        List<SchoolCode> studentSchoolCodes = await _context
            .Set<Student>()
            .Where(student => !student.IsDeleted)
            .SelectMany(student => student.SchoolEnrolments)
            .Where(enrolment =>
                !enrolment.IsDeleted &&
                enrolment.StartDate <= _dateTime.Today &&
                (enrolment.EndDate == null || enrolment.EndDate >= _dateTime.Today))
            .Select(enrolment => enrolment.SchoolCode)
            .ToListAsync(cancellationToken);

        List<SchoolCode> staffSchoolCodes = await _context
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
        List<SchoolCode> schoolCodes, 
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<School>()
            .Where(school => schoolCodes.Contains(school.Code))
            .ToListAsync(cancellationToken);

    public async Task<SchoolType> GetSchoolType(
        SchoolCode schoolCode,
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
        
        if (students.All(student => student.CurrentEnrolment?.Grade >= Grade.Y07))
            return SchoolType.Secondary;

        if (students.All(student => student.CurrentEnrolment?.Grade <= Grade.Y06))
            return SchoolType.Primary;

        return SchoolType.Central;
    }

    public void Insert(School school) =>
        _context.Set<School>().Add(school);

    public async Task<School?> GetById(
        SchoolCode schoolCode,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<School>()
            .FirstOrDefaultAsync(school => school.Code == schoolCode, cancellationToken);

    public async Task<List<School>> GetAll(
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<School>()
            .ToListAsync(cancellationToken);

    public async Task<List<School>> GetWithCurrentStudents(
        CancellationToken cancellationToken = default)
    {
        List<SchoolCode> schoolCodes = await _context
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
        SchoolCode schoolCode,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Student>()
            .Where(student => !student.IsDeleted)
            .SelectMany(student => student.SchoolEnrolments)
            .AnyAsync(enrolment =>
                !enrolment.IsDeleted &&
                enrolment.StartDate <= _dateTime.Today &&
                (enrolment.EndDate == null || enrolment.EndDate >= _dateTime.Today) &&
                enrolment.SchoolCode == schoolCode,
                cancellationToken);
}