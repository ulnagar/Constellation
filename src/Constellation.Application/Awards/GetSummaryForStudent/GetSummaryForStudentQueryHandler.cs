namespace Constellation.Application.Awards.GetSummaryForStudent;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Shared;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetSummaryForStudentQueryHandler
    : IQueryHandler<GetSummaryForStudentQuery, StudentAwardSummaryResponse>
{
    private readonly IStudentAwardRepository _awardRepository;
    private readonly IStaffRepository _staffRepository;
    private readonly IStoredFileRepository _fileRepository;

    public GetSummaryForStudentQueryHandler(
        IStudentAwardRepository awardRepository,
        IStaffRepository staffRepository,
        IStoredFileRepository fileRepository)
    {
        _awardRepository = awardRepository;
        _staffRepository = staffRepository;
        _fileRepository = fileRepository;
    }

    public async Task<Result<StudentAwardSummaryResponse>> Handle(GetSummaryForStudentQuery request, CancellationToken cancellationToken)
    {
        var data = await _awardRepository.GetByStudentId(request.StudentId, cancellationToken);

        if (data is null)
            return new StudentAwardSummaryResponse(0, 0, 0, 0, new());

        var recentAwards = data.OrderByDescending(award => award.AwardedOn).Take(10).ToList();
        List<StudentAwardSummaryResponse.StudentAwardResponse> recent = new();

        foreach (var entry in recentAwards)
        {
            string teacherName = string.Empty;

            if (!string.IsNullOrEmpty(entry.TeacherId))
            {
                var teacher = await _staffRepository.GetById(entry.TeacherId, cancellationToken);

                if (teacher is not null)
                    teacherName = teacher.DisplayName;
            }

            var hasCertificate = await _fileRepository.DoesAwardCertificateExistInDatabase(entry.Id.ToString(), cancellationToken);

            recent.Add(new(
                entry.Id,
                entry.Type,
                entry.AwardedOn,
                teacherName,
                entry.Reason,
                entry.IncidentId,
                hasCertificate));
        }

        var summary = new StudentAwardSummaryResponse(
            data.Count(award => award.Type == "Astra Award"),
            data.Count(award => award.Type == "Stellar Award"),
            data.Count(award => award.Type == "Galaxy Medal"),
            data.Count(award => award.Type == "Aurora Universal Achiever"),
            recent);

        return summary;
    }
}
