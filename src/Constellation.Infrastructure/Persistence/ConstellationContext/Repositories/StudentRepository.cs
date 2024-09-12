namespace Constellation.Infrastructure.Persistence.ConstellationContext.Repositories;

using Constellation.Core.Abstractions.Clock;
using Constellation.Core.Enums;
using Constellation.Core.Models.Offerings;
using Constellation.Core.Models.Offerings.Identifiers;
using Constellation.Core.Models.Students;
using Constellation.Core.Models.Students.Repositories;
using Constellation.Core.Models.Subjects.Identifiers;
using Core.Models.Absences;
using Core.Models.Enrolments;
using Core.Models.Students.Enums;
using Core.Models.Students.Identifiers;
using Core.Models.Students.ValueObjects;
using Core.ValueObjects;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

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

    public async Task<Student> GetById(
        StudentId studentId, 
        CancellationToken cancellationToken = default) => 
        await _context
            .Set<Student>()
            .Where(student => student.Id == studentId)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<Student?> GetBySRN(
        StudentReferenceNumber studentReferenceNumber,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Student>()
            .Where(student => student.StudentReferenceNumber == studentReferenceNumber)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<Student?> GetCurrentByEmailAddress(
        EmailAddress emailAddress, 
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Student>()
            .Where(student =>
                student.EmailAddress == emailAddress &&
                !student.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<Student?> GetAnyByEmailAddress(
        EmailAddress emailAddress, 
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Student>()
            .Where(student => student.EmailAddress == emailAddress)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<List<Student>> GetListFromIds(
        List<StudentId> studentIds,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Student>()
            .Where(student => studentIds.Contains(student.Id))
            .ToListAsync(cancellationToken);

    public async Task<List<Student>> GetCurrentEnrolmentsForOffering(
        OfferingId offeringId,
        CancellationToken cancellationToken = default)
    {
        List<StudentId> studentIds = await _context
            .Set<Enrolment>()
            .Where(enrolment => 
                enrolment.OfferingId == offeringId &&
                !enrolment.IsDeleted)
            .Select(enrolment => enrolment.StudentId)
            .ToListAsync(cancellationToken);

        return await _context
            .Set<Student>()
            .Where(student => studentIds.Contains(student.Id))
            .ToListAsync(cancellationToken);
    }

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

        List<StudentId> studentIds = await _context
            .Set<Enrolment>()
            .Where(enrolment =>
                offeringIds.Contains(enrolment.OfferingId) &&
                !enrolment.IsDeleted)
            .Select(enrolment => enrolment.StudentId)
            .ToListAsync(cancellationToken);

        return await _context
            .Set<Student>()
            .Where(student => studentIds.Contains(student.Id))
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> IsValidStudentId(
        StudentId studentId,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Student>()
            .AnyAsync(student => student.Id == studentId, cancellationToken);

    public async Task<List<Student>> GetCurrentStudents(
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Student>()
            .Where(student => !student.IsDeleted)
            .ToListAsync(cancellationToken);

    public async Task<List<Student>> GetInactiveStudents(
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Student>()
            .Where(student => student.IsDeleted)
            .ToListAsync(cancellationToken);

    public async Task<List<Student>> GetFilteredStudents(
        List<OfferingId> offeringIds,
        List<Grade> grades,
        List<string> schoolCodes,
        CancellationToken cancellationToken = default)
    {
        DateOnly today = DateOnly.FromDateTime(DateTime.Today);

        List<SchoolEnrolment> schoolEnrolments = await _context
            .Set<SchoolEnrolment>()
            .Where(enrolment => 
                !enrolment.IsDeleted &&
                enrolment.StartDate <= today &&
                (!enrolment.EndDate.HasValue || enrolment.EndDate >= today))
            .ToListAsync(cancellationToken);

        List<Student> students = await _context
            .Set<Student>()
            .Where(student => !student.IsDeleted)
            .ToListAsync(cancellationToken);

        if (offeringIds.Count > 0)
        {
            List<OfferingId> currentOfferingIds = await _context.Set<Offering>()
                .Where(offering =>
                    offeringIds.Contains(offering.Id) &&
                    offering.StartDate <= _dateTime.Today &&
                    offering.EndDate >= _dateTime.Today)
                .Select(offering => offering.Id)
                .ToListAsync(cancellationToken);

            List<StudentId> studentIds = await _context
                .Set<Enrolment>()
                .Where(enrolment =>
                    currentOfferingIds.Contains(enrolment.OfferingId) &&
                    !enrolment.IsDeleted)
                .Select(enrolment => enrolment.StudentId)
                .ToListAsync(cancellationToken);

            students = students
                .Where(student => studentIds.Contains(student.Id))
                .ToList();
        }

        if (grades.Count > 0)
        {
            students = students
                .Where(student => schoolEnrolments.Any(enrolment => 
                    enrolment.StudentId == student.Id && 
                    grades.Contains(enrolment.Grade)))
                .ToList();
        }

        if (schoolCodes.Count > 0)
        {
            students = students
                .Where(student => schoolEnrolments.Any(enrolment =>
                    enrolment.StudentId == student.Id &&
                    schoolCodes.Contains(enrolment.SchoolCode)))
                .ToList();
        }

        return students;
    }

    public async Task<List<Student>> GetCurrentStudentsFromSchool(
        string SchoolCode,
        CancellationToken cancellationToken = default)
    {
        DateOnly today = DateOnly.FromDateTime(DateTime.Today);

        return await _context
            .Set<Student>()
            .Where(student =>
                student.SchoolEnrolments.Any(enrolment =>
                    !enrolment.IsDeleted &&
                    enrolment.StartDate <= today &&
                    (!enrolment.EndDate.HasValue || enrolment.EndDate >= today) &&
                    enrolment.SchoolCode == SchoolCode) &&
                !student.IsDeleted)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Student>> GetCurrentStudentFromGrade(
        Grade grade,
        CancellationToken cancellationToken = default)
    {
        DateOnly today = DateOnly.FromDateTime(DateTime.Today);

        return await _context
            .Set<Student>()
            .Where(student =>
                student.SchoolEnrolments.Any(enrolment =>
                    !enrolment.IsDeleted &&
                    enrolment.StartDate <= today &&
                    (!enrolment.EndDate.HasValue || enrolment.EndDate >= today) &&
                    enrolment.Grade == grade) &&
                !student.IsDeleted)
            .ToListAsync(cancellationToken);
    }
    
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
            .Where(student => student.SystemLinks.All(link => link.System != SystemType.Sentral))
            .CountAsync(cancellationToken);

    public async Task<List<Student>> GetCurrentStudentsWithoutSentralId(
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Student>()
            .Where(student => student.IsDeleted == false)
            .Where(student => student.SystemLinks.All(link => link.System != SystemType.Sentral))
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

    public async Task<List<Student>> ForInterviewsExportAsync(
        List<int> filterGrades,
        List<OfferingId> filterClasses,
        CancellationToken cancellationToken = default)
    {
        List<StudentId> offeringStudentIds = await _context
            .Set<Enrolment>()
            .Where(enrolment =>
                !enrolment.IsDeleted &&
                filterClasses.Contains(enrolment.OfferingId))
            .Select(enrolment => enrolment.StudentId)
            .ToListAsync(cancellationToken);

        DateOnly today = DateOnly.FromDateTime(DateTime.Today);

        List<StudentId> gradeStudentIds = await _context
            .Set<Student>()
            .Where(student =>
                !student.IsDeleted &&
                student.SchoolEnrolments.Any(enrolment =>
                    !enrolment.IsDeleted &&
                    enrolment.StartDate <= today &&
                    (!enrolment.EndDate.HasValue || enrolment.EndDate >= today) &&
                    filterGrades.Contains((int)enrolment.Grade)))
            .Select(student => student.Id)
            .ToListAsync(cancellationToken);

        List<StudentId> combinedIdList = 
            (offeringStudentIds.Count > 0 && gradeStudentIds.Count > 0) ? offeringStudentIds.Intersect(gradeStudentIds).ToList()
            : offeringStudentIds.Count > 0 ? offeringStudentIds
            : gradeStudentIds;

        return await _context.Students
            .Where(student => 
                !student.IsDeleted &&
                combinedIdList.Contains(student.Id))
            .ToListAsync(cancellationToken);
    }
}