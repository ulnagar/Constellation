namespace Constellation.Infrastructure.Persistence.ConstellationContext.Repositories;

using Constellation.Application.DTOs;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Enums;
using Constellation.Core.Models;
using LinqKit;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

public class StudentRepository : IStudentRepository
{
    private readonly AppDbContext _context;

    public StudentRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Student?> GetById(
        string StudentId,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Student>()
            .Where(student => student.StudentId == StudentId)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<Student?> GetWithSchoolById(
        string studentId,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Student>()
            .Include(student => student.School)
            .Where(student => student.StudentId == studentId)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<List<Student>> GetListFromIds(
        List<string> studentIds,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Student>()
            .Where(student => studentIds.Contains(student.StudentId))
            .ToListAsync(cancellationToken);

    public async Task<List<Student>> GetCurrentEnrolmentsForOffering(
        int offeringId,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Student>()
            .Where(student => student.Enrolments.Any(enrolment => enrolment.OfferingId == offeringId && !enrolment.IsDeleted))
            .ToListAsync(cancellationToken);

    public async Task<List<Student>> GetCurrentEnrolmentsForOfferingWithSchool(
        int offeringId,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Student>()
            .Include(student => student.School)
            .Where(student => student.Enrolments.Any(enrolment => enrolment.OfferingId == offeringId && !enrolment.IsDeleted))
            .ToListAsync(cancellationToken);

    public async Task<List<Student>> GetCurrentStudentsWithSchool(
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Student>()
            .Include(student => student.School)
            .Where(student => !student.IsDeleted)
            .ToListAsync(cancellationToken);

    public async Task<List<Student>> GetCurrentStudentsWithFamilyMemberships(
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Student>()
            .Include(student => student.FamilyMemberships)
            .Where(student => !student.IsDeleted)
            .ToListAsync(cancellationToken);

    public async Task<bool> IsValidStudentId(
        string studentId, 
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Student>()
            .AnyAsync(student => student.StudentId == studentId && !student.IsDeleted, cancellationToken);

    public async Task<List<Student>> GetFilteredStudents(
        List<int> OfferingIds,
        List<Grade> Grades,
        List<string> SchoolCodes,
        CancellationToken cancellationToken = default)
    {
        var students = _context
            .Set<Student>()
            .Include(student => student.School)
            .Where(student => !student.IsDeleted);

        if (OfferingIds.Count > 0)
        {
            students = students
                .Where(student => student.Enrolments.Any(enrol => 
                    !enrol.IsDeleted && 
                    OfferingIds.Contains(enrol.OfferingId)));
        }

        if (Grades.Count > 0)
        {
            students = students
                .Where(student => Grades.Contains(student.CurrentGrade));
        }

        if (SchoolCodes.Count > 0)
        {
            students = students
                .Where(student => SchoolCodes.Contains(student.SchoolCode));
        }

        return await students.ToListAsync(cancellationToken);
    }
        
    public async Task<List<Student>> GetCurrentStudentsFromSchool(
        string SchoolCode,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Student>()
            .Where(student => 
                student.SchoolCode == SchoolCode &&
                student.IsDeleted == false)
            .ToListAsync(cancellationToken);

    public async Task<List<Student>> GetCurrentStudentFromGrade(
        Grade grade, 
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Student>()
            .Where(student => 
                student.IsDeleted == false &&
                student.CurrentGrade == grade)
            .ToListAsync(cancellationToken);

    public async Task<Student> ForDetailDisplayAsync(string id)
    {
        return await _context.Students
            .Include(student => student.School)
            .Include(student => student.Absences)
            .ThenInclude(absence => absence.Notifications)
            .Include(student => student.Absences)
            .ThenInclude(absence => absence.Responses)
            .Include(student => student.Absences)
            .ThenInclude(absence => absence.Offering)
            .Include(student => student.Devices)
            .ThenInclude(alloc => alloc.Device)
            .Include(student => student.Enrolments)
            .ThenInclude(enrol => enrol.Offering)
            .ThenInclude(offering => offering.Course)
            .Include(student => student.Enrolments)
            .ThenInclude(enrol => enrol.Offering)
            .ThenInclude(offering => offering.Sessions)
            .ThenInclude(session => session.Teacher)
            .Include(student => student.Enrolments)
            .ThenInclude(enrol => enrol.Offering)
            .ThenInclude(offering => offering.Sessions)
            .ThenInclude(session => session.Period)
            .Include(student => student.Enrolments)
            .ThenInclude(enrol => enrol.Offering)
            .ThenInclude(offering => offering.Sessions)
            .ThenInclude(session => session.Room)
            .Include(student => student.FamilyMemberships)
            .SingleOrDefaultAsync(student => student.StudentId == id);
    }

    public async Task<Student> GetForExistCheck(string id)
    {
        return await _context.Students
            .SingleOrDefaultAsync(student => student.StudentId == id);
    }

    public async Task<ICollection<Student>> AllWithAbsenceScanSettings()
    {
        return await _context.Students
            .Include(student => student.School)
            .Where(student => !student.IsDeleted)
            .ToListAsync();
    }

    public async Task<ICollection<Student>> AllActiveAsync()
    {
        return await _context.Students
            .Include(student => student.School)
            .Include(student => student.FamilyMemberships)
            .OrderBy(s => s.CurrentGrade)
            .ThenBy(s => s.LastName)
            .Where(s => !s.IsDeleted)
            .ToListAsync();
    }

    public async Task<ICollection<Student>> AllActiveForFTECalculations()
    {
        return await _context.Students
            .Include(student => student.Enrolments)
                .ThenInclude(enrol => enrol.Offering)
                    .ThenInclude(offering => offering.Course)
            .Where(student => !student.IsDeleted)
            .ToListAsync();
    }

    public async Task<ICollection<Student>> ForPTOFile(Expression<Func<Student, bool>> predicate)
    {
        return await _context.Students
            .Include(student => student.Enrolments)
                .ThenInclude(enrol => enrol.Offering)
                    .ThenInclude(offering => offering.Course)
            .Include(student => student.Enrolments)
                .ThenInclude(enrol => enrol.Offering)
                    .ThenInclude(offering => offering.Sessions)
                        .ThenInclude(session => session.Teacher)
            .OrderBy(student => student.CurrentGrade)
            .ThenBy(student => student.LastName)
            .Where(student => !student.IsDeleted)
            .Where(predicate)
            .ToListAsync();
    }

    public async Task<ICollection<Student>> AllActiveForClassAuditAsync()
    {
        return await _context.Students
            .Include(student => student.School)
            .Include(student => student.Enrolments)
            .ThenInclude(enrol => enrol.Offering)
            .Where(student => !student.IsDeleted)
            .OrderBy(student => student.CurrentGrade)
            .ThenBy(student => student.LastName)
            .ToListAsync();
    }

    public async Task<ICollection<Student>> AllEnrolledInCourse(int courseId)
    {
        var students = await _context.Students
            .Include(student => student.School)
            .Include(student => student.Enrolments)
                .ThenInclude(enrol => enrol.Offering)
            .Where(student => !student.IsDeleted && student.Enrolments.Any(enrol => enrol.Offering.CourseId == courseId && !enrol.IsDeleted))
            .ToListAsync();

        return students
            .Where(student => student.Enrolments.Any(enrol => enrol.Offering.CourseId == courseId && enrol.Offering.EndDate >= DateTime.Today))
            .ToList();
    }

    public async Task<ICollection<Student>> ForListAsync(Expression<Func<Student, bool>> predicate)
    {
        return await _context.Students
            .Include(student => student.School)
            .Include(student => student.Enrolments)
            .ThenInclude(enrol => enrol.Offering)
            .OrderBy(student => student.CurrentGrade)
            .ThenBy(student => student.LastName)
            .Where(predicate)
            .ToListAsync();
    }

    public async Task<ICollection<Student>> ForTrackItSync()
    {
        return await _context.Students
            .Where(student => !student.IsDeleted)
            .ToListAsync();
    }

    public async Task<Student> ForEditAsync(string studentId)
    {
        return await _context.Students
            .SingleOrDefaultAsync(student => student.StudentId == studentId);
    }

    public async Task<Student> ForBulkUnenrolAsync(string studentId)
    {
        return await _context.Students
            .Include(student => student.Enrolments)
            .SingleOrDefaultAsync(student => student.StudentId == studentId);
    }

    public async Task<ICollection<Student>> ForSelectionListAsync()
    {
        return await _context.Students
            .Where(student => !student.IsDeleted)
            .ToListAsync();
    }

    public async Task<Student> ForAttendanceQueryReport(string studentId)
    {
        return await _context.Students
            .Include(student => student.Enrolments)
            .ThenInclude(enrolment => enrolment.Offering)
            .ThenInclude(offering => offering.Sessions)
            .ThenInclude(session => session.Period)
            .SingleOrDefaultAsync(student => student.StudentId == studentId);
    }

    public async Task<List<Student>> ForInterviewsExportAsync(
        InterviewExportSelectionDto filter,
        CancellationToken cancellationToken = default) =>
        await _context.Students
            .Include(student => student.Enrolments)
            .Include(student => student.FamilyMemberships)
            .Where(student => !student.IsDeleted)
            .Where(FilterStudent(filter))
            .ToListAsync(cancellationToken);

    public async Task<bool> AnyWithId(string id)
    {
        return await _context.Students
            .AnyAsync(student => student.StudentId == id);
    }

    public async Task<Student> ForDeletion(string id)
    {
        return await _context.Students
            .Include(student => student.Enrolments)
            .Include(student => student.Devices)
            .ThenInclude(allocation => allocation.Device)
            .SingleOrDefaultAsync(student => student.StudentId == id);
    }

    public async Task<ICollection<Student>> ForAttendanceReports()
    {
        return await _context.Students
            .Include(student => student.School)
            .Where(student => !student.IsDeleted)
            .OrderBy(student => student.School.Name)
            .ToListAsync();
    }

    public async Task<ICollection<Student>> WithoutAdobeConnectDetailsForUpdate()
    {
        return await _context.Students
            .Where(student => string.IsNullOrWhiteSpace(student.AdobeConnectPrincipalId))
            .ToListAsync();
    }

    public async Task<ICollection<Student>> ForAbsenceScan(Grade grade)
    {
        return await _context.Students
            .Include(student => student.Enrolments)
            .ThenInclude(enrol => enrol.Offering)
            .ThenInclude(offer => offer.Sessions)
            .ThenInclude(session => session.Period)
            .Include(student => student.School)
            .Include(student => student.Absences)
            .ThenInclude(absence => absence.Notifications)
            .Include(student => student.Absences)
            .ThenInclude(absence => absence.Responses)
            .Where(student => !student.IsDeleted && student.IncludeInAbsenceNotifications && student.CurrentGrade == grade)
            .ToListAsync();
    }

    private static Expression<Func<Student, bool>> FilterStudent(InterviewExportSelectionDto filter)
    {
        var predicate = PredicateBuilder.New<Student>();

        if (filter.Grades.Any())
        {
            predicate.And(student => filter.Grades.Contains((int)student.CurrentGrade));
        }

        if (filter.ClassList.Any())
        {
            predicate.And(student => student.Enrolments.Any(enrolment => !enrolment.IsDeleted && filter.ClassList.Contains(enrolment.OfferingId)));
        }

        return predicate;
    }
}