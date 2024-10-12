namespace Constellation.Infrastructure.Jobs;

using Constellation.Application.Awards.GetAwardDetailsFromSentral;
using Constellation.Application.Awards.GetAwardIncidentsFromSentral;
using Constellation.Application.Interfaces.Jobs;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Models;
using Constellation.Core.Models.Awards;
using Constellation.Core.Models.Students;
using Constellation.Core.Models.Students.Repositories;
using Core.Abstractions.Clock;
using Core.Extensions;
using Core.Models.StaffMembers.Repositories;
using Core.Models.Students.Enums;
using Core.Shared;
using System;
using System.Threading;
using System.Threading.Tasks;

internal sealed class SentralAwardSyncJob : ISentralAwardSyncJob
{
    private readonly ILogger _logger;
    private readonly ISender _mediator;
    private readonly IStudentRepository _studentRepository;
    private readonly IStudentAwardRepository _awardRepository;
    private readonly IStaffRepository _staffRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _dateTime;

    public SentralAwardSyncJob(
        ILogger logger,
        ISender mediator,
        IStudentRepository studentRepository,
        IStudentAwardRepository awardRepository,
        IStaffRepository staffRepository,
        IUnitOfWork unitOfWork,
        IDateTimeProvider dateTime)
    {
        _logger = logger.ForContext<ISentralAwardSyncJob>();
        _mediator = mediator;
        _studentRepository = studentRepository;
        _awardRepository = awardRepository;
        _staffRepository = staffRepository;
        _unitOfWork = unitOfWork;
        _dateTime = dateTime;
    }

    public async Task StartJob(Guid jobId, CancellationToken cancellationToken)
    {
        _logger.ForContext(nameof(jobId), jobId);

        _logger
            .Information("Starting Sentral Awards Scan.");

        Result<List<AwardDetailResponse>> details = await _mediator.Send(new GetAwardDetailsFromSentralQuery(), cancellationToken);

        if (details.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), details.Error, true)
                .Warning("Failed to process Awards");

            return;
        }

        List<Student> students = await _studentRepository.GetCurrentStudents(cancellationToken);

        students = students
            .OrderBy(student => student.CurrentEnrolment?.Grade)
            .ThenBy(student => student.Name.SortOrder)
            .ToList();

        List<Staff> teachers = await _staffRepository.GetAllActive(cancellationToken);

        _logger
            .Information("Found {count} students to process.", students.Count);

        foreach (Student student in students)
        {
            _logger
                .Information("Processing Student {name} ({grade})", student.Name.DisplayName, student.CurrentEnrolment?.Grade.AsName());

            SchoolEnrolment? enrolment = student.CurrentEnrolment;

            if (enrolment is null)
                continue;

            SystemLink systemLink = student.SystemLinks.FirstOrDefault(link => link.System == SystemType.Sentral);

            if (systemLink is null)
                continue;

            List<AwardDetailResponse> reportAwards = details
                .Value
                .Where(entry => 
                    entry.StudentReferenceNumber == student.StudentReferenceNumber.Number)
                .ToList();

            List<StudentAward> existingAwards = await _awardRepository.GetByStudentId(student.Id, cancellationToken);

            _logger
                .Information("Found {count} total awards for student {name} ({grade})", reportAwards.Count, student.Name.DisplayName, enrolment.Grade.AsName());

            Result<List<AwardIncidentResponse>> awardIncidentsRequest = await _mediator.Send(new GetAwardIncidentsFromSentralQuery(systemLink.Value, _dateTime.CurrentYear.ToString()), cancellationToken);

            if (awardIncidentsRequest.IsFailure)
            {
                _logger
                    .ForContext(nameof(Error), awardIncidentsRequest.Error, true)
                    .Warning("Failed to process Awards");

                continue;
            }

            _logger
                .Information("Found {count} award incidents for student {name} ({grade})", awardIncidentsRequest.Value.Count, student.Name.DisplayName, enrolment.Grade.AsName());

            foreach (AwardDetailResponse item in reportAwards)
            {
                if (!existingAwards.Any(award => award.Type == item.Type && award.AwardedOn.AddSeconds(-award.AwardedOn.Second) == item.AwardCreated))
                {
                    _logger
                        .Information("Found new {type} on {date}", item.Type, item.AwardCreated.ToShortDateString());

                    StudentAward entry = StudentAward.Create(
                        student.Id,
                        item.Category,
                        item.Type,
                        item.AwardCreated);

                    switch (item.Type)
                    {
                        case StudentAward.Astra:
                            student.AwardTally.AddAstra();

                            AwardIncidentResponse matchingIncident = awardIncidentsRequest.Value
                                .FirstOrDefault(incident =>
                                    incident.AwardedAt == entry.AwardedOn);

                            if (matchingIncident is not null)
                                ProcessAward(matchingIncident, entry, student, teachers);

                            break;

                        case StudentAward.Stellar:
                            student.AwardTally.AddStellar();
                            break;

                        case StudentAward.Galaxy:
                            student.AwardTally.AddGalaxyMedal();
                            break;

                        case StudentAward.Universal:
                            student.AwardTally.AddUniversalAchiever();
                            break;
                    }

                    _awardRepository.Insert(entry);

                    existingAwards.Add(entry);
                }
            }

            await _unitOfWork.CompleteAsync(cancellationToken);
        }
    }

    private void ProcessAward(AwardIncidentResponse award, StudentAward matchingAward, Student student, List<Staff> teachers)
    {
        Staff teacher = teachers.FirstOrDefault(staff =>
        {
            string[] splitName = award.TeacherName.Trim().Split(' ');

            if (staff.FirstName.Contains(splitName[0], StringComparison.InvariantCultureIgnoreCase) && staff.LastName.Contains(splitName[1], StringComparison.InvariantCultureIgnoreCase))
                return true;

            string username = award.TeacherName.Trim().Replace(' ', '.').Replace("-", string.Empty, StringComparison.InvariantCultureIgnoreCase);

            return staff.PortalUsername.Contains(username, StringComparison.InvariantCultureIgnoreCase);
        });

        if (teacher is null)
        {
            _logger
                .ForContext(nameof(AwardIncidentResponse), award, true)
                .ForContext(nameof(StudentAward), matchingAward, true)
                .Warning("Could not identify staff member from name {name} while processing award", award.TeacherName);

            return;
        }

        // Update existing entry with the new details
        matchingAward.Update(
            award.IncidentId,
            teacher.StaffId,
            award.IssueReason);
    }
}
