namespace Constellation.Infrastructure.Features.Portal.School.Home.Queries;

using Application.Abstractions.Messaging;
using Application.StaffMembers.Models;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.StaffMembers.GetStaffFromSchool;
using Core.Models;
using Core.Shared;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public class GetStaffFromSchoolForSelectionQueryHandler 
    : IQueryHandler<GetStaffFromSchoolQuery, List<StaffSelectionListResponse>>
{
    private readonly IStaffRepository _staffRepository;

    public GetStaffFromSchoolForSelectionQueryHandler(
        IStaffRepository staffRepository)
    {
        _staffRepository = staffRepository;
    }

    public async Task<Result<List<StaffSelectionListResponse>>> Handle(GetStaffFromSchoolQuery request, CancellationToken cancellationToken)
    {
        List<StaffSelectionListResponse> response = new();

        List<Staff> staff = await _staffRepository.GetActiveFromSchool(request.SchoolCode, cancellationToken);

        foreach (Staff member in staff)
        {
            response.Add(new(
                member.StaffId,
                member.FirstName,
                member.LastName));
        }

        return response;
    }
}