﻿namespace Constellation.Infrastructure.Jobs;

using Constellation.Application.Absences.SendAbsenceDigestToCoordinator;
using Constellation.Application.Absences.SendAbsenceDigestToParent;
using Constellation.Application.Absences.SendAbsenceNotificationToParent;
using Constellation.Application.Absences.SendAbsenceNotificationToStudent;
using Constellation.Application.Absences.SendMissedWorkEmailToStudent;
using Constellation.Application.Interfaces.Jobs;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Interfaces.Services;
using Constellation.Core.Abstractions;
using Constellation.Core.Enums;
using Constellation.Core.Models;
using Constellation.Core.Models.Absences;
using Constellation.Core.Models.Identifiers;

public class AbsenceMonitorJob : IAbsenceMonitorJob, IHangfireJob
{
    private readonly IStudentRepository _studentRepository;
    private readonly IAbsenceRepository _absenceRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAbsenceProcessingJob _absenceProcessor;
    private readonly IMediator _mediator;
    private readonly ILogger _logger;

    public AbsenceMonitorJob(
        IStudentRepository studentRepository,
        IAbsenceRepository absenceRepository,
        IUnitOfWork unitOfWork,
        IAbsenceProcessingJob absenceProcessor,
        IMediator mediator,
        ILogger logger)
    {
        _studentRepository = studentRepository;
        _absenceRepository = absenceRepository;
        _unitOfWork = unitOfWork;
        _absenceProcessor = absenceProcessor;
        _mediator = mediator;
        _logger = logger.ForContext<IAbsenceMonitorJob>();
    }

    public async Task StartJob(Guid jobId, CancellationToken cancellationToken)
    {
        _logger.Information("{id}: Starting Absence Monitor Scan.", jobId);

        foreach (Grade grade in Enum.GetValues(typeof(Grade)))
        {
            if (cancellationToken.IsCancellationRequested)
                return;

            _logger.Information("{id}: Getting students from {grade}", jobId, grade);

            List<Student> students = await _studentRepository.GetCurrentStudentFromGrade(grade, cancellationToken);

            students = students
                .OrderBy(student => student.LastName)
                .ThenBy(student => student.FirstName)
                .ToList();

            _logger.Information("{id}: Found {students} students to scan.", jobId, students.Count);

            foreach (Student student in students)
            {
                if (cancellationToken.IsCancellationRequested)
                    return;

                List<Absence> absences = await _absenceProcessor.StartJob(jobId, student, cancellationToken);

                if (absences.Any())
                {
                    if (cancellationToken.IsCancellationRequested)
                        return;

                    foreach (Absence absence in absences)
                        _absenceRepository.Insert(absence);

                    if (string.IsNullOrWhiteSpace(student.SentralStudentId))
                        continue;

                    if (cancellationToken.IsCancellationRequested)
                        return;

                    List<AbsenceId> partialAbsenceIds = absences
                        .Where(absence => absence.Type == AbsenceType.Partial)
                        .Select(absence => absence.Id)
                        .ToList();

                    if (partialAbsenceIds.Count != 0)
                        await _mediator.Send(new SendAbsenceNotificationToStudentCommand(
                            jobId,
                            student.StudentId,
                            partialAbsenceIds),
                            cancellationToken);

                    List<AbsenceId> wholeAbsenceIds = absences
                        .Where(absence => absence.Type == AbsenceType.Whole)
                        .Select(absence => absence.Id)
                        .ToList();

                    if (wholeAbsenceIds.Count != 0)
                        await _mediator.Send(new SendAbsenceNotificationToParentCommand(
                            jobId,
                            student.StudentId,
                            wholeAbsenceIds),
                            cancellationToken);

                    partialAbsenceIds = null;
                    wholeAbsenceIds = null;
                }

                if (DateTime.Now.DayOfWeek == DayOfWeek.Monday)
                {
                    if (cancellationToken.IsCancellationRequested)
                        return;

                    await _mediator.Send(new SendAbsenceDigestToParentCommand(jobId, student.StudentId), cancellationToken);
                    await _mediator.Send(new SendAbsenceDigestToCoordinatorCommand(jobId, student.StudentId), cancellationToken);
                }

                // If there was any whole absence found for yesterday, send the missed work email to student and parents
                if (absences.Any(absence => 
                    absence.Date == DateOnly.FromDateTime(DateTime.Today.AddDays(-1)) && 
                    absence.Type == AbsenceType.Whole))
                {
                    // Send missed work generic email
                    await _mediator.Send(new SendMissedWorkEmailToStudentCommand(
                        jobId, 
                        student.StudentId),
                        cancellationToken);
                }

                await _unitOfWork.CompleteAsync(cancellationToken);
                absences = null;
            }
        }
    }
}
