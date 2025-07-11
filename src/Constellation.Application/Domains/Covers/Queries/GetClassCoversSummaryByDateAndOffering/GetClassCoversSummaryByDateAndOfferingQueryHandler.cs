namespace Constellation.Application.Domains.Covers.Queries.GetClassCoversSummaryByDateAndOffering;

using Abstractions.Messaging;
using Core.Abstractions.Repositories;
using Core.Models.Casuals;
using Core.Models.Covers;
using Core.Models.Covers.Enums;
using Core.Models.Covers.Repositories;
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

internal sealed class GetClassCoversSummaryByDateAndOfferingQueryHandler
    : IQueryHandler<GetClassCoversSummaryByDateAndOfferingQuery, List<ClassCoverSummaryByDateAndOfferingResponse>>
{
    private readonly ICoverRepository _coverRepository;
    private readonly ICasualRepository _casualRepository;
    private readonly IStaffRepository _staffRepository;

    public GetClassCoversSummaryByDateAndOfferingQueryHandler(
        ICoverRepository coverRepository,
        ICasualRepository casualRepository,
        IStaffRepository staffRepository)
    {
        _coverRepository = coverRepository;
        _casualRepository = casualRepository;
        _staffRepository = staffRepository;
    }

    public async Task<Result<List<ClassCoverSummaryByDateAndOfferingResponse>>> Handle(GetClassCoversSummaryByDateAndOfferingQuery request, CancellationToken cancellationToken)
    {
        List<ClassCoverSummaryByDateAndOfferingResponse> returnData = new();

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

            ClassCoverSummaryByDateAndOfferingResponse entry = new(cover.CreatedAt, teacherName, cover.TeacherType);

            returnData.Add(entry);
        }

        return returnData;
    }
}

