namespace Constellation.Infrastructure.Jobs;

using Constellation.Application.Extensions;
using Constellation.Application.Features.Awards.Commands;
using Constellation.Application.Features.Awards.Queries;
using Constellation.Application.Interfaces.Gateways;
using Constellation.Application.Interfaces.Jobs;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions;
using Constellation.Core.Models;
using OfficeOpenXml.ConditionalFormatting;
using Serilog;
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
    private readonly IUnitOfWork _unitOfWork;

    public SentralAwardSyncJob(
        Serilog.ILogger logger,
        ISentralGateway gateway,
        IStudentRepository studentRepository,
        IStudentAwardRepository awardRepository,
        IStoredFileRepository storedFileRepository,
        IUnitOfWork unitOfWork)
    {
        _logger = logger.ForContext<ISentralAwardSyncJob>();
        _gateway = gateway;
        _studentRepository = studentRepository;
        _awardRepository = awardRepository;
        _storedFileRepository = storedFileRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task StartJob(Guid jobId, CancellationToken token)
    {
        _logger.Information("{id}: Starting Sentral Awards Scan.", jobId);

        var details = await _gateway.GetAwardsReport();

        // Process individual students
        // Tally awards
        // Calculate expected award levels
        // Highlight discrepancies

        foreach (var group in details.GroupBy(detail => detail.StudentId))
        {
            var student = await _mediator.Send(new GetStudentWithAwardQuery { StudentId = group.Key }, token);

            _logger.LogInformation("{id}: Scanning {studentName} ({studentGrade})", jobId, student.DisplayName, student.CurrentGrade.AsName());

            foreach (var item in group)
            {
                if (!student.Awards.Any(award => award.Type == item.AwardType && award.AwardedOn == item.AwardCreated))
                {
                    _logger.LogInformation("{id}: Found new {type} on {date}", jobId, item.AwardType, item.AwardCreated.ToShortDateString());

                    await _mediator.Send(new CreateStudentAwardCommand
                    {
                        StudentId = student.StudentId,
                        Category = item.AwardCategory,
                        Type = item.AwardType,
                        AwardedOn = item.AwardCreated
                    }, token);
                }
            }
        }

        var students = await _studentRepository.GetCurrentStudentsWithSchool(token);

        //students = students.Where(student => student.LastName == "Beesley").ToList();

        _logger.Information("{id}: Found {count} students to process.", jobId, students.Count);

        foreach (var student in students)
        {
            _logger.Information("{id}: Processing Student {name} ({grade})", jobId, student.DisplayName, student.CurrentGrade.AsName());

            var awards = await _gateway.GetAwardsListing(student.SentralStudentId, DateTime.Today.Year.ToString());

            _logger.Information("{id}: Found {count} awards for student {name} ({grade})", jobId, awards.Count, student.DisplayName, student.CurrentGrade.AsName());

            var existingAwards = await _awardRepository.GetByStudentId(student.StudentId, token);
            var missingAwards = awards.Where(award => existingAwards.All(existing => existing.IncidentId != award.IncidentId)).ToList();

            foreach (var award in missingAwards)
            {
                // Save to database
                var entry = new StudentAward
                {
                    Id = Guid.NewGuid(),
                    StudentId = student.StudentId,
                    AwardedOn = award.DateIssued.ToDateTime(TimeOnly.MinValue),
                    IncidentId = award.IncidentId,
                    Reason = award.IssueReason,
                    Category = "Astra Award",
                    Type = "Astra Award"
                };

                _awardRepository.Insert(entry);

                var awardDocument = await _gateway.GetAwardDocument(student.SentralStudentId, award.IncidentId);

                var file = new StoredFile
                {
                    FileData = awardDocument,
                    FileType = "application/pdf",
                    Name = $"{student.DisplayName} - Astra Award - {award.DateIssued.ToString("dd-MM-yyyy")} - {award.IncidentId}.pdf",
                    CreatedAt = DateTime.Now,
                    LinkId = entry.Id.ToString(),
                    LinkType = StoredFile.AwardCertificate
                };

                _storedFileRepository.Insert(file);
            }

            await _unitOfWork.CompleteAsync(token);
        }

        return;
    }
}
