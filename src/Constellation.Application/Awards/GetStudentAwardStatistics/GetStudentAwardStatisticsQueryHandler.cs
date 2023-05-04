namespace Constellation.Application.Awards.GetStudentAwardStatistics;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions;
using Constellation.Core.Models.Awards;
using Constellation.Core.Shared;
using Constellation.Core.ValueObjects;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetStudentAwardStatisticsQueryHandler
    : IQueryHandler<GetStudentAwardStatisticsQuery, List<StudentAwardStatisticsResponse>>
{
    private readonly IStudentRepository _studentRepository;
    private readonly IStudentAwardRepository _awardRepository;
    private readonly ILogger _logger;

    public GetStudentAwardStatisticsQueryHandler(
        IStudentRepository studentRepository,
        IStudentAwardRepository awardRepository,
        Serilog.ILogger logger)
    {
        _studentRepository = studentRepository;
        _awardRepository = awardRepository;
        _logger = logger.ForContext<GetStudentAwardStatisticsQuery>();
    }

    public async Task<Result<List<StudentAwardStatisticsResponse>>> Handle(GetStudentAwardStatisticsQuery request, CancellationToken cancellationToken)
    {
        List<StudentAwardStatisticsResponse> results = new();

        var students = await _studentRepository.GetCurrentStudentsWithSchool(cancellationToken);

        if (students is null)
        {
            return results;
        }

        foreach (var student in students)
        {
            var nameRequest = Name.Create(student.FirstName, string.Empty, student.LastName);

            if (nameRequest.IsFailure)
                continue;

            var awards = await _awardRepository.GetByStudentId(student.StudentId, cancellationToken);

            if (request.FromDate.HasValue)
            {
                var fromDate = request.FromDate.Value.ToDateTime(TimeOnly.MinValue);

                awards = awards.Where(award => award.AwardedOn >= fromDate).ToList();
            }

            if (request.ToDate.HasValue)
            {
                var toDate = request.ToDate.Value.ToDateTime(TimeOnly.MaxValue);

                awards = awards.Where(award => award.AwardedOn <= toDate).ToList();
            }

            decimal astras = awards.Count(award => award.Type == StudentAward.Astra);
            decimal stellars = awards.Count(award => award.Type == StudentAward.Stellar);
            decimal galaxies = awards.Count(award => award.Type == StudentAward.Galaxy);
            decimal universals = awards.Count(award => award.Type == StudentAward.Universal);

            results.Add(new(
                student.StudentId,
                nameRequest.Value,
                student.CurrentGrade,
                astras,
                stellars,
                galaxies,
                universals,
                Math.Floor(astras / 5),
                Math.Floor(astras / 25),
                Math.Floor(astras / 125)));
        }

        return results;
    }
}
