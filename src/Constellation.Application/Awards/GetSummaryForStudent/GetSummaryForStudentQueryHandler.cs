namespace Constellation.Application.Awards.GetSummaryForStudent;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Models.Attachments.Repository;
using Constellation.Core.Shared;
using Core.Models;
using Core.Models.Awards;
using Core.Models.StaffMembers.Repositories;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetSummaryForStudentQueryHandler
    : IQueryHandler<GetSummaryForStudentQuery, StudentAwardSummaryResponse>
{
    private readonly IStudentAwardRepository _awardRepository;
    private readonly IStaffRepository _staffRepository;
    private readonly IAttachmentRepository _fileRepository;

    public GetSummaryForStudentQueryHandler(
        IStudentAwardRepository awardRepository,
        IStaffRepository staffRepository,
        IAttachmentRepository fileRepository)
    {
        _awardRepository = awardRepository;
        _staffRepository = staffRepository;
        _fileRepository = fileRepository;
    }

    public async Task<Result<StudentAwardSummaryResponse>> Handle(GetSummaryForStudentQuery request, CancellationToken cancellationToken)
    {
        List<StudentAward> data = await _awardRepository.GetByStudentId(request.StudentId, cancellationToken);

        if (data is null)
            return new StudentAwardSummaryResponse(0, 0, 0, 0, new());

        List<StudentAward> recentAwards = data.OrderByDescending(award => award.AwardedOn).Take(10).ToList();
        List<StudentAwardSummaryResponse.StudentAwardResponse> recent = new();

        foreach (StudentAward entry in recentAwards)
        {
            string teacherName = string.Empty;

            if (!string.IsNullOrEmpty(entry.TeacherId))
            {
                Staff teacher = await _staffRepository.GetById(entry.TeacherId, cancellationToken);

                if (teacher is not null)
                    teacherName = teacher.DisplayName;
            }

            bool hasCertificate = await _fileRepository.DoesAwardCertificateExistInDatabase(entry.Id.ToString(), cancellationToken);

            recent.Add(new(
                entry.Id,
                entry.Type,
                entry.AwardedOn,
                teacherName,
                entry.Reason,
                entry.IncidentId,
                hasCertificate));
        }

        StudentAwardSummaryResponse summary = new(
            data.Count(award => award.Type == "Astra Award"),
            data.Count(award => award.Type == "Stellar Award"),
            data.Count(award => award.Type == "Galaxy Medal"),
            data.Count(award => award.Type == "Aurora Universal Achiever"),
            recent);

        return summary;
    }
}
