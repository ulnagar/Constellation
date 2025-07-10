namespace Constellation.Application.Domains.Covers.Queries.GetCoversSummaryByDateAndOffering;

using Abstractions.Messaging;
using Constellation.Core.Models.Covers.Repositories;
using Core.Abstractions.Repositories;
using Core.Models.Casuals;
using Core.Models.Covers;
using Core.Models.Covers.Enums;
using Core.Models.Identifiers;
using Core.Models.StaffMembers;
using Core.Models.StaffMembers.Identifiers;
using Core.Models.StaffMembers.Repositories;
using Core.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetCoversSummaryByDateAndOfferingQueryHandler
    : IQueryHandler<GetCoversSummaryByDateAndOfferingQuery, List<CoverSummaryByDateAndOfferingResponse>>
{
    private readonly ICoverRepository _coverRepository;
    private readonly ICasualRepository _casualRepository;
    private readonly IStaffRepository _staffRepository;

    public GetCoversSummaryByDateAndOfferingQueryHandler(
        ICoverRepository coverRepository,
        ICasualRepository casualRepository,
        IStaffRepository staffRepository)
    {
        _coverRepository = coverRepository;
        _casualRepository = casualRepository;
        _staffRepository = staffRepository;
    }

    public async Task<Result<List<CoverSummaryByDateAndOfferingResponse>>> Handle(GetCoversSummaryByDateAndOfferingQuery request, CancellationToken cancellationToken)
    {
        List<CoverSummaryByDateAndOfferingResponse> returnData = new();

        List<Cover> covers = await _coverRepository
            .GetAllForDateAndOfferingId(
                request.CoverDate,
                request.OfferingId,
                cancellationToken);

        List<ClassCover> classCovers = covers
            .OfType<ClassCover>()
            .ToList();

        if (classCovers.Count == 0)
        {
            return returnData;
        }

        foreach (ClassCover cover in classCovers)
        {
            string teacherName = string.Empty;

            if (cover.TeacherType == CoverTeacherType.Casual)
            {
                Casual casual = await _casualRepository.GetById(CasualId.FromValue(Guid.Parse(cover.TeacherId)), cancellationToken);

                if (casual is not null)
                {
                    teacherName = casual.Name.DisplayName;
                }
            }
            else
            {
                StaffId staffId = StaffId.FromValue(Guid.Parse(cover.TeacherId));

                StaffMember teacher = staffId == StaffId.Empty
                    ? null
                    : await _staffRepository.GetById(staffId, cancellationToken);

                if (teacher is not null)
                    teacherName = teacher.Name.DisplayName;
            }

            CoverSummaryByDateAndOfferingResponse entry = new(cover.CreatedAt, teacherName, CoverType.ClassCover, cover.TeacherType);

            returnData.Add(entry);
        }

        return returnData;
    }
}

