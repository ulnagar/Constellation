namespace Constellation.Core.Abstractions.Repositories;

using Constellation.Core.Models.Absences.Identifiers;
using Models.Absences;
using Models.Offerings.Identifiers;
using Models.Students.Identifiers;
using Models.Tutorials.Identifiers;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public interface IAbsenceRepository
{
    Task<Absence> GetById(AbsenceId absenceId, CancellationToken cancellationToken = default);
    Task<List<Absence>> GetForStudentFromCurrentYear(StudentId studentId, CancellationToken cancellationToken = default);
    Task<List<Absence>> GetAllFromCurrentYear(CancellationToken cancellationToken = default);
    Task<List<Absence>> GetWholeAbsencesForScanDate(DateOnly scanDate, CancellationToken cancellationToken = default);
    Task<List<Absence>> GetUnexplainedPartialAbsences(CancellationToken cancellationToken = default);
    Task<int> GetCountForStudentDateAndOffering(StudentId studentId, DateOnly absenceDate, OfferingId offeringId, string absenceTimeframe, CancellationToken cancellationToken = default);
    Task<int> GetCountForStudentDateAndTutorial(StudentId studentId, DateOnly absenceDate, TutorialId tutorialId, string absenceTimeframe, CancellationToken cancellationToken = default);
    Task<List<Absence>> GetAllForStudentDateAndOffering(StudentId studentId, DateOnly absenceDate, OfferingId offeringId, string absenceTimeframe, CancellationToken cancellationToken = default);
    Task<List<Absence>> GetAllForStudentDateAndTutorial(StudentId studentId, DateOnly absenceDate, TutorialId tutorialId, string absenceTimeframe, CancellationToken cancellationToken = default);
    Task<List<Absence>> GetUnexplainedWholeAbsencesForStudentWithDelay(StudentId studentId, int ageInWeeks, CancellationToken cancellationToken = default);
    Task<List<Absence>> GetUnexplainedPartialAbsencesForStudentWithDelay(StudentId studentId, int ageInWeeks, CancellationToken cancellationToken = default);
    Task<List<Absence>> GetUnverifiedPartialAbsencesForStudentWithDelay(StudentId studentId, int ageInWeeks, CancellationToken cancellationToken = default);
    Task<List<Absence>> GetForStudentFromDateRange(StudentId studentId, DateOnly startDate, DateOnly endDate, CancellationToken cancellationToken = default);
    Task<List<Absence>> GetForStudents(List<StudentId> studentIds, CancellationToken cancellationToken = default);
    Task<List<Absence>> GetStudentWholeAbsencesForDateRange(StudentId studentId, DateOnly startDate, DateOnly endDate, CancellationToken cancellationToken = default);
    void Insert(Absence absence);
}
