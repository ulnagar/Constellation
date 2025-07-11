﻿namespace Constellation.Application.Domains.StaffMembers.Queries.GetStaffForSelectionList;

using Abstractions.Messaging;
using Core.Models.StaffMembers.Repositories;
using Core.Shared;
using Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetStaffForSelectionListQueryHandler
    : IQueryHandler<GetStaffForSelectionListQuery, List<StaffSelectionListResponse>>
{
    private readonly IStaffRepository _staffRepository;

    public GetStaffForSelectionListQueryHandler(
        IStaffRepository staffRepository)
    {
        _staffRepository = staffRepository;
    }

    public async Task<Result<List<StaffSelectionListResponse>>> Handle(GetStaffForSelectionListQuery request, CancellationToken cancellationToken)
    {
        List<StaffSelectionListResponse> returnData = new();

        var staff = await _staffRepository.GetAllActive(cancellationToken);

        if (staff.Count == 0)
            return returnData;

        foreach (var member in staff)
        {
            returnData.Add(new StaffSelectionListResponse(
                member.Id,
                member.EmployeeId,
                member.Name));
        }

        return returnData;
    }
}
