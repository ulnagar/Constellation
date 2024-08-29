namespace Constellation.Application.Awards.GetNominationPeriod;

using Abstractions.Messaging;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Models.Awards;
using Constellation.Core.Models.Students;
using Constellation.Core.Models.Students.Repositories;
using Core.Errors;
using Core.Shared;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetNominationPeriodRequestHandler
    : IQueryHandler<GetNominationPeriodRequest, NominationPeriodDetailResponse>
{
    private readonly IAwardNominationRepository _nominationRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly ILogger _logger;

    public GetNominationPeriodRequestHandler(
        IAwardNominationRepository nominationRepository,
        IStudentRepository studentRepository,
        ILogger logger)
    {
        _nominationRepository = nominationRepository;
        _studentRepository = studentRepository;
        _logger = logger.ForContext<GetNominationPeriodRequest>();
    }

    public async Task<Result<NominationPeriodDetailResponse>> Handle(GetNominationPeriodRequest request, CancellationToken cancellationToken)
    {
        NominationPeriod period = await _nominationRepository.GetById(request.PeriodId, cancellationToken);

        if (period is null)
        {
            _logger.Warning("Could not find Award Nomination Period with Id {id}", request.PeriodId);

            return Result.Failure<NominationPeriodDetailResponse>(DomainErrors.Awards.NominationPeriod.NotFound(request.PeriodId));
        }

        List<NominationPeriodDetailResponse.NominationResponse> nominations = new();

        foreach (Nomination nomination in period.Nominations)
        {
            if (nomination.IsDeleted)
                continue;

            Student student = await _studentRepository.GetById(nomination.StudentId, cancellationToken);

            if (student is null)
            {
                _logger
                    .ForContext(nameof(Nomination), nomination, true)
                    .Warning("Could not find student with {id} when attempting to convert award nomination", nomination.StudentId);

                continue;
            }

            NominationPeriodDetailResponse.NominationResponse entry = new(
                nomination.Id,
                student.Name,
                nomination.AwardType,
                nomination.GetDescription(),
                nomination.CreatedBy);

            nominations.Add(entry);
        }

        NominationPeriodDetailResponse response = new(
            period.Name,
            period.LockoutDate,
            period.IncludedGrades.Select(entry => entry.Grade).ToList(),
            nominations);

        return response;
    }
}
