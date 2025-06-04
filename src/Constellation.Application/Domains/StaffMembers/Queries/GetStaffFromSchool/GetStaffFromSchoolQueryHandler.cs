namespace Constellation.Application.Domains.StaffMembers.Queries.GetStaffFromSchool;

using Abstractions.Messaging;
using Core.Models.StaffMembers;
using Core.Models.StaffMembers.Repositories;
using Core.Shared;
using Models;
using Serilog;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetStaffFromSchoolQueryHandler 
    : IQueryHandler<GetStaffFromSchoolQuery, List<StaffSelectionListResponse>>
{
    private readonly IStaffRepository _staffRepository;
    private readonly ILogger _logger;

    public GetStaffFromSchoolQueryHandler(
        IStaffRepository staffRepository,
        ILogger logger)
    {
        _staffRepository = staffRepository;
        _logger = logger.ForContext<GetStaffFromSchoolQuery>();
    }

    public async Task<Result<List<StaffSelectionListResponse>>> Handle(GetStaffFromSchoolQuery request, CancellationToken cancellationToken)
    {
        List<StaffSelectionListResponse> response = new();
        
        List<StaffMember> staff = await _staffRepository.GetActiveFromSchool(request.SchoolCode, cancellationToken);

        foreach (StaffMember member in staff)
        {
            response.Add(new(
                member.Id,
                member.Name));
        }

        return response;
    }
}