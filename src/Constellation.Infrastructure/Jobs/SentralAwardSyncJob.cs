namespace Constellation.Infrastructure.Jobs;

using Constellation.Application.Awards.GetAwardDetailsFromSentral;
using Constellation.Application.Awards.GetAwardIncidentsFromSentral;
using Constellation.Application.Interfaces.Jobs;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Models;
using Constellation.Core.Models.Awards;
using Constellation.Core.Models.Students;
using Core.Abstractions.Clock;
using Core.Extensions;
using Core.Shared;
using Core.ValueObjects;
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
            .Debug("Starting Sentral Awards Scan.");

        Result<List<AwardDetailResponse>> details = await _mediator.Send(new GetAwardDetailsFromSentralQuery(), cancellationToken);

        if (details.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), details.Error, true)
                .Warning("Failed to process Awards");

            return;
        }

        List<Student> students = await _studentRepository.GetCurrentStudentsWithSchool(cancellationToken);

        students = students
            .OrderBy(student => student.CurrentGrade)
            .ThenBy(student => student.LastName)
            .ThenBy(student => student.FirstName)
            .ToList();

        _logger
            .Debug("Found {count} students to process.", students.Count);

        foreach (Student student in students)
        {
            Name name = student.GetName();

            _logger
                .Debug("Processing Student {name} ({grade})", name.DisplayName, student.CurrentGrade.AsName());

            List<AwardDetailResponse> reportAwards = details
                .Value
                .Where(entry => 
                    entry.StudentId == student.StudentId)
                .ToList();

            List<StudentAward> existingAwards = await _awardRepository.GetByStudentId(student.StudentId, cancellationToken);

            _logger
                .Debug("Found {count} total awards for student {name} ({grade})", reportAwards.Count, name.DisplayName, student.CurrentGrade.AsName());

            foreach (AwardDetailResponse item in reportAwards)
            {
                if (!existingAwards.Any(award => award.Type == item.Type && award.AwardedOn == item.AwardCreated))
                {
                    _logger
                        .Debug("Found new {type} on {date}", item.Type, item.AwardCreated.ToShortDateString());

                    StudentAward entry = StudentAward.Create(
                        student.StudentId,
                        item.Category,
                        item.Type,
                        item.AwardCreated);

                    switch (item.Type)
                    {
                        case StudentAward.Astra:
                            student.AwardTally.AddAstra();
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
                }
            }

            await _unitOfWork.CompleteAsync(cancellationToken);

            existingAwards = await _awardRepository.GetByStudentId(student.StudentId, cancellationToken);

            Result<List<AwardIncidentResponse>> checkAwardRequest = await _mediator.Send(new GetAwardIncidentsFromSentralQuery(student.SentralStudentId, _dateTime.CurrentYear.ToString()), cancellationToken);

            if (checkAwardRequest.IsFailure)
            {
                _logger
                    .ForContext(nameof(Error), checkAwardRequest.Error, true)
                    .Warning("Failed to process Awards");

                return;
            }

            _logger
                .Debug("Found {count} award certificates for student {name} ({grade})", checkAwardRequest.Value.Count, name.DisplayName, student.CurrentGrade.AsName());

            List<AwardIncidentResponse> missingAwards = checkAwardRequest.Value
                .Where(award => 
                    existingAwards.All(existing => 
                        existing.IncidentId != award.IncidentId))
                .ToList();
            
            foreach (AwardIncidentResponse award in missingAwards)
            {
                StudentAward matchingAward = existingAwards
                    .FirstOrDefault(entry =>
                        entry.Category == StudentAward.Astra &&
                        entry.Type == StudentAward.Astra &&
                        entry.AwardedOn == award.AwardedAt &&
                        string.IsNullOrEmpty(entry.IncidentId));

                if (matchingAward is null)
                    continue;

                _logger
                    .ForContext(nameof(AwardIncidentResponse), award, true)
                    .ForContext(nameof(StudentAward), matchingAward, true)
                    .Debug("Matched new award certificate with Incident Id {inc} for student {name} ({grade})", award.IncidentId, name.DisplayName, student.CurrentGrade.AsName());

                await ProcessAward(award, matchingAward, student, cancellationToken);
            }

            await _unitOfWork.CompleteAsync(cancellationToken);
        }
    }

    private async Task ProcessAward(AwardIncidentResponse award, StudentAward matchingAward, Student student, CancellationToken cancellationToken = default)
    {
        Staff teacher = await _staffRepository.GetFromName(award.TeacherName);

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
