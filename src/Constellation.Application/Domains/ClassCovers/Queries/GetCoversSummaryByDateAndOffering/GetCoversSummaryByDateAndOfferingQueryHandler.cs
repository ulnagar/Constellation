namespace Constellation.Application.Domains.ClassCovers.Queries.GetCoversSummaryByDateAndOffering;

using Abstractions.Messaging;
using Core.Abstractions.Repositories;
using Core.Models.Identifiers;
using Core.Models.StaffMembers.Repositories;
using Core.Shared;
using Core.ValueObjects;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetCoversSummaryByDateAndOfferingQueryHandler
    : IQueryHandler<GetCoversSummaryByDateAndOfferingQuery, List<CoverSummaryByDateAndOfferingResponse>>
{
    private readonly IClassCoverRepository _classCoverRepository;
    private readonly ICasualRepository _casualRepository;
    private readonly IStaffRepository _staffRepository;

    public GetCoversSummaryByDateAndOfferingQueryHandler(
        IClassCoverRepository classCoverRepository,
        ICasualRepository casualRepository,
        IStaffRepository staffRepository)
    {
        _classCoverRepository = classCoverRepository;
        _casualRepository = casualRepository;
        _staffRepository = staffRepository;
    }

    public async Task<Result<List<CoverSummaryByDateAndOfferingResponse>>> Handle(GetCoversSummaryByDateAndOfferingQuery request, CancellationToken cancellationToken)
    {
        List<CoverSummaryByDateAndOfferingResponse> returnData = new();

        var covers = await _classCoverRepository
            .GetAllForDateAndOfferingId(
                request.CoverDate,
                request.OfferingId,
                cancellationToken);

        if (covers.Count == 0)
        {
            return returnData;
        }

        foreach (var cover in covers)
        {
            string teacherName = string.Empty;

            if (cover.TeacherType == CoverTeacherType.Casual)
            {
                var casual = await _casualRepository.GetById(CasualId.FromValue(Guid.Parse(cover.TeacherId)), cancellationToken);

                if (casual is not null)
                {
                    teacherName = casual.DisplayName;
                }
            }
            else
            {
                var teacher = await _staffRepository.GetById(cover.TeacherId, cancellationToken);

                if (teacher is not null)
                {
                    teacherName = teacher.DisplayName;
                }
            }

            var entry = new CoverSummaryByDateAndOfferingResponse(cover.CreatedAt, teacherName, cover.TeacherType.Value);

            returnData.Add(entry);
        }

        return returnData;
    }
}

