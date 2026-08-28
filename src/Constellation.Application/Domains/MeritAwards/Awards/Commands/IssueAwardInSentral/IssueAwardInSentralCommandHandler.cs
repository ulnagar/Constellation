namespace Constellation.Application.Domains.MeritAwards.Awards.Commands.IssueAwardInSentral;

using Abstractions.Messaging;
using Constellation.Application.Domains.Import.Models;
using Constellation.Core.Enums;
using Constellation.Core.Primitives;
using Core.Abstractions.Repositories;
using Core.Models.Awards;
using Core.Models.Students;
using Core.Models.Students.Repositories;
using Core.Shared;
using Enums;
using Interfaces.Gateways;
using Interfaces.Repositories;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed record IssueAwardInSentralCommandHandler
    : ICommandHandler<IssueAwardInSentralCommand>
{
    private readonly IStudentRepository _studentRepository;
    private readonly IStudentAwardRepository _awardRepository;
    private readonly SentralAwardReportCsvParser _parser;
    private readonly ISentralGateway _gateway;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public IssueAwardInSentralCommandHandler(
        IStudentRepository studentRepository,
        IStudentAwardRepository awardRepository,
        SentralAwardReportCsvParser parser,
        ISentralGateway gateway,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _studentRepository = studentRepository;
        _awardRepository = awardRepository;
        _parser = parser;
        _gateway = gateway;
        _unitOfWork = unitOfWork;
        _logger = logger
            .ForContext<IssueAwardInSentralCommand>();
    }

    public async Task<Result> Handle(IssueAwardInSentralCommand request, CancellationToken cancellationToken)
    {
        List<Student> students = await _studentRepository.GetListFromIds(request.StudentIds, cancellationToken);

        List<string> sentralIds = students
            .SelectMany(student => student.SystemLinks.Where(link => link.System == SystemType.Sentral))
            .Select(link => link.Value)
            .Distinct()
            .ToList();

        Result<DateTime> result = await _gateway.IssueAward(sentralIds, request.AwardType);

        if (result.IsFailure)
            return result;

        Stream stream = await _gateway.GetAwardsReport(cancellationToken);

        Result<List<StudentAwardRow>> rows = _parser.Parse(stream);

        string awardType = request.AwardType switch
        {
            IssueAwardType.Stellar => StudentAward.Stellar,
            IssueAwardType.Galaxy => StudentAward.Galaxy,
            IssueAwardType.Universal => StudentAward.Universal
        };

        foreach (var student in students)
        {
            SystemLink? systemLink = student.SystemLinks.FirstOrDefault(link => link.System == SystemType.Sentral);

            if (systemLink is null)
                continue;

            List<StudentAwardRow> filteredRows = rows.Value
                .Where(entry => 
                    entry.StudentId == systemLink.Value 
                    && entry.Type == awardType)
                .ToList();

            List<StudentAward> existingAwards = await _awardRepository.GetByStudentId(student.Id, cancellationToken);

            foreach (StudentAwardRow item in filteredRows)
            {
                StudentAward? matchingAward = existingAwards.FirstOrDefault(award =>
                    award.Type == item.Type &&
                    award.AwardedOn == item.AwardCreated);

                if (matchingAward is null)
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
                else
                {
                    existingAwards.Remove(matchingAward);
                }
            }
        }

        await _unitOfWork.CompleteAsync(cancellationToken);
        
        return result;
    }
}