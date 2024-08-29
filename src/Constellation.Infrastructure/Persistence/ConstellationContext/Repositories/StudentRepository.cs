namespace Constellation.Infrastructure.Persistence.ConstellationContext.Repositories;

using Constellation.Core.Abstractions.Clock;
using Constellation.Core.Enums;
using Constellation.Core.Models.Offerings;
using Constellation.Core.Models.Offerings.Identifiers;
using Constellation.Core.Models.Students;
using Constellation.Core.Models.Students.Repositories;
using Constellation.Core.Models.Subjects.Identifiers;
using Core.Models.Absences;
using LinqKit;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq.Expressions;

public class StudentRepository : IStudentRepository
{
    private readonly AppDbContext _context;
    private readonly IDateTimeProvider _dateTime;

    public StudentRepository(
        AppDbContext context,
        IDateTimeProvider dateTime)
    {
        _context = context;
        _dateTime = dateTime;
    }

    public async Task<List<Student>> GetAll(
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Student>()
            .ToListAsync(cancellationToken);

    public async Task<List<Student>> GetAllWithSchool(
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Student>()
            .Include(student => student.School)
            .ToListAsync(cancellationToken);
    
    public async Task<Student?> GetBySRN(
        string StudentId,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Student>()
            .Where(student => student.StudentId == StudentId)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<Student?> GetWithSchoolBySRN(
        string studentId,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Student>()
            .Include(student => student.School)
            .Where(student => student.StudentId == studentId)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<Student?> GetCurrentByEmailAddress(
        string emailAddress, 
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Student>()
            .Where(student =>
                emailAddress.Contains(student.PortalUsername) &&
                !student.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<Student?> GetAnyByEmailAddress(
        string emailAddress, 
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Student>()
            .Where(student =>
                emailAddress.Contains(student.PortalUsername))
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<List<Student>> GetListFromIds(
        List<string> studentIds,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Student>()
            .Where(student => studentIds.Contains(student.StudentId))
            .ToListAsync(cancellationToken);

    public async Task<List<Student>> GetCurrentEnrolmentsForOffering(
        OfferingId offeringId,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Student>()
            .Where(student => student.Enrolments.Any(enrolment => enrolment.OfferingId == offeringId && !enrolment.IsDeleted))
            .ToListAsync(cancellationToken);

    public async Task<List<Student>> GetCurrentEnrolmentsForCourse(
        CourseId courseId,
        CancellationToken cancellationToken = default)
    {
        List<OfferingId> offeringIds = await _context
            .Set<Offering>()
            .Where(offering => 
                offering.CourseId == courseId &&
                offering.StartDate <= _dateTime.Today &&
                offering.EndDate >= _dateTime.Today)
            .Select(offering => offering.Id)
            .ToListAsync(cancellationToken);

        List<Student> students = await _context
            .Set<Student>()
            .Where(student => student.Enrolments.Any(enrolment => 
                offeringIds.Contains(enrolment.OfferingId) && 
                !enrolment.IsDeleted))
            .ToListAsync(cancellationToken);

        return students;
    }

    public async Task<List<Student>> GetCurrentEnrolmentsForOfferingWithSchool(
        OfferingId offeringId,
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

    public async Task<List<Student>> GetInactiveStudentsWithSchool(
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Student>()
            .Include(student => student.School)
            .Where(student => student.IsDeleted)
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
        List<OfferingId> OfferingIds,
        List<Grade> Grades,
        List<string> SchoolCodes,
        CancellationToken cancellationToken = default)
    {
        IQueryable<Student> students = _context
            .Set<Student>()
            .Include(student => student.School)
            .Include(student => student.Enrolments)
            .Where(student => !student.IsDeleted);

        if (OfferingIds.Count > 0)
        {
            List<OfferingId> currentOfferingIds = await _context.Set<Offering>()
                .Where(offering =>
                    OfferingIds.Contains(offering.Id) &&
                    offering.StartDate <= _dateTime.Today &&
                    offering.EndDate >= _dateTime.Today)
                .Select(offering => offering.Id)
                .ToListAsync(cancellationToken);

            students = students
                .Where(student => student.Enrolments.Any(enrol =>
                    !enrol.IsDeleted &&
                    currentOfferingIds.Contains(enrol.OfferingId)));
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

    public async Task<int> GetCountCurrentStudentsWithPartialAbsenceScanDisabled(
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Student>()
            .Where(student => student.IsDeleted == false)
            .Where(student => !student.AbsenceConfigurations.Any(configuration =>
                configuration.AbsenceType == AbsenceType.Partial &&
                !configuration.IsDeleted &&
                configuration.CalendarYear == _dateTime.CurrentYear &&
                configuration.ScanStartDate <= _dateTime.Today &&
                configuration.ScanEndDate >= _dateTime.Today))
            .CountAsync(cancellationToken);

    public async Task<int> GetCountCurrentStudentsWithWholeAbsenceScanDisabled(
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Student>()
            .Where(student => student.IsDeleted == false)
            .Where(student => !student.AbsenceConfigurations.Any(configuration =>
                configuration.AbsenceType == AbsenceType.Whole &&
                !configuration.IsDeleted &&
                configuration.CalendarYear == _dateTime.CurrentYear &&
                configuration.ScanStartDate <= _dateTime.Today &&
                configuration.ScanEndDate >= _dateTime.Today))
            .CountAsync(cancellationToken);

    public async Task<int> GetCountCurrentStudentsWithoutSentralId(
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Student>()
            .Where(student => student.IsDeleted == false)
            .Where(student => string.IsNullOrWhiteSpace(student.SentralStudentId))
            .CountAsync(cancellationToken);

    public async Task<List<Student>> GetCurrentStudentsWithoutSentralId(
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Student>()
            .Include(student => student.School)
            .Where(student => student.IsDeleted == false)
            .Where(student => string.IsNullOrWhiteSpace(student.SentralStudentId))
            .ToListAsync(cancellationToken);

    public async Task<int> GetCountCurrentStudentsWithAwardOverages(
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Student>()
            .Where(student => student.IsDeleted == false)
            .Where(student =>
                student.AwardTally.Stellars > student.AwardTally.Astras / 5 ||
                student.AwardTally.GalaxyMedals > student.AwardTally.Astras / 25 ||
                student.AwardTally.UniversalAchievers > student.AwardTally.Astras / 125)
            .CountAsync(cancellationToken);

    public async Task<int> GetCountCurrentStudentsWithPendingAwards(
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Student>()
            .Where(student => student.IsDeleted == false)
            .Where(student =>
                student.AwardTally.Stellars < student.AwardTally.Astras / 5 ||
                student.AwardTally.GalaxyMedals < student.AwardTally.Astras / 25 ||
                student.AwardTally.UniversalAchievers < student.AwardTally.Astras / 125)
            .CountAsync(cancellationToken);

    public void Insert(Student student) => _context.Set<Student>().Add(student);

    public async Task<Student> GetForExistCheck(string id)
    {
        return await _context.Students
            .SingleOrDefaultAsync(student => student.StudentId == id);
    }

    public async Task<ICollection<Student>> ForListAsync(Expression<Func<Student, bool>> predicate)
    {
        return await _context.Students
            .Include(student => student.School)
            .Include(student => student.Enrolments)
            .OrderBy(student => student.CurrentGrade)
            .ThenBy(student => student.LastName)
            .Where(predicate)
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

    public async Task<List<Student>> ForInterviewsExportAsync(
        List<int> filterGrades, 
        List<OfferingId> filterClasses,
        CancellationToken cancellationToken = default) =>
        await _context.Students
            .Include(student => student.Enrolments)
            .Include(student => student.FamilyMemberships)
            .Where(student => !student.IsDeleted)
            .Where(FilterStudent(filterGrades, filterClasses))
            .ToListAsync(cancellationToken);

    public async Task<ICollection<Student>> WithoutAdobeConnectDetailsForUpdate()
    {
        return await _context.Students
            .Where(student => string.IsNullOrWhiteSpace(student.AdobeConnectPrincipalId))
            .ToListAsync();
    }

    private static Expression<Func<Student, bool>> FilterStudent(List<int> filterGrades, List<OfferingId> filterClasses)
    {
        ExpressionStarter<Student> predicate = PredicateBuilder.New<Student>();

        if (filterGrades.Count > 0)
        {
            predicate.And(student => filterGrades.Contains((int)student.CurrentGrade));
        }

        if (filterClasses.Count > 0)
        {
            predicate.And(student => student.Enrolments.Any(enrolment => !enrolment.IsDeleted && filterClasses.Contains(enrolment.OfferingId)));
        }

        return predicate;
    }
}