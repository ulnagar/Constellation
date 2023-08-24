namespace Constellation.Infrastructure.Jobs;

using Constellation.Application.DTOs.Awards;
using Constellation.Application.Extensions;
using Constellation.Application.Interfaces.Gateways;
using Constellation.Application.Interfaces.Jobs;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Models;
using Constellation.Core.Models.Awards;
using Constellation.Core.Models.Identifiers;
using System;
using System.Threading;
using System.Threading.Tasks;

public class SentralAwardSyncJob : ISentralAwardSyncJob
{
    private readonly Serilog.ILogger _logger;
    private readonly ISentralGateway _gateway;
    private readonly IStudentRepository _studentRepository;
    private readonly IStudentAwardRepository _awardRepository;
    private readonly IStoredFileRepository _storedFileRepository;
    private readonly IStaffRepository _staffRepository;
    private readonly IUnitOfWork _unitOfWork;

    public SentralAwardSyncJob(
        Serilog.ILogger logger,
        ISentralGateway gateway,
        IStudentRepository studentRepository,
        IStudentAwardRepository awardRepository,
        IStoredFileRepository storedFileRepository,
        IStaffRepository staffRepository,
        IUnitOfWork unitOfWork)
    {
        _logger = logger.ForContext<ISentralAwardSyncJob>();
        _gateway = gateway;
        _studentRepository = studentRepository;
        _awardRepository = awardRepository;
        _storedFileRepository = storedFileRepository;
        _staffRepository = staffRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task StartJob(Guid jobId, CancellationToken token)
    {
        _logger.Information("{id}: Starting Sentral Awards Scan.", jobId);

        var details = await _gateway.GetAwardsReport();

        var students = await _studentRepository.GetCurrentStudentsWithSchool(token);

        students = students
            .OrderBy(student => student.CurrentGrade)
            .ThenBy(student => student.LastName)
            .ThenBy(student => student.FirstName)
            .ToList();

        _logger.Information("{id}: Found {count} students to process.", jobId, students.Count);

        foreach (var student in students)
        {
            _logger.Information("{id}: Processing Student {name} ({grade})", jobId, student.DisplayName, student.CurrentGrade.AsName());

            var reportAwards = details.Where(entry => entry.StudentId == student.StudentId).ToList();
            var existingAwards = await _awardRepository.GetByStudentId(student.StudentId, token);

            _logger.Information("{id}: Found {count} total awards for student {name} ({grade})", jobId, reportAwards.Count, student.DisplayName, student.CurrentGrade.AsName());

            foreach (var item in reportAwards)
            {
                if (!existingAwards.Any(award => award.Type == item.AwardType && award.AwardedOn == item.AwardCreated))
                {
                    _logger.Information("{id}: Found new {type} on {date}", jobId, item.AwardType, item.AwardCreated.ToShortDateString());

                    var entry = StudentAward.Create(
                        new StudentAwardId(),
                        student.StudentId,
                        item.AwardCategory,
                        item.AwardType,
                        item.AwardCreated);

                    _awardRepository.Insert(entry);
                }
            }

            await _unitOfWork.CompleteAsync(token);

            existingAwards = await _awardRepository.GetByStudentId(student.StudentId, token);

            var checkAwards = await _gateway.GetAwardsListing(student.SentralStudentId, DateTime.Today.Year.ToString());

            _logger.Information("{id}: Found {count} award certificates for student {name} ({grade})", jobId, checkAwards.Count, student.DisplayName, student.CurrentGrade.AsName());

            var missingAwards = checkAwards.Where(award => existingAwards.All(existing => existing.IncidentId != award.IncidentId)).ToList();

            List<AwardIncidentDto> unmatchedAwards = new();

            foreach (var award in missingAwards)
            {
                var matchingAward = existingAwards
                    .FirstOrDefault(entry =>
                        entry.Category == "Astra Award" &&
                        entry.Type == "Astra Award" &&
                        DateOnly.FromDateTime(entry.AwardedOn) == award.DateIssued &&
                        string.IsNullOrEmpty(entry.IncidentId));

                if (matchingAward is null)
                {
                    unmatchedAwards.Add(award);
                    continue;
                }

                _logger.Information("{id}: Matched new award certificate with Incident Id {inc} for student {name} ({grade})", jobId, award.IncidentId, student.DisplayName, student.CurrentGrade.AsName());

                await ProcessAward(award, matchingAward, student);
            }

            foreach (var award in unmatchedAwards)
            {
                var matchingAward = existingAwards
                    .FirstOrDefault(entry =>
                        entry.Category == "Astra Award" &&
                        entry.Type == "Astra Award" &&
                        (DateOnly.FromDateTime(entry.AwardedOn) <= award.DateIssued.AddDays(3) ||
                        DateOnly.FromDateTime(entry.AwardedOn) >= award.DateIssued.AddDays(-3)) &&
                        string.IsNullOrEmpty(entry.IncidentId));

                if (matchingAward is not null)
                {
                    _logger.Information("{id}: Matched new award certificate with Incident Id {inc} for student {name} ({grade})", jobId, award.IncidentId, student.DisplayName, student.CurrentGrade.AsName());
                    
                    await ProcessAward(award, matchingAward, student);
                }
            }

            await _unitOfWork.CompleteAsync(token);
        }

        return;
    }

    private async Task ProcessAward(AwardIncidentDto award, StudentAward matchingAward, Student student)
    {
        var teacher = await _staffRepository.GetFromName(award.TeacherName);

        if (teacher is null)
        {
            _logger.Warning("Could not identify staff member from name {name} while processing award {@award}/{@dbAward}", award.TeacherName, award, matchingAward);

            return;
        }

        // Update existing entry with the new details
        matchingAward.Update(
            award.IncidentId,
            teacher.StaffId,
            award.IssueReason);

        var awardDocument = await _gateway.GetAwardDocument(student.SentralStudentId, award.IncidentId);

        if (awardDocument.Length == 0)
        {
            _logger.Warning("Failed to retrieve the award certificate for award {@award}", matchingAward);

            return;
        }

        var file = new StoredFile
        {
            FileData = awardDocument,
            FileType = "application/pdf",
            Name = $"{student.DisplayName} - Astra Award - {award.DateIssued:dd-MM-yyyy} - {award.IncidentId}.pdf",
            CreatedAt = DateTime.Now,
            LinkId = matchingAward.Id.ToString(),
            LinkType = StoredFile.AwardCertificate
        };

        _storedFileRepository.Insert(file);
    }
}
