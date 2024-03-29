﻿namespace Constellation.Core.Abstractions.Repositories;

using Constellation.Core.Models.Absences;
using Constellation.Core.Models.Identifiers;
using Constellation.Core.Models.Offerings.Identifiers;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public interface IAbsenceRepository
{
    Task<Absence> GetById(AbsenceId absenceId, CancellationToken cancellationToken = default);
    Task<List<Absence>> GetForStudentFromCurrentYear(string StudentId, CancellationToken cancellationToken = default);
    Task<List<Absence>> GetAllFromCurrentYear(CancellationToken cancellationToken = default);
    Task<List<Absence>> GetWholeAbsencesForScanDate(DateOnly scanDate, CancellationToken cancellationToken = default);
    Task<List<Absence>> GetUnexplainedPartialAbsences(CancellationToken cancellationToken = default);
    Task<int> GetCountForStudentDateAndOffering(string studentId, DateOnly absenceDate, OfferingId offeringId, string absenceTimeframe, CancellationToken cancellationToken = default);
    Task<List<Absence>> GetAllForStudentDateAndOffering(string studentId, DateOnly absenceDate, OfferingId offeringId, string absenceTimeframe, CancellationToken cancellationToken = default);
    Task<List<Absence>> GetUnexplainedWholeAbsencesForStudentWithDelay(string studentId, int ageInWeeks, CancellationToken cancellationToken = default);
    Task<List<Absence>> GetUnexplainedPartialAbsencesForStudentWithDelay(string studentId, int ageInWeeks, CancellationToken cancellationToken = default);
    Task<List<Absence>> GetUnverifiedPartialAbsencesForStudentWithDelay(string studentId, int ageInWeeks, CancellationToken cancellationToken = default);
    Task<List<Absence>> GetForStudentFromDateRange(string studentId, DateOnly startDate, DateOnly endDate, CancellationToken cancellationToken = default);
    Task<List<Absence>> GetForStudents(List<string> studentIds, CancellationToken cancellationToken = default);
    void Insert(Absence absence);
}
