namespace Constellation.Infrastructure.Jobs;

using Application.Domains.Attendance.Absences.Commands.SendAbsenceDigestToCoordinator;
using Application.Domains.Attendance.Absences.Commands.SendAbsenceDigestToParent;
using Application.Domains.Attendance.Absences.Commands.SendAbsenceDigestToStudent;
using Application.Domains.Attendance.Absences.Commands.SendAbsenceNotificationToParent;
using Application.Domains.Attendance.Absences.Commands.SendAbsenceNotificationToStudent;
using Application.Domains.Attendance.Absences.Commands.SendMissedWorkEmailToStudent;
using Application.Interfaces.Repositories;
using Constellation.Application.Interfaces.Jobs;
using Constellation.Core.Models.Students.Repositories;
using Core.Abstractions.Repositories;
using Core.Enums;
using Core.Models.Absences;
using Core.Models.Identifiers;
using Core.Models.Students;
using Core.Models.Students.Enums;

internal sealed class AbsenceMonitorJob : IAbsenceMonitorJob
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

        foreach (Grade grade in Enum.GetValues<Grade>())
        {
            if (cancellationToken.IsCancellationRequested)
                return;

            _logger.Information("{id}: Getting students from {grade}", jobId, grade);

            List<Student> students = await _studentRepository.GetCurrentStudentFromGrade(grade, cancellationToken);

            students = students
                .OrderBy(student => student.Name.SortOrder)
                .ToList();

            _logger.Information("{id}: Found {students} students to scan.", jobId, students.Count);

            foreach (Student student in students)
            {
                if (cancellationToken.IsCancellationRequested)
                    return;

                List<Absence> absences = await _absenceProcessor.StartJob(jobId, student, cancellationToken);

                // If the student has absences with new external explanations,
                // and without any new absences, their details will not be saved
                // unless we do it here
                // This must be done early, otherwise the digests will not reflect these changes
                await _unitOfWork.CompleteAsync(cancellationToken);

                if (absences.Count != 0)
                {
                    if (cancellationToken.IsCancellationRequested)
                        return;

                    foreach (Absence absence in absences)
                        _absenceRepository.Insert(absence);

                    await _unitOfWork.CompleteAsync(cancellationToken);

                    SystemLink sentralId = student.SystemLinks.FirstOrDefault(link => link.System == SystemType.Sentral);

                    if (sentralId is null)
                        continue;

                    if (cancellationToken.IsCancellationRequested)
                        return;

                    List<AbsenceId> partialAbsenceIds = absences
                        .Where(absence => 
                            absence.Type == AbsenceType.Partial &&
                            !absence.Explained)
                        .Select(absence => absence.Id)
                        .ToList();

                    if (partialAbsenceIds.Count != 0)
                        await _mediator.Send(new SendAbsenceNotificationToStudentCommand(
                            jobId,
                            student.Id,
                            partialAbsenceIds),
                            cancellationToken);

                    List<AbsenceId> wholeAbsenceIds = absences
                        .Where(absence => 
                            absence.Type == AbsenceType.Whole &&
                            !absence.Explained)
                        .Select(absence => absence.Id)
                        .ToList();

                    if (wholeAbsenceIds.Count != 0)
                        await _mediator.Send(new SendAbsenceNotificationToParentCommand(
                            jobId,
                            student.Id,
                            wholeAbsenceIds),
                            cancellationToken);
                }

                if (DateTime.Now.DayOfWeek == DayOfWeek.Monday)
                {
                    if (cancellationToken.IsCancellationRequested)
                        return;

                    await _mediator.Send(new SendAbsenceDigestToStudentCommand(jobId, student.Id), cancellationToken);
                    await _mediator.Send(new SendAbsenceDigestToParentCommand(jobId, student.Id), cancellationToken);
                    await _mediator.Send(new SendAbsenceDigestToCoordinatorCommand(jobId, student.Id), cancellationToken);
                }

                // If there was any whole absence found for yesterday, send the missed work email to student and parents
                List<Absence> yesterdaysAbsences = absences
                    .Where(absence =>
                        absence.Date == DateOnly.FromDateTime(DateTime.Today.AddDays(-1)) &&
                        absence.Type == AbsenceType.Whole)
                    .ToList();

                foreach (Absence absence in yesterdaysAbsences)
                {
                    if (absence.AbsenceReason == AbsenceReason.Leave ||
                        absence.AbsenceReason == AbsenceReason.Suspended)
                        continue;

                    //Send missed work generic email
                   await _mediator.Send(new SendMissedWorkEmailToStudentCommand(
                       jobId,
                       student.Id,
                       absence.OfferingId,
                       absence.Date),
                       cancellationToken);
                }

                await _unitOfWork.CompleteAsync(cancellationToken);
            }
        }
    }
}
